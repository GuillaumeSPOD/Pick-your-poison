using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation; // Important : c'est là que se trouve NavMeshSurface !

public class NavMeshAutoBake : MonoBehaviour
{
    public NavMeshSurface surface; // À assigner dans l’inspecteur ou automatiquement

    public void UpdateNavMesh()
    {
        if (surface == null)
        {
            surface = GetComponent<NavMeshSurface>();
        }

        if (surface != null)
        {
            surface.BuildNavMesh(); // C'est ici que le bake se fait !
        }
    }
}