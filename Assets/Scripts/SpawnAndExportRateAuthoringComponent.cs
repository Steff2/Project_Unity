using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct SpawnAndExportRateAuthoringComponent : IComponentData
{
    [Tooltip("Slowest Spawnrate")]
    public bool slowestSpawn;

    [Tooltip("Second slowest Spawnrate")]
    public bool fasterSpawn;

    [Tooltip("Third slowest Spawnrate")]
    public bool fastestSpawn;

    public bool testCase1;

    public bool testCase2;

    public bool testCase3;
}
