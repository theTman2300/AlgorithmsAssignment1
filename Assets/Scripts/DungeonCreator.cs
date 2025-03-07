using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    [SerializeField] RectInt dungeonBounds;
    [SerializeField] RectInt maxGenerationSize;
    [SerializeField] RectInt maxRoomSize;
    [SerializeField] RectInt minRoomSize;
    [HorizontalLine]
    [SerializeField] int seed = 0; //this doesn't work
    [SerializeField] float secondsPerOperation = .5f;

    public List<RectInt> rooms = new List<RectInt>();
    public List<RectInt> newRooms = new List<RectInt>();
    public List<RectInt> completedRooms = new List<RectInt>();
    RectInt currentWorkingRoom = new RectInt(0, 0, 0, 0);
    System.Random rng; //this is used because unity random isn't deterministic when using ienumerators

    void Start()
    {
        ResetDungeon();
    }

    [Button]
    private void ResetDungeon()
    {
        StopAllCoroutines();
        rooms.Clear();
        newRooms.Clear();
        completedRooms.Clear();
        rooms.Add(dungeonBounds);

        rng = new System.Random(seed);
        StartCoroutine(CreateRooms());
    }

    void Update()
    {
        //show the current rooms
        foreach (RectInt room in rooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.yellow);
        }
        foreach (RectInt room in completedRooms)
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
        while (rooms.Count > 0 && loops < 50)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                //Test out changing it so that it always splits the room in its biggest acis. (so if room width is bigger then room height, then split vertically)

                currentWorkingRoom = rooms[i];
                int randomNumber = rng.Next(0, 2);

                if (rooms[i].width <= maxRoomSize.width && rooms[i].height > maxRoomSize.height)
                {
                    randomNumber = 1;
                }
                if (rooms[i].width > maxRoomSize.width && rooms[i].height <= maxRoomSize.height)
                {
                    randomNumber = 0;
                }
                if (rooms[i].width <= maxRoomSize.width && rooms[i].height <= maxRoomSize.height)
                {
                    randomNumber = 3;
                }

                if (randomNumber == 0)
                {
                    SplitRoomVertically(i);
                    yield return new WaitForSeconds(secondsPerOperation);
                }
                else if (randomNumber == 1)
                {
                    SplitRoomHorizontally(i);
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

        if (rooms.Count > 0)
        {
            Debug.LogWarning("CreateRooms exceeded max amount of loops");
        }

        yield return new WaitForSeconds(secondsPerOperation);
    }

    void SplitRoomVertically(int roomIndex)
    {
        RectInt currentRoom = rooms[roomIndex];
        int maxSplit = minRoomSize.width;
        if (currentRoom.width - minRoomSize.width > maxGenerationSize.width)
        {
            maxSplit = maxGenerationSize.width;
        }
        else
        {
            maxSplit = currentRoom.width - minRoomSize.width;
        }
        int splitPosition = rng.Next(minRoomSize.width, maxSplit);
        if (rng.Next(0, 2) == 0)
        {
            splitPosition = currentRoom.width - splitPosition;
        }

        RectInt newRoomLeft = new RectInt(
            currentRoom.x,
            currentRoom.y,
            splitPosition,
            currentRoom.height
            );
        RectInt newRoomRight = new RectInt(
            currentRoom.x + splitPosition - 1,
            currentRoom.y,
            currentRoom.width - splitPosition + 1,
            currentRoom.height
            );

        if (newRoomLeft.width <= maxRoomSize.width && newRoomLeft.height <= maxRoomSize.height)
            completedRooms.Add(newRoomLeft);
        else
            newRooms.Add(newRoomLeft);

        if (newRoomRight.width <= maxRoomSize.width && newRoomRight.height <= maxRoomSize.height)
            completedRooms.Add(newRoomRight);
        else
            newRooms.Add(newRoomRight);
    }

    void SplitRoomHorizontally(int roomIndex)
    {
        RectInt currentRoom = rooms[roomIndex];
        int maxSplit = minRoomSize.width;
        if (currentRoom.height - minRoomSize.height > maxGenerationSize.height)
        {
            maxSplit = maxGenerationSize.height;
        }
        else
        {
            maxSplit = currentRoom.height - minRoomSize.height;
        }
        int splitPosition = rng.Next(minRoomSize.height, maxSplit);
        if (rng.Next(0, 2) == 0)
        {
            splitPosition = currentRoom.height - splitPosition;
        }

        RectInt newRoomBottom = new RectInt(
            currentRoom.x,
            currentRoom.y,
            currentRoom.width,
            splitPosition
            );
        RectInt newRoomTop = new RectInt(
            currentRoom.x,
            currentRoom.y + splitPosition - 1,
            currentRoom.width,
            currentRoom.height - splitPosition + 1
            );

        if (newRoomBottom.width <= maxRoomSize.width && newRoomBottom.height <= maxRoomSize.height)
            completedRooms.Add(newRoomBottom);
        else
            newRooms.Add(newRoomBottom);

        if (newRoomTop.width <= maxRoomSize.width && newRoomTop.height <= maxRoomSize.height)
            completedRooms.Add(newRoomTop);
        else
            newRooms.Add(newRoomTop);
    }
}
