using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    [SerializeField] RectInt dungeonBounds;
    [SerializeField] RectInt minRoomSize;
    [HorizontalLine]
    [SerializeField] int seed = 0; //this doesn't work
    [SerializeField] float secondsPerOperation = .5f;

    List<RectInt> rooms = new List<RectInt>();
    List<RectInt> newRooms = new List<RectInt>();
    RectInt currentWorkingRoom = new RectInt(0, 0, 0, 0);
    System.Random rng; //this is used because unity random isn't deterministic when using ienumerators

    void Start()
    {
        ResetDungeon();
    }

    private void ResetDungeon()
    {
        rooms.Clear();
        newRooms.Clear();
        rooms.Add(dungeonBounds);

        rng = new System.Random(seed);
        StartCoroutine(CreateRooms());
    }

    void Update()
    {
        //show the current rooms
        foreach (RectInt room in rooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.white);
        }

        //show the division process
        foreach (RectInt room in newRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green);
        }
        //show the front of the division process
        AlgorithmsUtils.DebugRectInt(currentWorkingRoom, Color.cyan);
    }

    IEnumerator CreateRooms()
    {

        int loops = 0; //make sure it doesn't get in an infinite loop
        while (loops < 7)
        {

            for (int i = 0; i < rooms.Count; i++)
            {
                currentWorkingRoom = rooms[i];
                int randomNumber = rng.Next(0, 2);
                int randomWidth = rng.Next(0, 5) + Mathf.RoundToInt(minRoomSize.width * 1.25f);
                int randomHeight = rng.Next(0, 5) + Mathf.RoundToInt(minRoomSize.height * 1.25f);

                if (rooms[i].width <= randomWidth && rooms[i].height > randomHeight)
                {
                    randomNumber = 1;
                }
                if (rooms[i].width > randomWidth && rooms[i].height <= randomHeight)
                {
                    randomNumber = 0;
                }
                if (rooms[i].width <= randomWidth && rooms[i].height <= randomHeight)
                {
                    randomNumber = 3;
                }

                if (randomNumber == 0)
                {
                    SplitRoomHorizontally(i);
                    yield return new WaitForSeconds(secondsPerOperation);
                }
                else if (randomNumber == 1)
                {
                    SplitRoomVertically(i);
                    yield return new WaitForSeconds(secondsPerOperation);
                }
                else
                {
                    newRooms.Add(rooms[i]);
                    yield return new WaitForSeconds(secondsPerOperation);
                }
            }
            rooms = new List<RectInt>(newRooms);
            newRooms.Clear();
            currentWorkingRoom = new RectInt(0, 0, 0, 0);
            loops++;
        }
        yield return new WaitForSeconds(secondsPerOperation);
    }

    void SplitRoomHorizontally(int roomIndex)
    {
        RectInt currentRoom = rooms[roomIndex];
        int splitOffset = rng.Next(-2, 2);

        RectInt newRoomLeft = new RectInt(
            currentRoom.x,
            currentRoom.y,
            currentRoom.width / 2 + splitOffset,
            currentRoom.height
            );
        RectInt newRoomRight = new RectInt(
            currentRoom.x + (currentRoom.width / 2 - 1) + splitOffset,
            currentRoom.y,
            currentRoom.width - (currentRoom.width / 2 - 1) - splitOffset,
            currentRoom.height
            );

        newRooms.Insert(roomIndex, newRoomRight);
        newRooms.Insert(roomIndex, newRoomLeft);
    }

    void SplitRoomVertically(int roomIndex)
    {
        RectInt currentRoom = rooms[roomIndex];
        int splitOffset = rng.Next(-2, 2);

        RectInt newRoomBottom = new RectInt(
            currentRoom.x,
            currentRoom.y,
            currentRoom.width,
            currentRoom.height / 2 + splitOffset
            );
        RectInt newRoomTop = new RectInt(
            currentRoom.x,
            currentRoom.y + (currentRoom.height / 2 - 1) + splitOffset,
            currentRoom.width,
            currentRoom.height - (currentRoom.height / 2 - 1) - splitOffset
            );

        newRooms.Insert(roomIndex, newRoomTop);
        newRooms.Insert(roomIndex, newRoomBottom);
    }
}
