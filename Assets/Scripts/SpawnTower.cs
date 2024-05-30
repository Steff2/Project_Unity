using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.SceneManagement;

namespace testCases
{
    public class SpawnTower : ComponentSystem
    {
        private float YCoord = 0;
        private float XCoord = -30;
        private float XCoord2 = 0;
        private float XCoord3 = 30;
        private float ZCoord = 0;
        private bool Spawning = true;

        private int cubeNumber = 1;

        protected override void OnUpdate()
        {
            //Workaround for ECS, without it it would be called in every Scene
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Tower"))
            {


                if (Spawning == true)
                {
                    //Y-Coordinate Loop 
                    for (var s = 0; s <= 9; s++)
                    {
                        YCoord = s + 0.5f;
                        //X-Coordinate Loop
                        for (var x = 0; x <= 5; x++)
                        {
                            var X = XCoord + x;
                            //Front wall
                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {
                                var highestTower = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(highestTower,
                                    new Translation {Value = new float3(X, YCoord, 0)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(highestTower);
                                EntityManager.SetComponentData(highestTower, new SceneCubeComponent
                                {
                                    sceneChar = 'E',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                            //Back wall
                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {
                                var highestTower = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(highestTower,
                                    new Translation {Value = new float3(X, YCoord, 5)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(highestTower);
                                EntityManager.SetComponentData(highestTower, new SceneCubeComponent
                                {
                                    sceneChar = 'E',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                        }

                        //Z-Coordinate for side walls
                        for (var z = 1; z <= 4; z++)
                        {
                            ZCoord = z;
                            //Left wall
                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {
                                var highestTower = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(highestTower,
                                    new Translation {Value = new float3(XCoord, YCoord, ZCoord)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(highestTower);
                                EntityManager.SetComponentData(highestTower, new SceneCubeComponent
                                {
                                    sceneChar = 'E',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                            //Right wall
                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {
                                var highestTower = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(highestTower,
                                    new Translation {Value = new float3(XCoord + 5, YCoord, ZCoord)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(highestTower);
                                EntityManager.SetComponentData(highestTower, new SceneCubeComponent
                                {
                                    sceneChar = 'E',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                        }
                    }

                    //Y-Coordinate Loop
                    for (var s = 0; s <= 7; s++)
                    {
                        YCoord = s + 0.5f;
                        //X-Coordinate Loop
                        for (var x = 0; x <= 5; x++)
                        {
                            var X = XCoord2 + x;
                            //Front wall
                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {
                                var secondHighestTower = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(secondHighestTower,
                                    new Translation {Value = new float3(X, YCoord, 0)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(secondHighestTower);
                                EntityManager.SetComponentData(secondHighestTower, new SceneCubeComponent
                                {
                                    sceneChar = 'E',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                            //Back wall
                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {
                                var secondHighestTower = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(secondHighestTower,
                                    new Translation {Value = new float3(X, YCoord, 5)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(secondHighestTower);
                                EntityManager.SetComponentData(secondHighestTower, new SceneCubeComponent
                                {
                                    sceneChar = 'E',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                        }

                        //Z-Coordinate for side walls
                        for (var z = 1; z <= 4; z++)
                        {
                            ZCoord = z;
                            //Left wall
                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {
                                var secondHighestTower = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(secondHighestTower,
                                    new Translation {Value = new float3(XCoord2, YCoord, ZCoord)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(secondHighestTower);
                                EntityManager.SetComponentData(secondHighestTower, new SceneCubeComponent
                                {
                                    sceneChar = 'E',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                            //Right wall
                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {
                                var secondHighestTower = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(secondHighestTower,
                                    new Translation {Value = new float3(XCoord2 + 5, YCoord, ZCoord)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(secondHighestTower);
                                EntityManager.SetComponentData(secondHighestTower, new SceneCubeComponent
                                {
                                    sceneChar = 'E',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                        }
                    }

                    //Y-Coordinate Loop
                    for (var s = 0; s <= 1; s++)
                    {
                        YCoord = s + 0.5f;
                        //X-Coordinate Loop
                        for (var x = 0; x <= 5; x++)
                        {
                            var X = XCoord3 + x;
                            //Front wall
                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {
                                var thirdHighestTower = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(thirdHighestTower,
                                    new Translation {Value = new float3(X, YCoord, 0)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(thirdHighestTower);
                                EntityManager.SetComponentData(thirdHighestTower, new SceneCubeComponent
                                {
                                    sceneChar = 'E',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                            //Back wall
                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {
                                var thirdHighestTower = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(thirdHighestTower,
                                    new Translation {Value = new float3(X, YCoord, 5)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(thirdHighestTower);
                                EntityManager.SetComponentData(thirdHighestTower, new SceneCubeComponent
                                {
                                    sceneChar = 'E',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                        }

                        //Z-Coordinate for side walls
                        for (var z = 1; z <= 4; z++)
                        {
                            ZCoord = z;
                            //Left wall
                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {
                                var thirdHighestTower = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(thirdHighestTower,
                                    new Translation {Value = new float3(XCoord3, YCoord, ZCoord)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(thirdHighestTower);
                                EntityManager.SetComponentData(thirdHighestTower, new SceneCubeComponent
                                {
                                    sceneChar = 'E',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                            //Right wall
                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {
                                var thirdHighestTower = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(thirdHighestTower,
                                    new Translation {Value = new float3(XCoord3 + 5, YCoord, ZCoord)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(thirdHighestTower);
                                EntityManager.SetComponentData(thirdHighestTower, new SceneCubeComponent
                                {
                                    sceneChar = 'E',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                        }
                    }
                }

                Spawning = false;
            }
        }
    }
}

