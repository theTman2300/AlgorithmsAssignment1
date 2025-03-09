using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    [SerializeField] RectInt dungeonBounds;
    [Tooltip("Used to avoid spiral pattern when creating larger dungeons \n Must alway be bigger then minSize + 2, but reccomended to be somewhere around 3/4 of dungeonBounds")]
    [SerializeField] RectInt maxGenerationSize;
    [SerializeField] RectInt maxRoomSize;
    [SerializeField] RectInt minRoomSize;
    [HorizontalLine]
    [SerializeField] int seed = 0;
    [SerializeField] float secondsPerOperation = .5f;
    [SerializeField] bool generateInstantly = false;
    [HorizontalLine]
    [SerializeField, ReadOnly] List<RectInt> rooms = new List<RectInt>(); //rooms to be split
    [SerializeField, ReadOnly] List<RectInt> newRooms = new List<RectInt>(); //rooms that have been split this loop
    [SerializeField, ReadOnly] List<RectInt> completedRooms = new List<RectInt>(); //completed rooms  
    RectInt currentWorkingRoom = new RectInt(0, 0, 0, 0); //current room being split
    System.Random rng; //this is used because unity random seed doesn't work when using ienumerators

    Graph<RectInt> roomGraph = new Graph<RectInt>();

    void Start()
    {
        ResetDungeon();
    }

    [Button]
    private void ResetDungeon()
    {
        StopAllCoroutines(); //in event that dungeon generator was still running when this function is called
        rooms.Clear();
        newRooms.Clear();
        completedRooms.Clear();
        rooms.Add(dungeonBounds);

        //check wether maxGenerationSize is bigger then minimum
        if (maxGenerationSize.width < minRoomSize.width + 2 || maxGenerationSize.height < minRoomSize.height + 2)
        {
            Debug.LogError("MaxGenerationSize is smaller then minimum of width: " + (minRoomSize.width + 2) + " and height: " + (minRoomSize.height + 2));
        }

        rng = new System.Random(seed);
        StartCoroutine(CreateRooms());
    }

    void Update()
    {
        //show the rooms
        foreach (RectInt room in rooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.yellow); //rooms to be split
        }
        foreach (RectInt room in completedRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.white); //completed rooms
        }

        foreach (RectInt room in newRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green); //rooms that have been split this loop
        }
        AlgorithmsUtils.DebugRectInt(currentWorkingRoom, Color.cyan); //current room being split

        //show graph
        foreach (RectInt room in completedRooms)
        {
            Vector3 roomMiddle = new Vector3(room.x + room.width / 2, 0, room.y + room.height / 2);
            foreach (RectInt connectedRoom in roomGraph.GetNeighbours(room))
            {
                Vector3 connectedMiddle = new Vector3(connectedRoom.x + connectedRoom.width / 2, 0, connectedRoom.y + connectedRoom.height / 2);
                Debug.DrawLine(roomMiddle, connectedMiddle, Color.magenta);
            }
        }
    }

    #region create basic layout (step 1)
    IEnumerator CreateRooms()
    {
        while (rooms.Count > 0) //while there are still rooms to be split
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                currentWorkingRoom = rooms[i];
                bool splitVertical = false;

                //split room over longest side
                if (currentWorkingRoom.width > currentWorkingRoom.height)
                    splitVertical = true;
                else
                    splitVertical = false;

                //split room over the axis that is not yet smaller than maxRoomSize
                if (rooms[i].width <= maxRoomSize.width && rooms[i].height > maxRoomSize.height)
                {
                    splitVertical = false;
                }
                else if (rooms[i].width > maxRoomSize.width && rooms[i].height <= maxRoomSize.height)
                {
                    splitVertical = true;
                }

                //split rooms
                if (splitVertical)
                {
                    SplitRoomVertically(i);
                    if (!generateInstantly)
                        yield return new WaitForSeconds(secondsPerOperation);
                }
                else
                {
                    SplitRoomHorizontally(i);
                    if (!generateInstantly)
                        yield return new WaitForSeconds(secondsPerOperation);
                }
            }
            //set rooms to be split to be the newely generated rooms
            rooms = new List<RectInt>(newRooms);
            newRooms.Clear();
            currentWorkingRoom = new RectInt(0, 0, 0, 0);

            if (generateInstantly)
                yield return null; //make sure the editor won't hang in an infinite loop
        }

        if (!generateInstantly)
            yield return new WaitForSeconds(secondsPerOperation);

        Debug.Log("basic layout generation done");
        StartCoroutine(CreateGraph());
    }

    void SplitRoomVertically(int roomIndex)
    {
        RectInt currentRoom = rooms[roomIndex];
        int maxSplit = currentRoom.width - minRoomSize.width; //the maximum relative position for the room split to occur

        //this check is done to make sure very big rooms can be split in bigger portions
        //if this wasn't done you would get the rooms to form in an obvious spirally effect
        if (currentRoom.width - minRoomSize.width > maxGenerationSize.width)
            maxSplit = maxGenerationSize.width;

        int splitPosition = rng.Next(minRoomSize.width + 2, maxSplit); //relative position for the split to occur ( + 2 to account for walls)
        if (rng.Next(0, 2) == 0) //wether relative position is from the left or from the right
            splitPosition = currentRoom.width - splitPosition;

        //create new rooms
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

        //check wether room is completed or not and add it to corrosponding list
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
        int maxSplit = maxSplit = currentRoom.height - minRoomSize.height; //the maximum relative position for the room split to occur

        //this check is done to make sure very big rooms can be split in bigger portions
        //if this wasn't done you would get the rooms to form in an obvious spirally effect
        if (currentRoom.height - minRoomSize.height > maxGenerationSize.height)
            maxSplit = maxGenerationSize.height;

        int splitPosition = rng.Next(minRoomSize.height + 2, maxSplit); //relative position for the split to occur ( + 2 to account for walls)
        if (rng.Next(0, 2) == 0) //wether relative position is from the bottom or from up
            splitPosition = currentRoom.height - splitPosition;

        //create new rooms
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

        //check wether room is completed or not and add it to corrosponding list
        if (newRoomBottom.width <= maxRoomSize.width && newRoomBottom.height <= maxRoomSize.height)
            completedRooms.Add(newRoomBottom);
        else
            newRooms.Add(newRoomBottom);

        if (newRoomTop.width <= maxRoomSize.width && newRoomTop.height <= maxRoomSize.height)
            completedRooms.Add(newRoomTop);
        else
            newRooms.Add(newRoomTop);
    }
    #endregion

    IEnumerator CreateGraph()
    {
        foreach (RectInt room in completedRooms)
        {
            roomGraph.AddNode(room);
            foreach (RectInt edgeRoom in GetIntersectingRooms(room))
            {
                roomGraph.AddEdge(room, edgeRoom);
                if (!generateInstantly)
                    yield return new WaitForSeconds(secondsPerOperation);
                //add door if there isn't already one
            }
        }

        if (!generateInstantly)
            yield return new WaitForSeconds(secondsPerOperation);
    }

    List<RectInt> GetIntersectingRooms(RectInt currentRoom)
    {
        List<RectInt> result = new List<RectInt>();

        foreach (RectInt room in completedRooms)
        {
            if (room == currentRoom) continue;
            if (room.x + room.width < currentRoom.x) continue;
            if (room.y + room.height < currentRoom.y) continue;
            if (room.x > currentRoom.x + currentRoom.width) continue;
            if (room.y > currentRoom.y + currentRoom.height) continue;

            result.Add(room);
        }

        return result;
    }
}
