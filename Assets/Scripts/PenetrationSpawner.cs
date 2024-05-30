using System.IO;
using testCases;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine.SceneManagement;

namespace testCases
{
    /// <summary>
    /// Variablen die getestet oder verändert werden können:
    /// exitTime - reguliert die Dauer bis die Szene endet und der export passiert
    /// exportTimer - reguliert der Zeit zwischen dem Sammeln von Positionsdaten 0.15 heißt 0.15 Zeit zwischen dem Sammeln
    /// </summary>
    public class PenetrationSpawner : ComponentSystem
    {
        //Timer of the spawning interval
        private float exportTimer;

        private bool setup;
        private bool Spawning = true;

        //Cube Identifier
        private int cubeNumber = 1;

        protected override void OnUpdate()
        {
            //Workaround for ECS, without it it would be called in every Scene
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Penetration"))
            {
                PositionAndKollision.exitTime = 5f;
                PositionAndKollision.sceneName = "Penetration";
                PositionAndKollision.spawnRate = "";

                 exportTimer -= Time.DeltaTime;

                if (setup == false)
                {
                    //Get all the options used in the Editor
                    Entities.ForEach((ref SpawnAndExportRateAuthoringComponent spawnratecomponent) =>
                    {
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

                //Spawns Cubes in a regular interval
                if (Spawning == true)
                {
                    Entities
                        .WithAll<PhysicsVelocity>()
                        .ForEach((Entity entity) =>
                        {
                            EntityManager.AddComponent<SceneCubeComponent>(entity);
                            EntityManager.SetComponentData(entity, new SceneCubeComponent
                            {
                                sceneChar = 'B',
                                number = cubeNumber
                            });
                            cubeNumber++;
                        });
                    Spawning = false;
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
