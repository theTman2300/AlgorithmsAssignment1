using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(DungeonCreator))]
public class TilemapGenerator : MonoBehaviour
{
    DungeonCreator dungeonCreator;

    int[,] tilemap;
    RectInt[] rooms;
    RectInt[] doors;
    bool tileMapWasGenerated = false;

    private void Start()
    {
        dungeonCreator = GetComponent<DungeonCreator>();
    }

    [Button]
    void GenerateTilemap()
    {
        (rooms, doors) = dungeonCreator.GetRoomsAndDoors();
        tilemap = new int[dungeonCreator.DungeonBounds.width, dungeonCreator.DungeonBounds.height];

        //walls
        foreach (RectInt room in rooms)
        {
            foreach (Vector2Int roomPos in room.allPositionsWithin)
            {
                if (roomPos.x == room.x || roomPos.x == room.xMax - 1
                    || roomPos.y == room.y || roomPos.y == room.yMax - 1)
                {
                    tilemap[roomPos.x - dungeonCreator.DungeonBounds.x, roomPos.y - dungeonCreator.DungeonBounds.y] = 1;
                }
                //else not a wall
                //don't set to 0, otherwise some walls could be overwritten
            }
        }

        //doors
        foreach (RectInt door in doors)
        {
            foreach (Vector2Int doorPos in door.allPositionsWithin)
            {
                if (doorPos.x == door.x || doorPos.x == door.xMax - 1
                    || doorPos.y == door.y || doorPos.y == door.yMax - 1)
                {
                    tilemap[doorPos.x - dungeonCreator.DungeonBounds.x, doorPos.y - dungeonCreator.DungeonBounds.y] = 0;
                }
            }
        }
        tileMapWasGenerated = true;
    }

    [Button]
    void PrintTileMap()
    {
        string result = "";
        for (int y = tilemap.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = 0; x < tilemap.GetLength(0); x++)
            {
                result += tilemap[x, y] == 1 ? "#" : "0";
            }
            result += "\n";
        }

        print(result);
    }

    public int[,] GetTilemap()
    {
        if (!tileMapWasGenerated)
        {
            GenerateTilemap();
        }

        return tilemap;
    }

    public RectInt[] GetRooms()
    {
        if (rooms.Length == 0)
            (rooms, doors) = dungeonCreator.GetRoomsAndDoors();

        return rooms;
    }

    public void ResetDungeon()
    {
        StopAllCoroutines();
        tileMapWasGenerated = false;
        rooms = null;
        doors = null;
    }
}
