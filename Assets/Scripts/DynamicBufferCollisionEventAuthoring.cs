using System;
using System.IO;
using testCases;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

namespace Unity.Physics.Stateful
{
    // Describes the colliding state.
    // CollidingState in StatefulCollisionEvent is set to:
    //    1) EventCollidingState.Enter, when 2 bodies are colliding in the current frame,
    //    but they did not collide in the previous frame
    //    2) EventCollidingState.Stay, when 2 bodies are colliding in the current frame,
    //    and they did collide in the previous frame
    //    3) EventCollidingState.Exit, when 2 bodies are NOT colliding in the current frame,
    //    but they did collide in the previous frame
    public enum EventCollidingState : byte
    {
        Enter,
        Stay,
        Exit
    }

    // Collision Event that is stored inside a DynamicBuffer
    public struct StatefulCollisionEvent : IBufferElementData, IComparable<StatefulCollisionEvent>
    {
        internal BodyIndexPair BodyIndices;
        internal EntityPair Entities;
        internal ColliderKeyPair ColliderKeys;

        // Only if CalculateDetails is checked on PhysicsCollisionEventBuffer of selected entity,
        // this field will have valid value, otherwise it will be zero initialized
        internal Details CollisionDetails;

        public EventCollidingState CollidingState;

        // Normal is pointing from EntityB to EntityA
        public float3 Normal;

        public StatefulCollisionEvent(Entity entityA, Entity entityB, int bodyIndexA, int bodyIndexB,
                                      ColliderKey colliderKeyA, ColliderKey colliderKeyB, float3 normal)
        {
            Entities = new EntityPair
            {
                EntityA = entityA,
                EntityB = entityB
            };
            BodyIndices = new BodyIndexPair
            {
                BodyIndexA = bodyIndexA,
                BodyIndexB = bodyIndexB
            };
            ColliderKeys = new ColliderKeyPair
            {
                ColliderKeyA = colliderKeyA,
                ColliderKeyB = colliderKeyB
            };
            Normal = normal;
            CollidingState = default;
            CollisionDetails = default;
        }

        public Entity EntityA => Entities.EntityA;
        public Entity EntityB => Entities.EntityB;
        public ColliderKey ColliderKeyA => ColliderKeys.ColliderKeyA;
        public ColliderKey ColliderKeyB => ColliderKeys.ColliderKeyB;
        public int BodyIndexA => BodyIndices.BodyIndexA;
        public int BodyIndexB => BodyIndices.BodyIndexB;

        // This struct describes additional, optional, details about collision of 2 bodies
        public struct Details
        {
            internal int IsValid;

            // If 1, then it is a vertex collision
            // If 2, then it is an edge collision
            // If 3 or more, then it is a face collision
            public int NumberOfContactPoints;

            // Estimated impulse applied
            public float EstimatedImpulse;
            // Average contact point position
            public float3 AverageContactPointPosition;

            public Details(int numberOfContactPoints, float estimatedImpulse, float3 averageContactPosition)
            {
                IsValid = 1;
                NumberOfContactPoints = numberOfContactPoints;
                EstimatedImpulse = estimatedImpulse;
                AverageContactPointPosition = averageContactPosition;
            }
        }

        // Returns the other entity in EntityPair, if provided with other one
        public Entity GetOtherEntity(Entity entity)
        {
            Assert.IsTrue((entity == EntityA) || (entity == EntityB));
            int2 indexAndVersion = math.select(new int2(EntityB.Index, EntityB.Version),
                new int2(EntityA.Index, EntityA.Version), entity == EntityB);
            return new Entity
            {
                Index = indexAndVersion[0],
                Version = indexAndVersion[1]
            };
        }

        // Returns the normal pointing from passed entity to the other one in pair
        public float3 GetNormalFrom(Entity entity)
        {
            Assert.IsTrue((entity == EntityA) || (entity == EntityB));
            return math.select(-Normal, Normal, entity == EntityB);
        }

        public bool TryGetDetails(out Details details)
        {
            details = CollisionDetails;
            return CollisionDetails.IsValid != 0;
        }

