
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
    /// spawnPos/spawnPosGround - regulieren die Startpositionen der Cubes
    /// </summary>
    public class CubeSpawner : ComponentSystem
    {
        //Timer of the spawning interval
        private float spawnTimer;

        private float spawnTimertemp;

        //Timer to limit data gathering frequency
        private float exportTimer;


        //Spawnrates determined in editor and handed over in runtime
        public bool fastestSpawning = false;
        public bool fasterSpawning = false;
        public bool slowSpawning = false;

        //Position of Cubes spawned in the air
        private float3 spawnPos = new float3(100f, 40, 0);

        //Position of cubes spawned on the ground
        private float3 spawnPosGround = new float3(-40f, 0.5f, 0);

        //Cube Identifier
        private int cubeNumber = 1;

        private bool setup = false;


        protected override void OnUpdate()
        {

            //Workaround for ECS, without it it would be called in every Scene
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Restitution"))
            {
                PositionAndKollision.exitTime = 40f;

                PositionAndKollision.sceneName = "Restitution";

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
                        spawnTimer = .1f;
                        spawnTimertemp = spawnTimer;

                        PositionAndKollision.spawnRate = $"/Spawnrate_{spawnTimer}";
                    }

                    if (fasterSpawning)
                    {
                        spawnTimer = .125f;
                        spawnTimertemp = spawnTimer;

                        PositionAndKollision.spawnRate = $"/Spawnrate_{spawnTimer}";
                    }

                    if (slowSpawning)
                    {
                        spawnTimer = .15f;
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
                    spawnTimertemp = spawnTimer;
                    Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                    {
                        var spawnedEntity = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                        EntityManager.SetComponentData(spawnedEntity,
                            new Translation {Value = spawnPos}
                        );

                        var spawnedEntityground = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                        EntityManager.SetComponentData(spawnedEntityground,
                            new Translation {Value = spawnPosGround}
                        );

                        EntityManager.AddComponent<SceneCubeComponent>(spawnedEntity);
                        EntityManager.SetComponentData(spawnedEntity, new SceneCubeComponent
                        {
                            sceneChar = 'D',
                            number = cubeNumber,
                        });

                        cubeNumber++;

                        EntityManager.AddComponent<SceneCubeComponent>(spawnedEntityground);
                        EntityManager.SetComponentData(spawnedEntityground, new SceneCubeComponent
                        {
                            sceneChar = 'D',
                            number = cubeNumber,
                        });

                        cubeNumber++;
                    });
                }


                var elapsedTime = (float)Time.ElapsedTime;

                //Gather data from Cubes
                if (exportTimer <= 0f && (fastestSpawning || fasterSpawning || slowSpawning) && elapsedTime < 45f)
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
