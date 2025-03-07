using NaughtyAttributes;
using UnityEngine;

public class GraphTester : MonoBehaviour
{
    Graph<string> nodeGraph = new Graph<string>();
    [SerializeField] string currentNode = "A";
    [SerializeField] string toNode = "A";
    [SerializeField] string fromNode = "A";

    [Button]
    void AddNodes()
    {
        nodeGraph.AddNode(currentNode);
    }

    [Button]
    void AddEdge()
    {
        nodeGraph.AddEdge(fromNode, toNode);
    }

    [Button]
    void PrintNeigbours()
    {
        foreach (string node in nodeGraph.GetNeighbours(currentNode))
        {
            print(node);
        }
    }
}
