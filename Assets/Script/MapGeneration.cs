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
    static int minSize = 3;
    static int maxSize = 9;
    static int rooms = 100;
    static int radius = 10;
    static int tileSize = 1;
    static int iterations = 5000;

    static List<Vector3> Vertices = new List<Vector3>();
    static List<int> triangles = new List<int>();
    static List<room> nodes = new List<room>();

    // we sohuld generate floors in a circle
    // with some corriders moving closer to the centre
    // and the centre being the last room to exit the floor
    // some rooms will have teleporters that transport you to distance rooms
    struct room
    {
        public Vector2 position;
        public int width;
        public int height;
    }
    static void generateTriangles()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            room currentRoom = nodes[i];
            // clockwise direction (or so i thought)
            int[] xdir = { 1, -1, -1, 1 };
            int[] ydir = {1, 1, -1, -1};
            for (int j = 0; j < 4; j++)
            {
                Vector3 vert = new Vector3(
                    currentRoom.position.x + (currentRoom.width - 1) / 2 * xdir[j],
                    0,
                    currentRoom.position.y + (currentRoom.height - 1) / 2 * ydir[j]
                );
                Vertices.Add(vert);
                if (j == 2)
                {
                    // other way makes it face downwards idk why
                    triangles.Add(i * 4 + 2);
                    triangles.Add(i * 4 + 1);
                    triangles.Add(i * 4);
                }
                if (j == 3)
                {
                    triangles.Add(i * 4 + 3);
                    triangles.Add(i * 4 + 2);
                    triangles.Add(i * 4);
                    Debug.DrawLine(vert, Vertices[i * 4 + 2], Color.green, 100f);
                    Debug.DrawLine(Vertices[i*4+2], Vertices[i * 4 + 1], Color.green, 100f);
                    Debug.DrawLine(Vertices[i*4+1], Vertices[i * 4], Color.green, 100f);
                    Debug.DrawLine(Vertices[i * 4], vert, Color.green, 100f);
                }
            }
        }
    }
    static void generateRoom()
    {
        room newRoom = new room();
        newRoom.position = generatePoint();
        newRoom.width = Random.Range(minSize, maxSize)*2 - 1; // make it odd number
        newRoom.height = Random.Range(minSize, maxSize)*2 -1;
        nodes.Add(newRoom);
    }
    static bool isRoomColliding(room a, room b)
    {
        bool yCollide = a.position.y - (a.height - 1) / 2 < b.position.y + (b.height - 1) / 2 && b.position.y - (b.height - 1) / 2 < a.position.y + (a.height - 1) / 2;
        bool xCollide = a.position.x - (a.width - 1) / 2 < b.position.x + (b.width - 1) / 2 && b.position.x - (b.width - 1) / 2 < a.position.x + (a.width - 1) / 2;
        return xCollide && yCollide;
    }
    static void moveRooms()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            room currentRoom = nodes[i];
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue;
                room otherRoom = nodes[j];
                if (isRoomColliding(currentRoom, otherRoom))
                {
                    int w = 0;
                    while (isRoomColliding(currentRoom, otherRoom) && w<11)
                    {
                        w++;
                        // move away from the other room
                        if (otherRoom.position.x < currentRoom.position.x)
                        {
                            otherRoom.position.x += -tileSize;
                        }
                        else if (otherRoom.position.x > currentRoom.position.x)
                        {
                            otherRoom.position.x += tileSize;
                        }
                        if (otherRoom.position.y < currentRoom.position.y)
                        {
                            otherRoom.position.y += -tileSize;
                        }
                        else if (otherRoom.position.y > currentRoom.position.y)
                        {
                            otherRoom.position.y += tileSize;
                        }
                        else
                        {
                            // if they are in the same position, move randomly
                            otherRoom.position += new Vector2(roundToTile(Random.value * tileSize), roundToTile(Random.value * tileSize));
                        }
                    }
                    nodes[j] = otherRoom;
                }
            }
            //currentRoom.position += new Vector2 (roundToTile(velocity.x), roundToTile(velocity.y));
            //nodes[i] = currentRoom;
        }
        Debug.Log(nodes[0].position);
    }
    static int roundToTile(float value)
    {
        return (int)Math.Floor((value + tileSize - 1)/ tileSize) * tileSize;
    }
    static Vector2 generatePoint()
    {
        float t = 2f * (float)Math.PI * Random.value;
        float u = Random.value + Random.value;
        float r = 0f;
        if (u > 1f)
        {
            r = 2f - u;
        }
        else
        {
            r = u;
        }
        return new Vector2(roundToTile(r * 20 * Mathf.Cos(t)), roundToTile(r * 20 * Mathf.Sin(t)));
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
        for (int i = 0; i < rooms; i++)
        {
            generateRoom();
        }
        for (int i = 0; i < iterations; i++)
        {
            moveRooms();
        }
        generateTriangles();
        generateMesh();
        //generateDebugGrid();
    }

}