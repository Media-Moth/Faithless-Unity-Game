using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class NewMapGeneration : MonoBehaviour
{
    static int Rings = 3;
    static int minSize = 2;
    static int maxSize = 17;
    static int minRingSize = 20;

    static List<Vector3> Vertices = new List<Vector3>();
    static List<int> triangles = new List<int>();

    static Vector2 lastSquare = new Vector2(-1,-1);

    // we sohuld generate floors in a circle
    // with some corriders moving closer to the centre
    // and the centre being the last room to exit the floor
    // some rooms will have teleporters that transport you to distance rooms
    static void generateTriangles()
    {
        
    }
    static void generateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.uv = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            mesh.uv[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
        }
        GameObject meshObject = new GameObject("Mesh");
        meshObject.AddComponent<MeshFilter>();
        meshObject.AddComponent<MeshRenderer>();

        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        Material newMaterial = Resources.Load<Material>("MushToon");
        // Get the object's renderer
        Renderer renderer = meshObject.GetComponent<Renderer>();
        // Apply the material to the renderer
        renderer.material = newMaterial;

        MeshCollider collider = meshObject.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void OnBeforeSplashScreen()
    {
        // this runs before the game starts
        for (int i = 0; i < Rings; i++)
        {


        }
        generateTriangles();
        generateMesh();
        //generateDebugGrid();
    }

}