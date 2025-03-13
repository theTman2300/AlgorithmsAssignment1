using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Graph<T>
{
    private Dictionary<T, List<T>> adjacencyList;
    public Graph() { adjacencyList = new Dictionary<T, List<T>>(); }

    /// <summary>
    /// Add a node to the dictionary.
    /// </summary>
    public void AddNode(T node)
    {
        if (!adjacencyList.ContainsKey(node))
        {
            adjacencyList[node] = new List<T>();
        }
    }

    /// <summary>
    /// Add fromNode and toNode to eaach others list.
    /// This creates an edge between the 2 inputs.
    /// </summary>
    public void AddEdge(T fromNode, T toNode)
    {
        if (!adjacencyList.ContainsKey(fromNode) || !adjacencyList.ContainsKey(toNode))
        {
            Debug.Log("One or both nodes do not exist in the graph.");
            return;
        }
        adjacencyList[fromNode].Add(toNode);
        //adjacencyList[toNode].Add(fromNode);
    }

    public List<T> GetEdgeNodes(T node)
    {
        if (!adjacencyList.ContainsKey(node))
        {
            Debug.Log("Node does not excist in graph");
            return new List<T>(); //return an empty list with no edge nodes
        }
        return adjacencyList[node];
    }

    public void PrintEdgeNodes(T node)
    {
        foreach (T edgeNode in GetEdgeNodes(node))
        {
            Debug.Log(edgeNode);
        }
    }

    /// <summary>
    /// Clears the graph.
    /// </summary>
    public void Clear()
    {
        adjacencyList.Clear();
    }

    public bool ContainsKey(T node)
    {
        return adjacencyList.ContainsKey(node);
    }

    public bool NodeContainsEdgeNode(T node, T edgeNode)
    {
        return adjacencyList[node].Contains(edgeNode);
    }

    public List<T> GetKeys()
    {
        return new List<T>(adjacencyList.Keys);
    }

    //public bool BFS(T startNode) //breadth first search
    //{
    //    //Note: this creates a directional graph
    //    Dictionary<T, List<T>> tempGraph = new Dictionary<T, List<T>>();
    //    List<T> visited = new(); //using a list, because there can't be any duplicate values and I don't know the size beforehand
    //    Queue<T> bfsQueue = new(); //using a queue because otherwise I would have to constantly shift values in an array and I'm not using a list because I am always using the value at the end anyway
    //    bfsQueue.Enqueue(startNode);

    //    visited.Add(startNode);
    //    while (bfsQueue.Count != 0)
    //    {
    //        T node = bfsQueue.Dequeue();
    //        tempGraph[node] = new();
    //        foreach (T edgeNode in adjacencyList[node])
    //        {
    //            if (!visited.Contains(edgeNode))
    //            {
    //                bfsQueue.Enqueue(edgeNode);
    //                visited.Add(edgeNode);
    //                tempGraph[node].Add(edgeNode);
    //            }
    //        }
    //    }
    //    bool result = adjacencyList.Count == tempGraph.Count;
    //    adjacencyList = tempGraph;
    //    return result;
    //}

    public bool DFS(T startNode) //Depth First Search
    {
        //Note: this creates a directional graph
        Dictionary<T, List<T>> tempGraph = new Dictionary<T, List<T>>();
        List<T> visited = new(); //using a list, because there can't be any duplicate values and I don't know the size beforehand
        Stack<T> dfsStack = new(); //using a stack instead of a queue, because it is last in first out so i get actual depth first search
        dfsStack.Push(startNode);

        while (dfsStack.Count != 0)
        {
            T node = dfsStack.Pop();
            if (!visited.Contains(node))
            {
                visited.Add(node);
                if (!tempGraph.Keys.Contains(node)) //add node in dictionary if it does NOT exist already
                    tempGraph[node] = new List<T>(); 
                foreach (T edgeNode in adjacencyList[node])
                {
                    if (!visited.Contains(edgeNode))
                    {
                        if(!dfsStack.Contains(edgeNode))
                        {
                            tempGraph[node].Add(edgeNode);

                            if (!tempGraph.Keys.Contains(edgeNode)) //add edgeNode in dictionary if it does NOT exist already
                                tempGraph[edgeNode] = new();
                            tempGraph[edgeNode].Add(node);
                        }
                        dfsStack.Push(edgeNode);
                    }
                }
            }
        }

        bool result = adjacencyList.Count == tempGraph.Count;
        adjacencyList = new(tempGraph);
        return result;
    }
}
