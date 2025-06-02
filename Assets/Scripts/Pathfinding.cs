using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Pathfinding : MonoBehaviour
{
    enum PathfindMethod { navMesh };
    [SerializeField] PathfindMethod pathMethod;

    NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            SetGoal();
        }
    }

    void SetGoal()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(mouseRay, out RaycastHit hit))
            return;

        print(hit.point);
        agent.SetDestination(hit.point);
    }

}
