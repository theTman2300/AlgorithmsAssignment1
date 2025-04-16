using UnityEngine;

[RequireComponent(typeof(TilemapGenerator))]
public class MarchinSquaresSpawner : MonoBehaviour
{
    TilemapGenerator tilemapGenerator;
    int[,] marchedTilemap;

    void Start()
    {
        tilemapGenerator = GetComponent<TilemapGenerator>();
    }

    
}
