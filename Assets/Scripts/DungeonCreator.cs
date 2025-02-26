using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    [SerializeField] RectInt dungeonBounds;
    [SerializeField] RectInt minRoomSize;
    [SerializeField] int wallWidth = 1;
    [SerializeField] bool splitHorizontally = false;

    List<RectInt> rooms = new List<RectInt>();

    void Start()
    {
        dungeonBounds.width += wallWidth;
        dungeonBounds.height += wallWidth;
        rooms.Add(dungeonBounds);

        if (splitHorizontally)
        {
            SplitRoomHorizontally(0);
            SplitRoomHorizontally(1);
            SplitRoomVertically(0);
        }
        else
        {
            SplitRoomVertically(0); 
            SplitRoomVertically(1);
            SplitRoomHorizontally(0);
        }


    }

    void Update()
    {
        //walls
        foreach (RectInt room in rooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.yellow);
        }
    }

    void SplitRoomVertically(int roomIndex)
    {
        RectInt currentRoom = rooms[roomIndex];
        rooms.RemoveAt(roomIndex);

        RectInt newRoomLeft = new RectInt(
            currentRoom.x,
            currentRoom.y,
            (currentRoom.width - wallWidth) / 2 + wallWidth,
            currentRoom.height);
        ;
        RectInt newRoomRight = new RectInt(
            currentRoom.x + ((currentRoom.width - wallWidth) / 2),
            currentRoom.y,
            (currentRoom.width - wallWidth) / 2 + wallWidth,
            currentRoom.height);

        rooms.Insert(roomIndex, newRoomRight);
        rooms.Insert(roomIndex, newRoomLeft);
    }

    void SplitRoomHorizontally(int roomIndex)
    {
        RectInt currentRoom = rooms[roomIndex];
        rooms.RemoveAt(roomIndex);

        RectInt newRoomBottom = new RectInt(
            currentRoom.x,
            currentRoom.y,
            currentRoom.width,
            (currentRoom.height - wallWidth) / 2 + wallWidth);
        ;
        RectInt newRoomTop = new RectInt(
            currentRoom.x,
            currentRoom.y + ((currentRoom.height - wallWidth) / 2),
            currentRoom.width,
            (currentRoom.height - wallWidth) / 2 + wallWidth);

        rooms.Insert(roomIndex, newRoomTop);
        rooms.Insert(roomIndex, newRoomBottom);
    }
}
