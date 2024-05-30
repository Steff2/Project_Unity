using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace testCases
{
    public struct SceneCubeComponent : IComponentData
    {
        //Identifier for the current scene
        public char sceneChar;

        //Identifier of the cube
        public int number;

    }
}