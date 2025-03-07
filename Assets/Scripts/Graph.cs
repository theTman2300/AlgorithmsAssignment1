using System.Collections.Generic;
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
        if ( ! adjacencyList.ContainsKey(node))
        {
            adjacencyList[node] = new List<T>();
        }
    }

    /// <summary>
    /// Add fromNode and toNode to eaach others list.
    /// This creates an edge between the 2 inputs
    /// </summary>
    public void AddEdge(T fromNode, T toNode)
    {
        if ( ! adjacencyList.ContainsKey(fromNode) || ! adjacencyList.ContainsKey(toNode))
        {
            Debug.Log("One or both nodes do not exist in the graph.");
            return;
        }
        adjacencyList[fromNode].Add(toNode);
        adjacencyList[toNode].Add(fromNode);
    }

    public List<T> GetNeighbours(T node)
    {
        if ( ! adjacencyList.ContainsKey(node))
        {
            Debug.Log("Node does not excist in graph");
            return new List<T>(); //return an empty list with no neighbours
        }
        return adjacencyList[node];
    }
}