        public int CompareTo(StatefulCollisionEvent other)
        {
            var cmpResult = EntityA.CompareTo(other.EntityA);
            if (cmpResult != 0)
            {
                return cmpResult;
            }

            cmpResult = EntityB.CompareTo(other.EntityB);
            if (cmpResult != 0)
            {
                return cmpResult;
            }

            if (ColliderKeyA.Value != other.ColliderKeyA.Value)
            {
                return ColliderKeyA.Value < other.ColliderKeyA.Value ? -1 : 1;
            }

            if (ColliderKeyB.Value != other.ColliderKeyB.Value)
            {
                return ColliderKeyB.Value < other.ColliderKeyB.Value ? -1 : 1;
            }

            return 0;
        }
    }

    public struct CollisionEventBuffer : IComponentData
    {
        public int CalculateDetails;
    }

    // This system converts stream of CollisionEvents to StatefulCollisionEvents that are stored in a Dynamic Buffer.
    // In order for CollisionEvents to be transformed to StatefulCollisionEvents and stored in a Dynamic Buffer, it is required to:
    //    1) Tick Raises Collision Events on PhysicsShapeAuthoring on the entity that should raise collision events
    //    2) Add a DynamicBufferCollisionEventAuthoring component to that entity (and select if details should be calculated or not)
    //    3) If this is desired on a Character Controller, tick RaiseCollisionEvents flag on CharacterControllerAuthoring (skip 1) and 2)),
    [UpdateAfter(typeof(StepPhysicsWorld))]
    [UpdateBefore(typeof(EndFramePhysicsSystem))]
    public class CollisionEventConversionSystem : SystemBase
    {

        private float exportTimer;
        private float exitTimer;

        public JobHandle OutDependency => Dependency;

        private StepPhysicsWorld m_StepPhysicsWorld = default;
        private BuildPhysicsWorld m_BuildPhysicsWorld = default;
        private EndFramePhysicsSystem m_EndFramePhysicsSystem = default;
        private EntityQuery m_Query = default;

        private NativeList<StatefulCollisionEvent> m_PreviousFrameCollisionEvents;
        private NativeList<StatefulCollisionEvent> m_CurrentFrameCollisionEvents;

