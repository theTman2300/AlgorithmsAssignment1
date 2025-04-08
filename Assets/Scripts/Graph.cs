using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Get a list of all edge nodes of a node.
    /// </summary>
    /// <param name="node">Node to get the edges of.</param>
    /// <returns>A list with all edges of a node.</returns>
    public List<T> GetEdgeNodes(T node)
    {
        if (!adjacencyList.ContainsKey(node))
        {
            Debug.Log("Node does not exists in graph");
            return new List<T>(); //return an empty list with no edge nodes
        }
        return adjacencyList[node];
    }

    /// <summary>
    /// Print all edges of a node.
    /// </summary>
    /// <param name="node"></param>
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

    /// <summary>
    /// Check if graph contains node.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <returns>Whether graph contains node.</returns>
    public bool ContainsNode(T node)
    {
        return adjacencyList.ContainsKey(node);
    }

    /// <summary>
    /// Check if node contains edge.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <param name="edgeNode">Edge to check.</param>
    /// <returns>Whether node contains edge.</returns>
    public bool NodeContainsEdgeNode(T node, T edgeNode)
    {
        return adjacencyList[node].Contains(edgeNode);
    }

    /// <summary>
    /// Get a list of all keys in graph.
    /// </summary>
    /// <returns>A list of all keys in graph.</returns>
    public List<T> GetKeys()
    {
        return new List<T>(adjacencyList.Keys);
    }

    /// <summary>
    /// Depth First Search algorithm.
    /// </summary>
    /// <param name="startNode">The node from which the dfs function will start.</param>
    /// <param name="copyNewGraph">When true, copy the new graph created by this method to the graph.</param>
    /// <returns>Whether every node is reachable</returns>
    public bool DFS(T startNode, bool copyNewGraph) //Depth First Search
    {
        //Note: this creates a directional graph
        Dictionary<T, List<T>> tempGraph = new Dictionary<T, List<T>>();
        HashSet<T> visited = new(); //using a hashset because it is faster than a list. (this helped my performance by a few seconds)
        Stack<T> dfsStack = new(); //using a stack because it causes the newest added edge nodes to be processed first, which is dfs
        HashSet<T> wasInStack = new(); //using this hashset to avoid a contains in the dfsStack, which is way slower
        dfsStack.Push(startNode);

        while (dfsStack.Count != 0)
        {
            T node = dfsStack.Pop();
            if (!visited.Contains(node)) //check if current node has not already been processed
            {
                visited.Add(node);
                tempGraph.TryAdd(node, new());

                foreach (T edgeNode in adjacencyList[node])
                {
                    if (!visited.Contains(edgeNode)) //if edge node has NOT been visited
                    {
                        if (!wasInStack.Contains(edgeNode) && copyNewGraph) //if it is not already in the stack. Second check is because edges are not necessary for checking if layout is valid
                        {
                            //add edge nodes in both directions
                            tempGraph[node].Add(edgeNode);

                            tempGraph.TryAdd(edgeNode, new());
                            tempGraph[edgeNode].Add(node);
                        }
                        dfsStack.Push(edgeNode);
                        wasInStack.Add(edgeNode);
                    }
                }
            }
        }

        bool result = adjacencyList.Count == tempGraph.Count; //if all nodes are reachable the count will be the same
        if (copyNewGraph)
            adjacencyList = new(tempGraph);
        return result;
    }
}
