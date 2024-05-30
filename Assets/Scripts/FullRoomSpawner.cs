using System.IO;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace testCases
{
    /// <summary>
    /// Variablen die getestet oder verändert werden können:
    /// spawnTimer - reguliert die Dauer bis zu dem Erstellen des nächsten Cubes
    /// exportTimer - reguliert der Zeit zwischen dem Sammeln von Positionsdaten 0.15 heißt 0.15 Sekunden Zeit zwischen dem Sammeln
    /// exitTime - reguliert die Dauer bis die Szene endet und der export passiert
    /// Startposition der Cubes kann in der respektiven Schleife geändert werden
    /// </summary>

    //Contains FullRoom and FullRoomSmall spawn system
    public class FullRoomSpawner : ComponentSystem
    {
        private float spawnTimer;
        private float spawnTimertemp;

        private float exportTimer;

        //Cube Identifier
        private int cubeNumber;

        //Spawnrates determined in editor and handed over in runtime
        public bool fastestSpawning = false;
        public bool fasterSpawning = false;
        public bool slowSpawning = false;


        private bool setup = false;

        private bool smallCubesSpawned = false;

        protected override void OnUpdate()
        {

            //Workaround for ECS, without it it would be called in every Scene
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("FullRoom"))
            {
                PositionAndKollision.exitTime = 40f;

                PositionAndKollision.sceneName = "FullRoom";
                if (setup == false)
                {
                    //Get all the options used in the Editor
                    Entities.ForEach((ref SpawnAndExportRateAuthoringComponent spawnratecomponent) =>
                    {
                        fastestSpawning = spawnratecomponent.fastestSpawn;
                        fasterSpawning = spawnratecomponent.fasterSpawn;
                        slowSpawning = spawnratecomponent.slowestSpawn;

                        if (spawnratecomponent.testCase1)
                        {
                            PositionAndKollision.testCase = 1;
                        }

                        if (spawnratecomponent.testCase2)
                        {
                            PositionAndKollision.testCase = 2;
                        }

                        if (spawnratecomponent.testCase3)
                        {
                            PositionAndKollision.testCase = 3;
                        }
                    });

                    //Set options for the scene
                    if (fastestSpawning)
                    {
                        spawnTimer = 0.125f;
                        spawnTimertemp = spawnTimer;

                        PositionAndKollision.spawnRate = $"/Spawnrate_{spawnTimer}";
                    }

                    if (fasterSpawning)
                    {
                        spawnTimer = 0.25f;
                        spawnTimertemp = spawnTimer;

                        PositionAndKollision.spawnRate = $"/Spawnrate_{spawnTimer}";
                    }

                    if (slowSpawning)
                    {
                        spawnTimer = 0.5f;
                        spawnTimertemp = spawnTimer;

                        PositionAndKollision.spawnRate = $"/Spawnrate_{spawnTimer}";
                    }

                    setup = true;
                }

                spawnTimertemp -= Time.DeltaTime;
                exportTimer -= Time.DeltaTime;

                //Spawns Cubes in a regular interval
                if (spawnTimertemp <= 0f && (fastestSpawning || fasterSpawning || slowSpawning))
                {
                    cubeNumber++;
                    spawnTimertemp = spawnTimer;
                    //Spawns Cubes in a regular interval
                    Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                    {

                        var spawnedEntity = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);
                        EntityManager.AddComponent<SceneCubeComponent>(spawnedEntity);
                        EntityManager.SetComponentData(spawnedEntity, new SceneCubeComponent
                        {
                            sceneChar = 'A',
                            number = cubeNumber
                        });

                        EntityManager.SetComponentData(spawnedEntity, new Translation
                        {
                            Value = new float3(0, 0.5f, 0)
                        });
                    });
                }

                var elapsedTime = (float) Time.ElapsedTime;

                //Gather data from Cubes
                if (exportTimer <= 0f && (fastestSpawning || fasterSpawning || slowSpawning))
                {
                    exportTimer = 0.15f;

                    Entities
                        .WithAll<PhysicsVelocity>()
                        .ForEach((Entity entity, ref Translation translation, ref SceneCubeComponent sceneCube) =>
                        {
                            PositionAndKollision.Position.Add(
                                $"{sceneCube.number},{translation.Value.x},{translation.Value.y},{translation.Value.z},{elapsedTime}\n");
                        });
                }

                //When the test duration is over, call the export function
                if (elapsedTime >= PositionAndKollision.exitTime && PositionAndKollision.exportedPos == false)
                {
                    PositionAndKollision.ExportPos();
                }

            }

            //Workaround for ECS, without it it would be called in every Scene
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("FullRoomSmall"))
            {
                PositionAndKollision.exitTime = 10f;

                PositionAndKollision.sceneName = "FullRoomSmall";
                if (setup == false)
                {
                    //Get all the options used in the Editor
                    Entities.ForEach((ref SpawnAndExportRateAuthoringComponent spawnratecomponent) =>
                    {
                        PositionAndKollision.spawnRate = "";

                        if (spawnratecomponent.testCase1)
                        {
                            PositionAndKollision.testCase = 1;
                        }

                        if (spawnratecomponent.testCase2)
                        {
                            PositionAndKollision.testCase = 2;
                        }

                        if (spawnratecomponent.testCase3)
                        {
                            PositionAndKollision.testCase = 3;
                        }
                    });

                    setup = true;
                }

                exportTimer -= Time.DeltaTime;

                if (!smallCubesSpawned)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        cubeNumber++;
                        //Spawns 5 Cubes
                        Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                        {

                            var spawnedEntity = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);
                            EntityManager.AddComponent<SceneCubeComponent>(spawnedEntity);
                            EntityManager.SetComponentData(spawnedEntity, new SceneCubeComponent
                            {
                                sceneChar = 'F',
                                number = cubeNumber
                            });

                            EntityManager.SetComponentData(spawnedEntity, new Translation
                            {
                                Value = new float3(0, 0.5f, 0)
                            });
                        });
                    }

                    smallCubesSpawned = true;
                }

                var elapsedTime = (float)Time.ElapsedTime;

                //Gather data from Cubes
                if (exportTimer <= 0f)
                {
                    exportTimer = 0.15f;

                    Entities
                        .WithAll<PhysicsVelocity>()
                        .ForEach((Entity entity, ref Translation translation, ref SceneCubeComponent sceneCube) =>
                        {
                            PositionAndKollision.Position.Add(
                                $"{sceneCube.number},{translation.Value.x},{translation.Value.y},{translation.Value.z},{elapsedTime}\n");
                        });
                }

                //When the test duration is over, call the export function
                if (elapsedTime >= PositionAndKollision.exitTime && PositionAndKollision.exportedPos == false)
                {
                    PositionAndKollision.ExportPos();
                }
            }
        }
    }
}