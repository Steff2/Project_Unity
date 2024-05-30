using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PositionAndKollision
{
    //List of the gathered position data for exporting
    public static List<String> Position = new List<string>();

    //List of the gathered collision data
    public static List<String> Collision = new List<string>();


    public static string spawnRate = "";
    public static string sceneName = "";

    public static int testCase;

    public static float exitTime;


    public static bool exportedPos = false;
    public static bool exportedCol = false;


    private static bool posDone;
    private static bool colDone;

    //Used when gathering collision data
    public static string duplicateCheckString;

    //Exports the positionsdata
    public static void ExportPos()
    {
        if (!exportedPos)
        {
            exportedPos = true;

            foreach (string dataPos in Position)
            {
                var tempsplit = dataPos.Split(',');

                var saveFilePosition =
                    new StreamWriter(
                        $"f:/MATLAB/Data/Unity/{sceneName}{spawnRate}/Testsatz{testCase}/data_Position_{tempsplit[0]}.txt", true);

                saveFilePosition.Write(dataPos);

                saveFilePosition.Close();

            }


            posDone = true;


            //Exit scene when both export functions are done
            if (posDone && colDone)
            {
                Exit();
            }
        }
    }

    //Exports the collision data
    public static void ExportCol()
    {
        if (!exportedCol)
        {
            exportedCol = true;

            var saveFileCollision =
                new StreamWriter($"f:/MATLAB/Data/Unity/{sceneName}{spawnRate}/Testsatz{testCase}/data_Collision.txt", true);

            foreach (string dataCol in Collision)
            {
                saveFileCollision.Write(dataCol);
            }

            saveFileCollision.Close();

            colDone = true;


            //Exit scene when both export functions are done
            if (posDone && colDone)
            {
                Exit();
            }
        }
    }


    //Function to exit play mode after exporting is done
    public static void Exit()
    {
        EditorApplication.ExitPlaymode();
    }
}
