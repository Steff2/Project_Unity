using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using System.IO;
using Unity.Collections;
using Unity.Physics;
using Unity.Physics.Systems;

//UNUSED
namespace testCases
{
    [UpdateBefore(typeof(EndFramePhysicsSystem))]
    public class ExportSystem : SystemBase
    {
        protected override void OnUpdate()
        {

            var elapsedTime = (float) Time.ElapsedTime;
            Entities.ForEach((in Translation cubeTranslation, in SceneCubeComponent scene) =>
            {
                /*if (scene.sceneChar == 'A')
                {
                    StreamWriter saveFile = new StreamWriter("data_FullRoom.txt", true);
                    saveFile.Write($"{scene.number};{cubeTranslation.Value};{elapsedTime}; ");


                    saveFile.Close();
                    //Debug.Log($"{scene.number};{cubeTranslation.Value.x};{cubeTranslation.Value.y};{cubeTranslation.Value.z}");
                }*/

                /*if (scene.sceneChar == 'B')
                {
                    StreamWriter saveFile = new StreamWriter("data_Penetration.txt", true);
                    saveFile.Write($"{scene.number};{cubeTranslation.Value};{elapsedTime}; ");
    
    
                    saveFile.Close();
                    Debug.Log(
                        $"{scene.number};{cubeTranslation.Value.x};{cubeTranslation.Value.y};{cubeTranslation.Value.z}");
                }
    
                if (scene.sceneChar == 'C')
                {
                    StreamWriter saveFile = new StreamWriter("data_Pyramid.txt", true);
                    saveFile.Write($"{scene.number};{cubeTranslation.Value};{elapsedTime}; ");
    
    
                    saveFile.Close();
                    Debug.Log(
                        $"{scene.number};{cubeTranslation.Value.x};{cubeTranslation.Value.y};{cubeTranslation.Value.z}");
                }
    
                if (scene.sceneChar == 'D')
                {
                    StreamWriter saveFile = new StreamWriter("data_Resitution.txt", true);
                    saveFile.Write($"{scene.number};{cubeTranslation.Value};{elapsedTime}; ");
    
    
                    saveFile.Close();
                    Debug.Log(
                        $"{scene.number};{cubeTranslation.Value.x};{cubeTranslation.Value.y};{cubeTranslation.Value.z}");
                }
    
                if (scene.sceneChar == 'E')
                {
                    StreamWriter saveFile = new StreamWriter("data_Tower.txt", true);
                    saveFile.Write($"{scene.number};{cubeTranslation.Value};{elapsedTime}; ");
    
    
                    saveFile.Close();
                    Debug.Log(
                        $"{scene.number};{cubeTranslation.Value.x};{cubeTranslation.Value.y};{cubeTranslation.Value.z}");
                }*/
            }).WithoutBurst().Run();
        }
    }
}
