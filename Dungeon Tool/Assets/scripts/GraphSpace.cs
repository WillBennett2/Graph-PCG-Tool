using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Graph;

public class GraphSpace : MonoBehaviour
{
    [SerializeField] private GameObject placeholder;
    [SerializeField] private GameObject edgePlaceholder;
    [SerializeField] private GameObject dirEdgePlaceholder;
    private GameObject container;
    // Start is called before the first frame update
    void Start()
    {
        container = new GameObject();
        container.name = "Container";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateSpace(List<Index2NodeDataLinker> nodes, List<Index2StoredNodeDataLinker> storedNodes, List<Index2EdgeDataLinker> edges)
    {
        foreach(Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }


        Debug.Log("creating space.");
        foreach (Index2NodeDataLinker node in nodes)
        {
            GameObject temp = Instantiate(placeholder, node.nodeData.position, Quaternion.identity);
            temp.transform.SetParent(container.transform);
            temp.GetComponentInChildren<Renderer>().material.color = node.nodeData.colour;
        }
        foreach (Index2StoredNodeDataLinker storedNode in storedNodes)
        {
            GameObject temp = Instantiate(placeholder, storedNode.storedNodeData.position, Quaternion.identity);
            temp.transform.SetParent(container.transform);
            temp.GetComponentInChildren<Renderer>().material.color = storedNode.storedNodeData.colour;
        }
        foreach (Index2EdgeDataLinker edge in edges)
        {
            edge.edgeData.rotation = GraphInfo.graphInfo.SetRotation(edge.edgeData);
            GameObject temp = null;
            if (edge.edgeData.directional)
            {
                temp = Instantiate(dirEdgePlaceholder, edge.edgeData.position, edge.edgeData.rotation);
            }
            else if (edge.edgeData.directional==false)
            {
                temp = Instantiate(edgePlaceholder, edge.edgeData.position, edge.edgeData.rotation);
            }
                
            temp.transform.SetParent(container.transform);
        }

    }
}