        protected override void OnCreate()
        {
            m_StepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
            m_BuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
            m_EndFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem>();
            m_Query = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(CollisionEventBuffer)
                }
            });

            m_PreviousFrameCollisionEvents = new NativeList<StatefulCollisionEvent>(Allocator.Persistent);
            m_CurrentFrameCollisionEvents = new NativeList<StatefulCollisionEvent>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            m_PreviousFrameCollisionEvents.Dispose();
            m_CurrentFrameCollisionEvents.Dispose();
        }

        protected void SwapCollisionEventState()
        {
            var tmp = m_PreviousFrameCollisionEvents;
            m_PreviousFrameCollisionEvents = m_CurrentFrameCollisionEvents;
            m_CurrentFrameCollisionEvents = tmp;
            m_CurrentFrameCollisionEvents.Clear();
        }

        public static void UpdateCollisionEventState(NativeList<StatefulCollisionEvent> previousFrameCollisionEvents,
            NativeList<StatefulCollisionEvent> currentFrameCollisionEvents, NativeList<StatefulCollisionEvent> resultList)
        {
            int i = 0;
            int j = 0;

            while (i < currentFrameCollisionEvents.Length && j < previousFrameCollisionEvents.Length)
            {
                var currentFrameCollisionEvent = currentFrameCollisionEvents[i];
                var previousFrameCollisionEvent = previousFrameCollisionEvents[j];

                int cmpResult = currentFrameCollisionEvent.CompareTo(previousFrameCollisionEvent);

                // Appears in previous, and current frame, mark it as Stay
                if (cmpResult == 0)
                {
                    currentFrameCollisionEvent.CollidingState = EventCollidingState.Stay;
                    resultList.Add(currentFrameCollisionEvent);
                    i++;
                    j++;
                }
                else if (cmpResult < 0)
                {
                    // Appears in current, but not in previous, mark it as Enter
                    currentFrameCollisionEvent.CollidingState = EventCollidingState.Enter;
                    resultList.Add(currentFrameCollisionEvent);
                    i++;
                }
                else
                {
                    // Appears in previous, but not in current, mark it as Exit
                    previousFrameCollisionEvent.CollidingState = EventCollidingState.Exit;
                    resultList.Add(previousFrameCollisionEvent);
                    j++;
                }
            }

            if (i == currentFrameCollisionEvents.Length)
            {
                while (j < previousFrameCollisionEvents.Length)
                {
                    var collisionEvent = previousFrameCollisionEvents[j++];
                    collisionEvent.CollidingState = EventCollidingState.Exit;
                    resultList.Add(collisionEvent);
                }
            }
            else if (j == previousFrameCollisionEvents.Length)
            {
                while (i < currentFrameCollisionEvents.Length)
                {
                    var collisionEvent = currentFrameCollisionEvents[i++];
                    collisionEvent.CollidingState = EventCollidingState.Enter;
                    resultList.Add(collisionEvent);
                }
            }
        }

        protected static void AddCollisionEventsToDynamicBuffers(NativeList<StatefulCollisionEvent> collisionEventList,
            ref BufferFromEntity<StatefulCollisionEvent> bufferFromEntity, NativeHashMap<Entity, byte> entitiesWithCollisionEventBuffers)
        {
            for (int i = 0; i < collisionEventList.Length; i++)
            {
                var collisionEvent = collisionEventList[i];
                if (entitiesWithCollisionEventBuffers.ContainsKey(collisionEvent.EntityA))
                {
                    bufferFromEntity[collisionEvent.EntityA].Add(collisionEvent);
                }
                if (entitiesWithCollisionEventBuffers.ContainsKey(collisionEvent.EntityB))
                {
                    bufferFromEntity[collisionEvent.EntityB].Add(collisionEvent);
                }
            }
        }

        protected override void OnUpdate()
        {
            if (m_Query.CalculateEntityCount() == 0)
            {
                return;
            }

            exportTimer -= Time.DeltaTime;

            Dependency = JobHandle.CombineDependencies(m_StepPhysicsWorld.FinalSimulationJobHandle, Dependency);

            Entities
                .WithName("ClearCollisionEventDynamicBuffersJobParallel")
                .WithBurst()
                .WithAll<CollisionEventBuffer>()
                .ForEach((ref DynamicBuffer<StatefulCollisionEvent> buffer, ref SceneCubeComponent sceneCube) =>
                {
                    buffer.Clear();
                }).ScheduleParallel();

            SwapCollisionEventState();

            var previousFrameCollisionEvents = m_PreviousFrameCollisionEvents;
            var currentFrameCollisionEvents = m_CurrentFrameCollisionEvents;

            var collisionEventBufferFromEntity = GetBufferFromEntity<StatefulCollisionEvent>();
            var physicsCollisionEventBufferTags = GetComponentDataFromEntity<CollisionEventBuffer>();

            // Using HashMap since HashSet doesn't exist
            // Setting value type to byte to minimize memory waste
            NativeHashMap<Entity, byte> entitiesWithBuffersMap = new NativeHashMap<Entity, byte>(0, Allocator.TempJob);

            Entities
                .WithName("CollectCollisionBufferJob")
                .WithBurst()
                .WithAll<CollisionEventBuffer>()
                .ForEach(
                    (Entity e, ref DynamicBuffer<StatefulCollisionEvent> buffer, ref SceneCubeComponent sceneCube) =>
                    {
                        entitiesWithBuffersMap.Add(e, 0);
                    }).Schedule();

            var collectCollisionEventsJob = new CollectCollisionEventsJob
            {
                CollisionEvents = currentFrameCollisionEvents,
                PhysicsCollisionEventBufferTags = physicsCollisionEventBufferTags,
                PhysicsWorld = m_BuildPhysicsWorld.PhysicsWorld,
                EntitiesWithBuffersMap = entitiesWithBuffersMap
            };

            Dependency = collectCollisionEventsJob.Schedule(m_StepPhysicsWorld.Simulation,
                ref m_BuildPhysicsWorld.PhysicsWorld, Dependency);

            Job
                .WithName("ConvertCollisionEventStreamToDynamicBufferJob")
                .WithBurst()
                .WithCode(() =>
                {
                    currentFrameCollisionEvents.Sort();

                    var collisionEventsWithStates =
                        new NativeList<StatefulCollisionEvent>(currentFrameCollisionEvents.Length, Allocator.Temp);
                    UpdateCollisionEventState(previousFrameCollisionEvents, currentFrameCollisionEvents,
                        collisionEventsWithStates);
                    AddCollisionEventsToDynamicBuffers(collisionEventsWithStates, ref collisionEventBufferFromEntity,
                        entitiesWithBuffersMap);
                }).Schedule();

            m_EndFramePhysicsSystem.AddInputDependency(Dependency);
            entitiesWithBuffersMap.Dispose(Dependency);

            //IMPORTANT EXPORT STUFF FROM HERE

            //Get all the translation components
            var translations = GetComponentDataFromEntity<Translation>();
            var elapsedTime = (float) Time.ElapsedTime;
            //var sceneCube = GetComponentDataFromEntity<SceneCubeComponent>();

            exitTimer = (float) Time.ElapsedTime;

            //Uniforming export time
            if (exitTimer < PositionAndKollision.exitTime)
            {
                //Export in interval because of too many duplicate entries
                if (exportTimer <= 0f)
                {
                    exportTimer = 0.15f;

                    //Main Loop for export
                    Entities
                        .WithAll<CollisionEventBuffer>()
                        .ForEach((Entity collisionEntity, ref DynamicBuffer<StatefulCollisionEvent> buffer,
                            ref SceneCubeComponent sceneCube) =>
                        {
                            //Get all Entities with the collisioneventbuffer and the SceneCubeComponent
                            //and iterate through all the collisions
                            foreach (var collisionEvent in buffer)
                            {
                                //Set the colliding entity and  calculate the distance between both
                                var otherEntity = collisionEvent.GetOtherEntity(collisionEntity);
                                var distance = Vector3.Distance(
                                    new Vector3(translations[collisionEntity].Value.x,
                                        translations[collisionEntity].Value.y, translations[collisionEntity].Value.z),
                                    new Vector3(translations[otherEntity].Value.x, translations[otherEntity].Value.y,
                                        translations[otherEntity].Value.z));


                                //Case for every scene
                                //Pattern for export per line: 1. Main entity identifier
                                //2. Main entity (X,Y,Z) coordinates
                                //3. Colliding entity (X,Y,Z) coordinates
                                //4. distance between colliding objects
                                //5. elapsed time since the start
                                switch (sceneCube.sceneChar)
                                {
                                    case 'A':

                                        string tempstring1 =
                                            $"{sceneCube.number};{translations[collisionEntity].Value.x};{translations[collisionEntity].Value.y};{translations[collisionEntity].Value.z};{translations[otherEntity].Value.x};{translations[otherEntity].Value.y};{translations[otherEntity].Value.z};{distance};{elapsedTime}\n";

                                        if (PositionAndKollision.duplicateCheckString != tempstring1)
                                        {
                                            PositionAndKollision.duplicateCheckString = tempstring1;
                                            PositionAndKollision.Collision.Add(tempstring1);
                                        }

                                        break;

                                    case 'B':

                                        string tempstring2 =
                                            $"{sceneCube.number};{translations[collisionEntity].Value.x};{translations[collisionEntity].Value.y};{translations[collisionEntity].Value.z};{translations[otherEntity].Value.x};{translations[otherEntity].Value.y};{translations[otherEntity].Value.z};{distance};{elapsedTime}\n";

                                        if (PositionAndKollision.duplicateCheckString != tempstring2)
                                        {
                                            PositionAndKollision.duplicateCheckString = tempstring2;
                                            PositionAndKollision.Collision.Add(tempstring2);
                                        }

                                        break;

                                    case 'C':
                                        //StreamWriter saveFile2 = new StreamWriter("data_Pyramid.txt", true);

                                        //saveFile2.Write($"{sceneCube.number};{translations[collisionEntity].Value.x};{translations[collisionEntity].Value.y};{translations[collisionEntity].Value.z};{translations[otherEntity].Value.x};{translations[otherEntity].Value.y};{translations[otherEntity].Value.z};{distance};{elapsedTime}\n");


                                        //saveFile2.Close();
                                        break;

                                    case 'D':

                                        string tempstring3 =
                                            $"{sceneCube.number};{translations[collisionEntity].Value.x};{translations[collisionEntity].Value.y};{translations[collisionEntity].Value.z};{translations[otherEntity].Value.x};{translations[otherEntity].Value.y};{translations[otherEntity].Value.z};{distance};{elapsedTime}\n";

                                        if (PositionAndKollision.duplicateCheckString != tempstring3)
                                        {
                                            PositionAndKollision.duplicateCheckString = tempstring3;
                                            PositionAndKollision.Collision.Add(tempstring3);
                                        }

                                        break;

                                    case 'E':
                                        //StreamWriter saveFile4 = new StreamWriter("data_Tower.txt", true);

                                        //saveFile4.Write($"{sceneCube.number};{translations[collisionEntity].Value.x};{translations[collisionEntity].Value.y};{translations[collisionEntity].Value.z};{translations[otherEntity].Value.x};{translations[otherEntity].Value.y};{translations[otherEntity].Value.z};{distance};{elapsedTime}\n");


                                        //saveFile4.Close();
                                        break;

                                    case 'F':

                                        string tempstring4 =
                                            $"{sceneCube.number};{translations[collisionEntity].Value.x};{translations[collisionEntity].Value.y};{translations[collisionEntity].Value.z};{translations[otherEntity].Value.x};{translations[otherEntity].Value.y};{translations[otherEntity].Value.z};{distance};{elapsedTime}\n";

                                        if (PositionAndKollision.duplicateCheckString != tempstring4)
                                        {
                                            PositionAndKollision.duplicateCheckString = tempstring4;
                                            PositionAndKollision.Collision.Add(tempstring4);
                                        }

                                        break;

                                }
                            }
                        }).WithoutBurst().Run();
                }
            }

            if (exitTimer >= PositionAndKollision.exitTime && PositionAndKollision.exportedCol == false)
            {
                PositionAndKollision.ExportCol();
            }

            //TO HERE
        }

        [BurstCompile]
        public struct CollectCollisionEventsJob : ICollisionEventsJob
        {
            public NativeList<StatefulCollisionEvent> CollisionEvents;
            public ComponentDataFromEntity<CollisionEventBuffer> PhysicsCollisionEventBufferTags;

            [ReadOnly] public NativeHashMap<Entity, byte> EntitiesWithBuffersMap;
            [ReadOnly] public PhysicsWorld PhysicsWorld;

            public void Execute(CollisionEvent collisionEvent)
            {
                var collisionEventBufferElement = new StatefulCollisionEvent(collisionEvent.EntityA, collisionEvent.EntityB,
                    collisionEvent.BodyIndexA, collisionEvent.BodyIndexB, collisionEvent.ColliderKeyA,
                    collisionEvent.ColliderKeyB, collisionEvent.Normal);

                var calculateDetails = false;

                if (EntitiesWithBuffersMap.ContainsKey(collisionEvent.EntityA))
                {
                    if (PhysicsCollisionEventBufferTags[collisionEvent.EntityA].CalculateDetails != 0)
                    {
                        calculateDetails = true;
                    }
                }

                if (!calculateDetails && EntitiesWithBuffersMap.ContainsKey(collisionEvent.EntityB))
                {
                    if (PhysicsCollisionEventBufferTags[collisionEvent.EntityB].CalculateDetails != 0)
                    {
                        calculateDetails = true;
                    }
                }

                if (calculateDetails)
                {
                    var details = collisionEvent.CalculateDetails(ref PhysicsWorld);
                    collisionEventBufferElement.CollisionDetails = new StatefulCollisionEvent.Details(
                        details.EstimatedContactPointPositions.Length, details.EstimatedImpulse, details.AverageContactPointPosition);
                }

                CollisionEvents.Add(collisionEventBufferElement);
            }
        }
    }

    public class DynamicBufferCollisionEventAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Tooltip("If selected, the details will be calculated in collision event dynamic buffer of this entity")]
        public bool CalculateDetails = false;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var dynamicBufferTag = new CollisionEventBuffer
            {
                CalculateDetails = CalculateDetails ? 1 : 0
            };

            dstManager.AddComponentData(entity, dynamicBufferTag);
            dstManager.AddBuffer<StatefulCollisionEvent>(entity);
        }
    }
}
