using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(TilemapGenerator))]
public class MarchinSquaresSpawner : MonoBehaviour
{
    TilemapGenerator tilemapGenerator;
    int[,] tilemap;
    int[,] marchedTilemap;
    //counterclockwise binary representation of the corners
    //this way each bit has an associated corner
    //bottom right (br) = 1
    //top right (tr) = 2
    //top left (tl) = 4
    //bottom left (bl) = 8

    void Start()
    {
        tilemapGenerator = GetComponent<TilemapGenerator>();
    }

    [Button]
    //seperated the spawning of the tiles and the making of the tilemap to make clear to look at/easier to read
    //this will also make it easier to animate
    void CreateMarchingSquaresTilemap()
    {
        tilemap = tilemapGenerator.GetTilemap();
        marchedTilemap = new int[tilemap.GetLength(0) - 1, tilemap.GetLength(1) - 1];

        //march from left to right, bottom to top
        for (int x = 0; x < tilemap.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < tilemap.GetLength(1) - 1; y++)
            {
                int binaryCorners = 0;
                if (tilemap[x + 1, y] == 1) //br
                    binaryCorners += 1;
                if (tilemap[x + 1, y + 1] == 1) //tr
                    binaryCorners += 2;
                if (tilemap[x, y + 1] == 1) //tl
                    binaryCorners += 4;
                if (tilemap[x, y] == 1) //bl
                    binaryCorners += 8;

                marchedTilemap[x, y] = binaryCorners;

                if (binaryCorners == 5 || binaryCorners == 10) //the way my dungeon is generated should never cause one of these corners and would indacate some sort of error somewhere
                    Debug.LogWarning("Found odd corner at: " + x + ", " + y);
            }
        }
    }

    [Button]
    //!REMINDER! message can get so long it will get truncated in unity
    void PrintMarchedTileMap()
    {
        string result = "";
        for (int y = marchedTilemap.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = 0; x < marchedTilemap.GetLength(0); x++)
            {
                result += marchedTilemap[x, y] + " "; //add a aspace because there can be numbers above 9
            }
            result += "\n";
        }

        print(result);
    }
}
