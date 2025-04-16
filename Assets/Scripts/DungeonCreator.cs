using JetBrains.Annotations;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    [Tooltip("The size of the outer bounds of the dungeon.")]
    public RectInt DungeonBounds;
    //even though only width and height are only taken into account, I still use RectInt instead of Vector2Int because is think it looks better when it actually says width and height in the inspector
    [Tooltip("Used to avoid spiral pattern when creating larger dungeons. \nMust always be bigger then minSize + 2, but recommended to be somewhere around 3/4 of dungeonBounds. \nOnly width and height are taken into account.")]
    [SerializeField] RectInt maxGenerationSize;
    [Tooltip("Rooms will always be smaller then this value. \nOnly width and height are taken into account.")]
    [SerializeField] RectInt maxRoomSize;
    [Tooltip("Rooms will always be larger then this value. \nOnly width and height are taken into account.")]
    [SerializeField] RectInt minRoomSize;
    [Tooltip("the minimum overlap to make a graph edge between the rooms.")] //both x and y axis use this vale
    [SerializeField] int minDoorOverlap = 4;
    [SerializeField] int doorLength;

    [HorizontalLine]
    [SerializeField] int seed = 0;
    [SerializeField] float secondsPerOperation = .5f;
    [Tooltip("Skip the animation to generate the rooms as fast as possible")]
    [SerializeField] bool generateFast = false;
    [Tooltip("This is reserved for any gameObject to more easily debug rooms.")]

    [HorizontalLine]
    [SerializeField] bool drawCompletedRooms = true;
    [SerializeField] bool drawDoors = true;
    [SerializeField] bool drawGraph = true;
    [SerializeField, ReadOnly] List<RectInt> rooms = new List<RectInt>(); //rooms to be split
    [SerializeField, ReadOnly] List<RectInt> newRooms = new List<RectInt>(); //rooms that have been split this loop
    [SerializeField, ReadOnly] List<RectInt> completedRooms = new List<RectInt>(); //completed rooms  
    [SerializeField, ReadOnly] List<RectInt> doors = new List<RectInt>(); //doors between rooms
    RectInt currentWorkingRoom = default; //current room being split
    System.Random rng; //this is used because unity random seed doesn't work when using ienumerators

    Graph<RectInt> roomGraph = new Graph<RectInt>();
    RectInt selectedRoom = default; //used by roomCursor

    void Start()
    {
        ResetDungeon();
    }

    [Button]
    /// <summary>
    /// Clears the previous dungeon and restarts the generation process with the current settings.
    /// </summary>
    private void ResetDungeon()
    {
        StopAllCoroutines(); //in event that dungeon generator was still running when this function is called
        rooms.Clear();
        newRooms.Clear();
        completedRooms.Clear();
        roomGraph.Clear();
        doors.Clear();
        selectedRoom = default;
        drawGraph = false; //stays false until final graph is completed, otherwise you would see an incorrect graph while rooms are being removed. Also only shown after doors are done to make the door generation more visible

        //set orthographic camera size and position to fit dungeon
        Camera.main.orthographicSize = DungeonBounds.width > DungeonBounds.height ? DungeonBounds.width / 2 + 2 : DungeonBounds.height / 2 + 2; //+ 2 for some padding
        Camera.main.transform.position = new(DungeonBounds.width / 2, Camera.main.transform.position.y, DungeonBounds.height / 2);

        //set initial room
        //rooms.Add(new RectInt(DungeonBounds.x + 1, DungeonBounds.y + 1, DungeonBounds.width - 2, DungeonBounds.height - 2));
        rooms.Add(DungeonBounds);

        //check whether maxGenerationSize is bigger then minimum
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
        if (drawCompletedRooms)
        {
            foreach (RectInt room in completedRooms)
            {
                AlgorithmsUtils.DebugRectInt(room, Color.white); //completed rooms
            }
        }
        //AlgorithmsUtils.DebugRectInt(DungeonBounds, Color.white); //dungeon outline

        foreach (RectInt room in newRooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green); //rooms that have been split this loop
        }
        AlgorithmsUtils.DebugRectInt(currentWorkingRoom, Color.cyan); //current room being split

        //show graph
        if (drawGraph)
        {
            foreach (RectInt room in completedRooms)
            {
                if (!roomGraph.ContainsNode(room)) continue;

                Vector3 roomMiddle = new Vector3(room.x + room.width / 2, 0, room.y + room.height / 2);
                foreach (RectInt connectedRoom in roomGraph.GetEdgeNodes(room))
                {
                    Vector3 connectedMiddle = new Vector3(connectedRoom.x + connectedRoom.width / 2, 0, connectedRoom.y + connectedRoom.height / 2);
                    Debug.DrawLine(roomMiddle, connectedMiddle, Color.magenta);
                }
            }
        }
        if (drawDoors)
        {
            foreach (RectInt door in doors)
            {
                AlgorithmsUtils.DebugRectInt(door, Color.cyan);
            }
        }

        if (Input.GetMouseButtonUp(0))
            SelectRoom();

        //room cursor
        AlgorithmsUtils.DebugRectInt(selectedRoom, Color.red);

    }
    void SelectRoom()
    {

        if (selectedRoom == default || selectedRoom != FindRoomAtPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
        {
            selectedRoom = FindRoomAtPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Debug.Log("Selected Room " + selectedRoom);
            roomGraph.PrintEdgeNodes(selectedRoom);
        }
        else
        {
            selectedRoom = default;
        }
    }

    #region create basic layout (step 1)
    /// <summary>
    /// Splits the dungeon bounds into rooms according to the current settings.
    /// </summary>
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
        StartCoroutine(FinalizeDungeonLayout());
    }

    /// <summary>
    /// Splits a room along the vertical axis.
    /// </summary>
    /// <param name="roomIndex">Index of the room to split in the list rooms[]</param>
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

    /// <summary>
    /// Splits a room along the horizontal axis.
    /// </summary>
    /// <param name="roomIndex">Index of the room to split in the list rooms[]</param>
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
    /// <summary>
    /// Removes 10% of the smallest rooms if possible, then create connections between rooms without loops.
    /// </summary>
    IEnumerator FinalizeDungeonLayout()
    {
        //sort rooms by smallest area
        completedRooms = new(completedRooms.OrderBy(room => room.width * room.height));
        SetRoomGraph();

        //remove 10% of smallest rooms
        for (int i = 0; i < completedRooms.Count * .1f; i++)
        {
            RectInt roomToRemove = completedRooms[0];
            completedRooms.RemoveAt(0);

            bool roomsAreReachable = false;
            roomsAreReachable = roomGraph.DFS(roomToRemove, false);

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

        if (!generateFast)
            yield return new WaitForSeconds(secondsPerOperation);
        SetRoomGraph(); //set all the new nodes and edges after removal to make sure dfs is able to create the correct graph

        Debug.Log(completedRooms[0]); //log where dfs starts
        Debug.Log("All rooms reachable DFS: " + roomGraph.DFS(completedRooms[0], true)); //last graph check, should always return true
        Debug.Log("Graph creation done");
        StartCoroutine(CreateDoors());
    }

    /// <summary>
    /// Sets all the nodes and edges of those nodes in a graph.
    /// </summary>
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

    /// <summary>
    /// Creates all the doors on all the node edges.
    /// </summary>
    IEnumerator CreateDoors()
    {
        List<RectInt> roomsWithDoor = new();
        foreach (RectInt room in roomGraph.GetKeys()) //using GetKeys to go in direction of the graph, otherwise there would be doors that don't generate
        {
            roomsWithDoor.Add(room); //keep track of doors that have already been added
            foreach (RectInt edgeNode in roomGraph.GetEdgeNodes(room))
            {
                //check if door already exists
                if (roomsWithDoor.Contains(edgeNode)) continue;

                RectInt overlap = AlgorithmsUtils.Intersect(room, edgeNode);
                if (overlap.width > overlap.height)
                {
                    //horizontal door

                    RectInt door = new(0, 0, doorLength, 1);
                    door.y = edgeNode.yMax < room.yMax ? room.y : room.yMax - 1;
                    door.x = rng.Next(Mathf.Min(room.xMax, edgeNode.xMax) - overlap.width + 1, Mathf.Min(room.xMax, edgeNode.xMax) - doorLength - 1);
                    doors.Add(door);
                }
                else
                {
                    //vertical door

                    RectInt door = new(0, 0, 1, doorLength);
                    door.x = edgeNode.x < room.x ? room.x : room.xMax - 1;
                    door.y = rng.Next(Mathf.Min(room.yMax, edgeNode.yMax) - overlap.height + 1, Mathf.Min(room.yMax, edgeNode.yMax) - doorLength - 1);
                    doors.Add(door);
                }
            }
            if (!generateFast)
                yield return new WaitForSeconds(secondsPerOperation);
        }

        Debug.Log("Door creation done");
        drawGraph = true;
    }

    /// <summary>
    /// Gets a list of all the rooms intersecting with the provided room, excluding itself, and taking minDoorOverlap into account.
    /// </summary>
    /// <param name="currentRoom">The room to find the intersections of.</param>
    /// <returns>A list with all rooms that intersect CurrentRoom that have an overlap >= minDoorOverlap.</returns>
    List<RectInt> GetIntersectingRooms(RectInt currentRoom)
    {
        List<RectInt> result = new List<RectInt>();
        foreach (RectInt edgeNode in completedRooms)
        {
            if (edgeNode == currentRoom) continue; //ignore itself
            if (edgeNode.xMax <= currentRoom.x) continue;
            if (edgeNode.yMax <= currentRoom.y) continue;
            if (edgeNode.x >= currentRoom.xMax) continue;
            if (edgeNode.y >= currentRoom.yMax) continue;

            RectInt overlap = AlgorithmsUtils.Intersect(currentRoom, edgeNode);
            if (overlap.width >= minDoorOverlap + 2 || overlap.height >= minDoorOverlap + 2) // + 2 to take into account the thickness of walls
            {
                result.Add(edgeNode);
            }
        }

        return result;
    }

    #endregion

    /// <summary>
    /// Finds a room at a position using the x and z coordinates.
    /// Returns default of no room at that position.
    /// </summary>
    /// <param name="position">Position to find a room at.</param>
    /// <returns>A room at that position.</returns>
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

    public (RectInt[], RectInt[]) GetRoomsAndDoors()
    {
        return (completedRooms.ToArray(), doors.ToArray());
    }
}
