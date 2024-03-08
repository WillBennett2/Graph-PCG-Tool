using JetBrains.Annotations;
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
            m_above = new Node(pos + Vector3.forward * size / 2f);
            m_right = new Node(pos + Vector3.right * size / 2f);
        }
    }
    public class Square
    {
        public HostNode m_topLeft, m_topRight, m_bottomRight, m_bottomLeft;
        public Node m_centreTop, m_centreRight, m_centreBottom, m_centreLeft;
        public int m_config;
        public Square(HostNode topLeft, HostNode topRight, HostNode bottomRight, HostNode bottomLeft)
        {
            m_topLeft = topLeft;
            m_topRight = topRight;
            m_bottomRight = bottomRight;
            m_bottomLeft = bottomLeft;

            m_centreTop = m_topLeft.m_right;
            m_centreRight = m_bottomRight.m_above;
            m_centreBottom = m_bottomLeft.m_right;
            m_centreLeft = m_bottomLeft.m_above;

            if (m_topLeft.m_active)
                m_config += 8;
            if (m_topRight.m_active)
                m_config += 4;
            if (m_bottomRight.m_active)
                m_config += 2;
            if (m_bottomLeft.m_active)
                m_config += 1;
        }
    }
    public class SquareGrid
    {
        public Square[,] m_squares;
        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            HostNode[,] hostNodes = new HostNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    hostNodes[x, y] = new HostNode(pos, map[x, y] == 1, squareSize);

                }
            }
            m_squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    m_squares[x, y] = new Square(hostNodes[x, y + 1], hostNodes[x + 1, y + 1], hostNodes[x + 1, y], hostNodes[x, y]);

                }
            }
        }
    }

    struct Triangle
    {
        public int m_vertexIndexA;
        public int m_vertexIndexB;
        public int m_vertexIndexC;
        public int[] m_vertices;

        public Triangle(int indexA, int indexB, int indexC)
        {
            m_vertexIndexA = indexA;
            m_vertexIndexB = indexB;
            m_vertexIndexC = indexC;

            m_vertices = new int[3];
            m_vertices[0] = m_vertexIndexA;
            m_vertices[1] = m_vertexIndexB;
            m_vertices[2] = m_vertexIndexC;
        }

        public int this[int i]
        {
            get
            { 
                return m_vertices [i];
            }
        }

        public bool Contains(int vertexIndex)
        {
            if (vertexIndex == m_vertexIndexA || vertexIndex == m_vertexIndexB || vertexIndex == m_vertexIndexC)
                return true;
            else
                return false;
        }
    }

    public SquareGrid m_squareGrid;
    List<Vector3> m_vertices;
    List<int>  m_triangles;

    Dictionary<int, List<Triangle>> m_triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> m_outlineEdges = new List<List<int>>();
    HashSet<int> m_clearedVertices = new HashSet<int>();

    public MeshFilter m_walls;
    [SerializeField] private float m_wallHeight = 5f;

    public void GenerateMesh(int[,] map, float squareSize)
    {
        m_triangleDictionary.Clear();
        m_outlineEdges.Clear();
        m_clearedVertices.Clear();

        m_squareGrid = new SquareGrid(map, squareSize);

        m_vertices = new List<Vector3>();
        m_triangles = new List<int>();

        for (int x = 0; x < m_squareGrid.m_squares.GetLength(0); x++)
        {
            for (int y = 0; y < m_squareGrid.m_squares.GetLength(1); y++)
            {
                TriangulateSquare(m_squareGrid.m_squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = m_vertices.ToArray();
        mesh.triangles = m_triangles.ToArray();
        mesh.RecalculateNormals();

        CreateWallMesh();
    }
    void CreateWallMesh()
    {
        MeshOutlines();
        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangle = new List<int>();
        Mesh wallMesh = new Mesh();

        foreach (List<int> outline in m_outlineEdges) 
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(m_vertices[outline[i + 1]]);//left
                wallVertices.Add(m_vertices[outline[i]]);//right
                wallVertices.Add(m_vertices[outline[i + 1]] - Vector3.up * m_wallHeight);//bottomleft
                wallVertices.Add(m_vertices[outline[i]] - Vector3.up * m_wallHeight);//bottomright

                wallTriangle.Add(startIndex + 0);
                wallTriangle.Add(startIndex + 2);
                wallTriangle.Add(startIndex + 3);

                wallTriangle.Add(startIndex + 3);
                wallTriangle.Add(startIndex + 1);
                wallTriangle.Add(startIndex + 0);
            }
        }

        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangle.ToArray();
        m_walls.mesh = wallMesh;
    }

    void TriangulateSquare(Square square)
    {
        switch (square.m_config)
        {
            case 0:
                break;

            // 1 point
            case 1:
                MeshFromPoints(square.m_centreLeft, square.m_centreBottom, square.m_bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.m_bottomRight, square.m_centreBottom, square.m_centreRight);
                break;
            case 4:
                MeshFromPoints(square.m_topRight, square.m_centreRight, square.m_centreTop);
                break;
            case 8:
                MeshFromPoints(square.m_topLeft, square.m_centreTop, square.m_centreLeft);
                break;

            // 2 point
            case 3:
                MeshFromPoints(square.m_centreRight, square.m_bottomRight, square.m_bottomLeft, square.m_centreLeft);
                break;
            case 6:
                MeshFromPoints(square.m_centreTop, square.m_topRight, square.m_bottomRight, square.m_centreBottom);
                break;
            case 9:
                MeshFromPoints(square.m_topLeft, square.m_centreTop, square.m_centreBottom, square.m_bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.m_topLeft, square.m_topRight, square.m_centreRight, square.m_centreLeft);
                break;
            case 5:
                MeshFromPoints(square.m_centreTop, square.m_topRight, square.m_centreRight, square.m_centreBottom, square.m_bottomLeft ,square.m_centreLeft);
                break;
            case 10:
                MeshFromPoints(square.m_topLeft, square.m_centreTop, square.m_centreRight, square.m_bottomRight, square.m_centreBottom, square.m_centreLeft);
                break;

            // 3 point
            case 7:
                MeshFromPoints(square.m_centreTop, square.m_topRight, square.m_bottomRight, square.m_bottomLeft, square.m_centreLeft);
                break;
            case 11:
                MeshFromPoints(square.m_topLeft, square.m_centreTop, square.m_centreRight, square.m_bottomRight, square.m_bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.m_topLeft, square.m_topRight, square.m_centreRight, square.m_centreBottom, square.m_bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.m_topLeft, square.m_topRight, square.m_bottomRight, square.m_centreBottom, square.m_centreLeft);
                break;

            // 4 points
            case 15:
                MeshFromPoints(square.m_topLeft,square.m_topRight,square.m_bottomRight,square.m_bottomLeft);
                m_clearedVertices.Add(square.m_topLeft.vertexIndex);
                m_clearedVertices.Add(square.m_topRight.vertexIndex);
                m_clearedVertices.Add(square.m_bottomLeft.vertexIndex);
                m_clearedVertices.Add(square.m_bottomRight.vertexIndex);
                break;
        }
    }
    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if(points.Length>=3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);
    }
    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex==-1)//unassigned
            {
                points[i].vertexIndex = m_vertices.Count;
                m_vertices.Add(points[i].m_position);
            }
        }
    }
    
    void CreateTriangle(Node a, Node b, Node c)
    {
        m_triangles.Add(a.vertexIndex);
        m_triangles.Add(b.vertexIndex);
        m_triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex,b.vertexIndex,c.vertexIndex);
        StoreTriangle(triangle.m_vertexIndexA,triangle);
        StoreTriangle(triangle.m_vertexIndexB, triangle);
        StoreTriangle(triangle.m_vertexIndexC, triangle);
    }
    private void StoreTriangle(int vertexIndexKey, Triangle triangle)
    {
        if(m_triangleDictionary.ContainsKey(vertexIndexKey))
            m_triangleDictionary[vertexIndexKey].Add(triangle);
        else//create key if not already in.
        {
            List<Triangle> triangleList = new List<Triangle>
            {
                triangle
            };
            m_triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    void MeshOutlines()
    {
        for (int i = 0; i < m_vertices.Count; i++)
        {
            if(!m_clearedVertices.Contains(i))
            {
                int newOutlineVertex = GetOutlineVertex(i);
                if(newOutlineVertex != -1)
                {
                    m_clearedVertices.Add(newOutlineVertex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(i);
                    m_outlineEdges.Add(newOutline);
                    FollowOutline(newOutlineVertex, m_outlineEdges.Count - 1);
                    m_outlineEdges[m_outlineEdges.Count-1].Add(i);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        m_outlineEdges[outlineIndex].Add(vertexIndex);
        m_clearedVertices.Add(vertexIndex);
        int nextVertexIndex = GetOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
            FollowOutline(nextVertexIndex, outlineIndex);

    }

    int GetOutlineVertex(int vertexIndex)
    {
        List<Triangle> triangleWithParamVertex = m_triangleDictionary[vertexIndex];

        for (int i = 0; i < triangleWithParamVertex.Count; i++)
        {
            Triangle triangle = triangleWithParamVertex[i];
            for (int j = 0; j < 3; j++)
            {
                int secondVertex = triangle[j];
                if (secondVertex != vertexIndex && !m_clearedVertices.Contains(secondVertex))
                    if(OutlineEdgeCheck(vertexIndex,secondVertex))
                        return secondVertex;
            }
        }
        return -1;//was not an outline edge
    }

    bool OutlineEdgeCheck(int vertexA, int vertexB) // if a shared vertex is in both triangles then its an outer edge ( has to only have 1 trianlge in common)
    {
        List<Triangle> vertexATriangles = m_triangleDictionary[vertexA];
        int sharedCount = 0;

        for(int i=0; i < vertexATriangles.Count;i++)
        {
            if (vertexATriangles[i].Contains(vertexB))
            {
                sharedCount++;
                if (sharedCount > 1)
                    break;
            }
        }
        if(sharedCount == 1)
            return true;
        else
            return false;
    }
    //private void OnDrawGizmos()
    //{
    //    //if (m_squareGrid != null)
    //    //{
    //    //    for (int x = 0; x < m_squareGrid.m_squares.GetLength(0); x++)
    //    //    {
    //    //        for (int y = 0; y < m_squareGrid.m_squares.GetLength(1); y++)
    //    //        {
    //    //            Gizmos.color = (m_squareGrid.m_squares[x, y].m_topLeft.m_active)?Color.black:Color.white;
    //    //            Gizmos.DrawCube(m_squareGrid.m_squares[x, y].m_topLeft.m_position, Vector3.one * 0.4f);

    //    //            Gizmos.color = (m_squareGrid.m_squares[x, y].m_topRight.m_active) ? Color.black : Color.white;
    //    //            Gizmos.DrawCube(m_squareGrid.m_squares[x, y].m_topRight.m_position, Vector3.one * 0.4f);

    //    //            Gizmos.color = (m_squareGrid.m_squares[x, y].m_bottomRight.m_active) ? Color.black : Color.white;
    //    //            Gizmos.DrawCube(m_squareGrid.m_squares[x, y].m_bottomRight.m_position, Vector3.one * 0.4f);

    //    //            Gizmos.color = (m_squareGrid.m_squares[x, y].m_bottomLeft.m_active) ? Color.black : Color.white;
    //    //            Gizmos.DrawCube(m_squareGrid.m_squares[x, y].m_bottomLeft.m_position, Vector3.one * 0.4f);

    //    //            Gizmos.color = Color.grey;
    //    //            Gizmos.DrawCube(m_squareGrid.m_squares[x, y].m_centreTop.m_position, Vector3.one * 0.15f);
    //    //            Gizmos.DrawCube(m_squareGrid.m_squares[x, y].m_centreRight.m_position, Vector3.one * 0.15f);
    //    //            Gizmos.DrawCube(m_squareGrid.m_squares[x, y].m_centreBottom.m_position, Vector3.one * 0.15f);
    //    //            Gizmos.DrawCube(m_squareGrid.m_squares[x, y].m_centreLeft.m_position, Vector3.one * 0.15f);
    //    //        }
    //    //    }
    //    //}
    //}


}
