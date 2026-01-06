using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor window to generate custom plane meshes for water surfaces.
/// </summary>
public class WaterMeshGenerator : EditorWindow
{
    float _width = 50f;
    float _length = 50f;
    int _subdivisionsX = 32;
    int _subdivisionsZ = 32;
    string _meshName = "WaterPlane";
    string _savePath = "Assets/DOOM/Meshes";

    [MenuItem("Tools/DOOM/Water Mesh Generator")]
    static void ShowWindow()
    {
        var window = GetWindow<WaterMeshGenerator>("Water Mesh Generator");
        window.minSize = new Vector2(300, 220);
    }

    void OnGUI()
    {
        GUILayout.Label("Water Plane Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        _meshName = EditorGUILayout.TextField("Mesh Name", _meshName);
        _savePath = EditorGUILayout.TextField("Save Path", _savePath);

        EditorGUILayout.Space();
        GUILayout.Label("Dimensions", EditorStyles.boldLabel);

        _width = EditorGUILayout.FloatField("Width", _width);
        _length = EditorGUILayout.FloatField("Length", _length);

        EditorGUILayout.Space();
        GUILayout.Label("Subdivisions (more = smoother waves)", EditorStyles.boldLabel);

        _subdivisionsX = EditorGUILayout.IntSlider("X Subdivisions", _subdivisionsX, 2, 128);
        _subdivisionsZ = EditorGUILayout.IntSlider("Z Subdivisions", _subdivisionsZ, 2, 128);

        int vertexCount = (_subdivisionsX + 1) * (_subdivisionsZ + 1);
        int triCount = _subdivisionsX * _subdivisionsZ * 2;
        EditorGUILayout.HelpBox($"Vertices: {vertexCount}, Triangles: {triCount}", MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate and Save Mesh", GUILayout.Height(30)))
        {
            GenerateAndSaveMesh();
        }
    }

    void GenerateAndSaveMesh()
    {
        Mesh mesh = GeneratePlaneMesh(_width, _length, _subdivisionsX, _subdivisionsZ);
        mesh.name = _meshName;

        // Ensure directory exists
        if (!Directory.Exists(_savePath))
        {
            Directory.CreateDirectory(_savePath);
            AssetDatabase.Refresh();
        }

        string fullPath = $"{_savePath}/{_meshName}.asset";

        // Check if asset already exists
        Mesh existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(fullPath);
        if (existingMesh != null)
        {
            if (!EditorUtility.DisplayDialog("Overwrite?",
                $"Mesh '{_meshName}' already exists. Overwrite?", "Yes", "No"))
            {
                return;
            }
            AssetDatabase.DeleteAsset(fullPath);
        }

        AssetDatabase.CreateAsset(mesh, fullPath);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = mesh;

        Debug.Log($"Water mesh saved to: {fullPath}");
    }

    static Mesh GeneratePlaneMesh(float width, float length, int subdX, int subdZ)
    {
        Mesh mesh = new Mesh();

        int vertsX = subdX + 1;
        int vertsZ = subdZ + 1;
        int vertCount = vertsX * vertsZ;

        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        Vector3[] normals = new Vector3[vertCount];

        float halfWidth = width * 0.5f;
        float halfLength = length * 0.5f;

        // Generate vertices
        for (int z = 0; z < vertsZ; z++)
        {
            for (int x = 0; x < vertsX; x++)
            {
                int i = z * vertsX + x;

                float xPos = (x / (float)subdX) * width - halfWidth;
                float zPos = (z / (float)subdZ) * length - halfLength;

                vertices[i] = new Vector3(xPos, 0, zPos);
                uvs[i] = new Vector2(x / (float)subdX, z / (float)subdZ);
                normals[i] = Vector3.up;
            }
        }

        // Generate triangles
        int[] triangles = new int[subdX * subdZ * 6];
        int triIndex = 0;

        for (int z = 0; z < subdZ; z++)
        {
            for (int x = 0; x < subdX; x++)
            {
                int bottomLeft = z * vertsX + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + vertsX;
                int topRight = topLeft + 1;

                // First triangle
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = topRight;

                // Second triangle
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topRight;
                triangles[triIndex++] = bottomRight;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();

        return mesh;
    }
}
