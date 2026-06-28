using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NodeGridGenerator : MonoBehaviour
{
    [Header("Grilla")]
    public int columns = 15;
    public int rows = 15;
    public float spacing = 3f;

    [Header("Filtros")]
    public bool onlyOnNavMesh = true;
    public float obstacleCheckRadius = 0.5f;
    public string obstacleLayer = "Obstaculos";

    [Header("Conexiones")]
    public float connectionRadius = 5f;

#if UNITY_EDITOR
    [ContextMenu("Generar Nodos")]
    public void GenerateNodes()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        List<Node> createdNodes = new List<Node>();
        Vector3 origin = transform.position - new Vector3(columns * spacing / 2f, 0, rows * spacing / 2f);

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 pos = origin + new Vector3(x * spacing, 0, z * spacing);

                if (onlyOnNavMesh)
                {
                    NavMeshHit navHit;
                    if (!NavMesh.SamplePosition(pos, out navHit, 1.5f, NavMesh.AllAreas))
                        continue;
                    pos = navHit.position;
                }

                Collider[] hits = Physics.OverlapSphere(pos, obstacleCheckRadius);
                bool blocked = false;
                foreach (Collider c in hits)
                {
                    if (c.gameObject.layer == LayerMask.NameToLayer(obstacleLayer))
                    {
                        blocked = true;
                        break;
                    }
                }
                if (blocked) continue;

                GameObject nodeGO = new GameObject($"Node_{x}_{z}");
                nodeGO.transform.position = pos;
                nodeGO.transform.parent = transform;

                int nodeLayer = LayerMask.NameToLayer("Node");
                if (nodeLayer != -1)
                    nodeGO.layer = nodeLayer;
                else
                    Debug.LogWarning("Creá la layer 'Node'.");

                SphereCollider sc = nodeGO.AddComponent<SphereCollider>();
                sc.radius = 0.3f;
                sc.isTrigger = true;

                Node node = nodeGO.AddComponent<Node>();
                createdNodes.Add(node);
            }
        }
        int wallMask = LayerMask.GetMask(obstacleLayer);
        foreach (Node node in createdNodes)
        {
            foreach (Node other in createdNodes)
            {
                if (node == other) continue;

                float dist = Vector3.Distance(node.transform.position, other.transform.position);
                if (dist > connectionRadius) continue;

                Vector3 dir = other.transform.position - node.transform.position;
                if (Physics.Raycast(node.transform.position + Vector3.up * 0.5f, dir.normalized, dist, wallMask))
                    continue;

                if (!node.neightbourds.Contains(other))
                    node.neightbourds.Add(other);
            }
        }

        Debug.Log($"Generados {createdNodes.Count} nodos.");
    }

    [ContextMenu("Borrar Nodos")]
    public void ClearNodes()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
        Debug.Log("Nodos borrados.");
    }
#endif
}