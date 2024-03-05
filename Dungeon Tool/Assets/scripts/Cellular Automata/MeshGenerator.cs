using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public class Node
    {
        public Vector3 m_position;
        public int vertexIndex = -1;

        public Node(Vector3 pos)
        {
            m_position = pos;
        }
    }

    public class HostNode : Node 
    {
        public bool m_active;
        public Node m_above, m_right;

        public HostNode(Vector3 pos, bool active, float size) : base(pos) 
        {
            m_active = active;
            m_above = new Node(pos+Vector3.forward * size/2f);
            m_right = new Node(pos + Vector3.right * size / 2f);
        }
    }
}
