using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    /*
     TODO:
    -do away with wallWitdh
    -make seeds work
    -make the room generation look better
     */

    [SerializeField] RectInt dungeonBounds;
    [SerializeField] RectInt minRoomSize;
    [SerializeField] int wallWidth = 1;
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

        dungeonBounds.width += wallWidth;
        dungeonBounds.height += wallWidth;
        rooms.Add(dungeonBounds);

        UnityEngine.Random.InitState(seed); //this is broken for now
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
                int randomNumber = Random.Range(0, 3);
                if (rooms[i].width <= minRoomSize.width && rooms[i].height <= minRoomSize.height)
                {
                    randomNumber = 3;
                }
                else if (rooms[i].width <= minRoomSize.width)
                {
                    randomNumber = Random.Range(0, 2) == 0 ? 1 : 3;
                }
                else if(rooms[i].height <= minRoomSize.height)
                {
                    randomNumber = Random.Range(0, 2) == 0 ? 0 : 3;
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
            (currentRoom.width - wallWidth) / 2 + wallWidth,
            currentRoom.height
            );
        RectInt newRoomRight = new RectInt(
            currentRoom.x + ((currentRoom.width - wallWidth) / 2),
            currentRoom.y,
            (currentRoom.width - wallWidth) / 2 + wallWidth,
            currentRoom.height
            );

        smallestRoomWidth = newRoomLeft.width - wallWidth < smallestRoomWidth ? newRoomLeft.width - wallWidth : smallestRoomWidth;
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
            (currentRoom.height - wallWidth) / 2 + wallWidth
            );
        RectInt newRoomTop = new RectInt(
            currentRoom.x,
            currentRoom.y + ((currentRoom.height - wallWidth) / 2),
            currentRoom.width,
            (currentRoom.height - wallWidth) / 2 + wallWidth
            );

        smallestRoomHeight = newRoomBottom.height - wallWidth < smallestRoomHeight ? newRoomBottom.height - wallWidth : smallestRoomHeight;
        newRooms.Insert(roomIndex, newRoomTop);
        newRooms.Insert(roomIndex, newRoomBottom);
    }
}
