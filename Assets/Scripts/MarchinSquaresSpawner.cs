using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(TilemapGenerator))]
public class MarchinSquaresSpawner : MonoBehaviour
{
    TilemapGenerator tilemapGenerator;
    int[,] tilemap;
    int[,] marchedTilemap;

    void Start()
    {
        tilemapGenerator = GetComponent<TilemapGenerator>();
    }

    [Button]
    void CreateMarchingSquaresTilemap()
    {
        tilemap = tilemapGenerator.GetTilemap();

        for (int x = 0; x < tilemap.GetLength(0) - 2; x++)
        {
            for (int y = 0; y < tilemap.GetLength(1) - 2; y++)
            {

            }
        }
    }
}
