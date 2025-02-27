using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    /*
     TODO:
    -do away with wallWitdh - DONE
    -make seeds work (done by using random.value instead of random.range)
    -make the room generation look better
     */

    [SerializeField] RectInt dungeonBounds;
    [SerializeField] RectInt minRoomSize;
    [HorizontalLine]
    [SerializeField] int seed = 0; //this doesn't work
    [SerializeField] float secondsPerOperation = .5f;

    List<RectInt> rooms = new List<RectInt>();
    List<RectInt> newRooms = new List<RectInt>();
    int smallestRoomWidth;
    int smallestRoomHeight;

    void Start()
    {
        smallestRoomWidth = dungeonBounds.width;
        smallestRoomHeight = dungeonBounds.height;

        rooms.Add(dungeonBounds);

        Random.InitState(seed); //this is broken for now
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
    }

    IEnumerator CreateRooms()
    {
        int loops = 0; //make sure it doesn't get in an infinite loop
        //(smallestRoomWidth > minRoomSize.width && smallestRoomHeight > minRoomSize.height) && 
        while (loops < 7)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                int randomNumber = Mathf.RoundToInt(Random.value * 3);
                if (rooms[i].width <= minRoomSize.width && rooms[i].height <= minRoomSize.height)
                {
                    randomNumber = 3;
                }
                else if (rooms[i].width <= minRoomSize.width)
                {
                    randomNumber = Mathf.RoundToInt(Random.value * 2) == 0 ? 1 : 3;
                }
                else if (rooms[i].height <= minRoomSize.height)
                {
                    randomNumber = Mathf.RoundToInt(Random.value * 2) == 0 ? 0 : 3;
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
            loops++;
        }
        yield return new WaitForSeconds(secondsPerOperation);
    }

    void SplitRoomVertically(int roomIndex)
    {
        RectInt currentRoom = rooms[roomIndex];

        RectInt newRoomLeft = new RectInt(
            currentRoom.x,
            currentRoom.y,
            currentRoom.width / 2,
            currentRoom.height
            );
        RectInt newRoomRight = new RectInt(
            currentRoom.x + (currentRoom.width / 2 - 1),
            currentRoom.y,
            currentRoom.width - (currentRoom.width / 2 - 1),
            currentRoom.height
            );

        smallestRoomWidth = newRoomLeft.width < smallestRoomWidth ? newRoomLeft.width : smallestRoomWidth;
        newRooms.Insert(roomIndex, newRoomRight);
        newRooms.Insert(roomIndex, newRoomLeft);
    }

    void SplitRoomHorizontally(int roomIndex)
    {
        RectInt currentRoom = rooms[roomIndex];

        RectInt newRoomBottom = new RectInt(
            currentRoom.x,
            currentRoom.y,
            currentRoom.width,
            currentRoom.height / 2
            );
        RectInt newRoomTop = new RectInt(
            currentRoom.x,
            currentRoom.y + (currentRoom.height / 2 - 1),
            currentRoom.width,
            currentRoom.height - (currentRoom.height / 2 - 1)
            );

        smallestRoomHeight = newRoomBottom.height < smallestRoomHeight ? newRoomBottom.height : smallestRoomHeight;
        newRooms.Insert(roomIndex, newRoomTop);
        newRooms.Insert(roomIndex, newRoomBottom);
    }
}
