using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Cube : MonoBehaviour
{

    public int xSize, ySize, zSize;

    private Mesh mesh;
    private Vector3[] vertices;

    private void OnEnable()
    {
        Generate();
    }
    
    

    private void Generate()
    {
        var wait = new WaitForSeconds(0.05f);
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Cube";

        CreateVertices();
        CreateTriangles();

        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);


        mesh.uv = uv;
        mesh.tangents = tangents;

        int[] triangles = new int[xSize * ySize * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void CreateVertices()
    {
        int cornerVertices = 8;
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
        int faceVertices = ((xSize - 1) * (ySize - 1) +
        (xSize - 1) * (zSize - 1) +
        (ySize - 1) * (zSize - 1)) * 2;

        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        int v = 0;
        for (int y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                vertices[v++] = new Vector3(x, y, 0);
            }
            for (int z = 1; z <= zSize; z++)
            {
                vertices[v++] = new Vector3(xSize, y, z);
            }
            for (int x = xSize - 1; x >= 0; x--)
            {
                vertices[v++] = new Vector3(x, y, zSize);
            }
            for (int z = zSize - 1; z > 0; z--)
            {
                vertices[v++] = new Vector3(0, y, z);
            }
        }
        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                vertices[v++] = new Vector3(x, ySize, z);
            }
        }
        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                vertices[v++] = new Vector3(x, 0, z);
            }
        }

        mesh.vertices = vertices;
    }

    private void CreateTriangles()
    {

    }

    private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
    {
        triangles[i] = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5] = v11;
        return i + 6;
    }

}
