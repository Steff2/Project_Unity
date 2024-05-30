using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.SceneManagement;

namespace testCases
{
    public class PyramidSpawner : ComponentSystem
    {
        private float YCoord = 0;
        private float XCoord = -30;
        private float XCoord2 = 0;
        private float XCoord3 = 30;
        private bool Spawning = true;

        private int cubeNumber = 1;

        private int pyramidHeight = 6;

        protected override void OnUpdate()
        {
            //Workaround for ECS, without it it would be called in every Scene
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Pyramid"))
            {


                if (Spawning == true)
                {
                    //First Pyramid (farthest behind)
                    //Y-Coordinate Loop 
                    for (var s = 0; s < pyramidHeight; s++)
                    {
                        YCoord = pyramidHeight - 1 + 0.5f - s;

                        //Center
                        Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                        {
                            var centerEntity = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                            EntityManager.SetComponentData(centerEntity,
                                new Translation {Value = new float3(XCoord, YCoord, 0)}
                            );
                            EntityManager.AddComponent<SceneCubeComponent>(centerEntity);
                            EntityManager.SetComponentData(centerEntity, new SceneCubeComponent
                            {
                                sceneChar = 'C',
                                number = cubeNumber
                            });

                            cubeNumber++;
                        });
                        //X-Coordinate Loop
                        for (var x = 0; x < s; x++)
                        {

                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {

                                var leftSide = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(leftSide,
                                    new Translation {Value = new float3(XCoord, YCoord, -x - 1)}
                                );

                                EntityManager.AddComponent<SceneCubeComponent>(leftSide);
                                EntityManager.SetComponentData(leftSide, new SceneCubeComponent
                                {
                                    sceneChar = 'C',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                                var rightSide = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(rightSide,
                                    new Translation {Value = new float3(XCoord, YCoord, x + 1)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(rightSide);
                                EntityManager.SetComponentData(rightSide, new SceneCubeComponent
                                {
                                    sceneChar = 'C',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                        }
                    }

                    //Second Pyramid
                    //Y-Coordinate Loop 
                    for (var s = 0; s < pyramidHeight - 1; s++)
                    {
                        YCoord = pyramidHeight - 2 + 0.5f - s;

                        //Center
                        Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                        {
                            var centerEntity = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                            EntityManager.SetComponentData(centerEntity,
                                new Translation {Value = new float3(XCoord2, YCoord, 0)}
                            );
                            EntityManager.AddComponent<SceneCubeComponent>(centerEntity);
                            EntityManager.SetComponentData(centerEntity, new SceneCubeComponent
                            {
                                sceneChar = 'C',
                                number = cubeNumber
                            });

                            cubeNumber++;
                        });
                        //X-Coordinate Loop
                        for (var x = 0; x < s; x++)
                        {

                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {

                                var leftSide1 = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(leftSide1,
                                    new Translation {Value = new float3(XCoord2, YCoord, -x - 1)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(leftSide1);
                                EntityManager.SetComponentData(leftSide1, new SceneCubeComponent
                                {
                                    sceneChar = 'C',
                                    number = cubeNumber
                                });

                                var rightSide1 = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(rightSide1,
                                    new Translation {Value = new float3(XCoord2, YCoord, x + 1)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(rightSide1);
                                EntityManager.SetComponentData(rightSide1, new SceneCubeComponent
                                {
                                    sceneChar = 'C',
                                    number = cubeNumber
                                });

                                cubeNumber++;
                            });
                        }
                    }

                    //Third Pyramid
                    //Y-Coordinate Loop 
                    for (var s = 0; s < pyramidHeight - 4; s++)
                    {
                        YCoord = pyramidHeight - 5 + 0.5f - s;

                        //Center
                        Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                        {
                            var centerEntity = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                            EntityManager.SetComponentData(centerEntity,
                                new Translation {Value = new float3(XCoord3, YCoord, 0)}
                            );
                            EntityManager.AddComponent<SceneCubeComponent>(centerEntity);
                            EntityManager.SetComponentData(centerEntity, new SceneCubeComponent
                            {
                                sceneChar = 'C',
                                number = cubeNumber
                            });

                            cubeNumber++;
                        });
                        //X-Coordinate Loop
                        for (var x = 0; x < s; x++)
                        {

                            Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
                            {

                                var leftSide2 = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(leftSide2,
                                    new Translation {Value = new float3(XCoord3, YCoord, -x - 1)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(leftSide2);
                                EntityManager.SetComponentData(leftSide2, new SceneCubeComponent
                                {
                                    sceneChar = 'C',
                                    number = cubeNumber
                                });

                                cubeNumber++;

                                var rightSide2 = EntityManager.Instantiate(prefabEntityComponent.PrefabEntity);

                                EntityManager.SetComponentData(rightSide2,
                                    new Translation {Value = new float3(XCoord3, YCoord, x + 1)}
                                );
                                EntityManager.AddComponent<SceneCubeComponent>(rightSide2);
                                EntityManager.SetComponentData(rightSide2, new SceneCubeComponent
                                {
                                    sceneChar = 'C',
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