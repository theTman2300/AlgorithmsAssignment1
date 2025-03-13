using JetBrains.Annotations;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    [SerializeField] RectInt dungeonBounds;
    [Tooltip("Used to avoid spiral pattern when creating larger dungeons \n Must alway be bigger then minSize + 2, but reccomended to be somewhere around 3/4 of dungeonBounds")]
    [SerializeField] RectInt maxGenerationSize;
    [SerializeField] RectInt maxRoomSize;
    [SerializeField] RectInt minRoomSize;
    [Tooltip("the minimum overlap to make an edge between the rooms")]
    [SerializeField] int minDoorOverlap = 4;
    [SerializeField] int doorWidth;
    [HorizontalLine]
    [SerializeField] int seed = 0;
    [SerializeField] float secondsPerOperation = .5f;
    [SerializeField] bool generateFast = false;
    [SerializeField] Transform roomCursor;
    [HorizontalLine]
    [SerializeField, ReadOnly] List<RectInt> rooms = new List<RectInt>(); //rooms to be split
    [SerializeField, ReadOnly] List<RectInt> newRooms = new List<RectInt>(); //rooms that have been split this loop
    [SerializeField, ReadOnly] List<RectInt> completedRooms = new List<RectInt>(); //completed rooms  
    [SerializeField, ReadOnly] List<RectInt> doors = new List<RectInt>(); //doors between rooms
    RectInt currentWorkingRoom = default; //current room being split
    System.Random rng; //this is used because unity random seed doesn't work when using ienumerators

    Graph<RectInt> roomGraph = new Graph<RectInt>();
    RectInt selectedRoom = default;

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
        roomGraph.Clear();
        doors.Clear();
        selectedRoom = default;

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
            if (!roomGraph.ContainsKey(room)) continue;

            Vector3 roomMiddle = new Vector3(room.x + room.width / 2, 0, room.y + room.height / 2);
            foreach (RectInt connectedRoom in roomGraph.GetEdgeNodes(room))
            {
                Vector3 connectedMiddle = new Vector3(connectedRoom.x + connectedRoom.width / 2, 0, connectedRoom.y + connectedRoom.height / 2);
                Debug.DrawLine(roomMiddle, connectedMiddle, Color.magenta);
            }
        }
        foreach (RectInt door in doors)
        {
            AlgorithmsUtils.DebugRectInt(door, Color.cyan);
        }

        //room cusror
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedRoom == default || selectedRoom != FindRoomAtPosition(roomCursor.position))
            {
                selectedRoom = FindRoomAtPosition(roomCursor.position);
                Debug.Log("Selected Room " + selectedRoom);
                roomGraph.PrintEdgeNodes(selectedRoom);
            }
            else
            {
                selectedRoom = default;
            }
        }
        AlgorithmsUtils.DebugRectInt(selectedRoom, Color.red);

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
                    if (!generateFast)
                        yield return new WaitForSeconds(secondsPerOperation);
                }
                else
                {
                    SplitRoomHorizontally(i);
                    if (!generateFast)
                        yield return new WaitForSeconds(secondsPerOperation);
                }
            }
            //set rooms to be split to be the newely generated rooms
            rooms = new List<RectInt>(newRooms);
            newRooms.Clear();
            currentWorkingRoom = default;
        }

        if (!generateFast)
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

    #region room removal, graph creation and door creation (step 2)
    IEnumerator CreateGraph()
    {
        //sort rooms by smallest area
        completedRooms = new(completedRooms.OrderBy(room => room.width * room.height));
        SetRoomGraph(); //set nodes and edges

        //remove 10% of smallest rooms
        for (int i = 0; i < completedRooms.Count * .1f; i++)
        {
            RectInt roomToRemove = completedRooms[0];
            completedRooms.RemoveAt(0);
            SetRoomGraph(); //set all the current nodes and edges after removing a room

            bool roomsAreReachable = false;
            roomsAreReachable = roomGraph.DFS(completedRooms[0]);

            if (!generateFast)
                yield return new WaitForSeconds(secondsPerOperation);

            if (!roomsAreReachable)
            {
                completedRooms.Insert(0, roomToRemove);
                roomGraph.AddNode(roomToRemove);
                SetRoomGraph();
                Debug.Log("Not all rooms are reachable, exited room removal early");
                break;
            }
        }

        Debug.Log(completedRooms[0]); //log where dfs starts
        Debug.Log("All rooms reachable DFS: " + roomGraph.DFS(completedRooms[0])); //last graph check
        Debug.Log("Graph creation done");
        StartCoroutine(CreateDoors());
    }

    void SetRoomGraph()
    {
        roomGraph.Clear();
        foreach (RectInt room in completedRooms) //add nodes
        {
            roomGraph.AddNode(room);
        }
        foreach (RectInt room in completedRooms)
        {
            foreach (RectInt edgeNode in GetIntersectingRooms(room))
            {
                roomGraph.AddEdge(room, edgeNode);
            }
        }
    }

    IEnumerator CreateDoors()
    {
        List<RectInt> roomsWithDoor = new();
        foreach (RectInt room in roomGraph.GetKeys()) //using getkeys to go in direction of the graph, otherwise there would be doors that don't generate
        {
            roomsWithDoor.Add(room);
            foreach (RectInt edgeNode in roomGraph.GetEdgeNodes(room))
            {
                //check if door already exists
                if (roomsWithDoor.Contains(edgeNode)) continue;

                int overlapX = edgeNode.x < room.x ? edgeNode.xMax - room.x : edgeNode.x == room.x ? (edgeNode.xMax < room.xMax ? edgeNode.xMax - room.x : room.xMax - edgeNode.x) : room.xMax - edgeNode.x;
                int overlapY = edgeNode.y < room.y ? edgeNode.yMax - room.y : edgeNode.y == room.y ? (edgeNode.yMax < room.yMax ? edgeNode.yMax - room.y : room.yMax - edgeNode.y) : room.yMax - edgeNode.y;
                if (overlapX > overlapY)
                {
                    RectInt door = new(0, 0, doorWidth, 1);
                    door.y = edgeNode.yMax < room.yMax ? room.y : room.yMax - 1;
                    door.x = edgeNode.xMax < room.xMax ? edgeNode.xMax - (overlapX / 2) : room.xMax - (overlapX / 2);
                    door.x -= doorWidth / 2;
                    doors.Add(door);
                }
                else
                {
                    RectInt door = new(0, 0, 1, doorWidth);
                    door.x = edgeNode.x < room.x ? room.x : room.xMax - 1;
                    door.y = edgeNode.yMax < room.yMax ? edgeNode.yMax - (overlapY / 2) : room.yMax - (overlapY / 2);
                    door.y -= doorWidth / 2;
                    doors.Add(door);
                }
            }
            if (!generateFast)
                yield return new WaitForSeconds(secondsPerOperation);
        }

        Debug.Log("Door creation done");
    }

    List<RectInt> GetIntersectingRooms(RectInt currentRoom)
    {
        List<RectInt> result = new List<RectInt>();
        foreach (RectInt edgeNode in completedRooms)
        {
            //xMax = x + width
            if (edgeNode == currentRoom) continue;
            if (edgeNode.xMax <= currentRoom.x) continue;
            if (edgeNode.yMax <= currentRoom.y) continue;
            if (edgeNode.x >= currentRoom.xMax) continue;
            if (edgeNode.y >= currentRoom.yMax) continue;

            int overlapX = edgeNode.x < currentRoom.x ? overlapX = edgeNode.xMax - currentRoom.x : overlapX = currentRoom.xMax - edgeNode.x;
            int overlapY = edgeNode.y < currentRoom.y ? overlapY = edgeNode.yMax - currentRoom.y : overlapY = currentRoom.yMax - edgeNode.y;
            if (overlapX >= minDoorOverlap + 2 || overlapY >= minDoorOverlap + 2) // + 2 to take into account the thickness of walls
            {
                result.Add(edgeNode);
            }
        }

        return result;
    }

    #endregion

    RectInt FindRoomAtPosition(Vector3 position)
    {
        foreach (RectInt room in completedRooms)
        {
            if (room.x + room.width < position.x) continue;
            if (room.y + room.height < position.z) continue;
            if (room.x > position.x) continue;
            if (room.y > position.z) continue;

            return room;
        }
        return default;
    }

}
