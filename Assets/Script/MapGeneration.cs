using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class MapGeneration : MonoBehaviour
{
    static int Rooms = 60;
    static int minSize = 6;
    static int maxSize = 10;
    static int LevelSize = 50;

    static bool[,] filled = new bool[LevelSize + maxSize, LevelSize + maxSize];
    static List<Vector3> Vertices = new List<Vector3>();
    static List<int> triangles = new List<int>();

    static int generateFloor(int x,int y,int sizeX,int sizeY)
    {
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                if (filled[x+i,y+j] == true)
                {
                    return -1;
                }
            }
        }



        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Quad);
        cube.transform.Rotate(90, 0, 0);
        cube.transform.localScale = new Vector3(sizeX, sizeY, 1);
        //because origin is centre of block instead of bottom left
        cube.transform.localPosition = new Vector3(x + ((float)sizeX/2), -0f, y + ((float)sizeY/2));
        
        for (int i = 0; i < sizeX + 1; i++)
        {
            for (int j = 0; j < sizeY + 1; j++)
            {
                filled[x + i, y + j] = true;
            }
        }

        return 0;
    }
    //static void generateFloorMesh(int bottomX, int topX, int bottomY, int topY)
    //{
    //    // bottom x,y = bottom left
    //    // top x, y = top right

    //    Vector3 bottomLeft = new Vector3(bottomX, 0, bottomY);
    //    Vector3 TopLeft = new Vector3(bottomX, 0, topY);
    //    Vector3 topRight = new Vector3(topX, 0, topY);
    //    Vector3 bottomRight = new Vector3(topX, 0, bottomY);

    //    // bottom left
    //    Vector3 tmp = Vertices.Find(tri => tri.x == bottomLeft.x);
    //    if (tmp == null)
    //    {
    //        Vertices.Add(new Vector3(bottomX, 0, bottomY));
    //        triangles.Add()
    //    }
    //    else
    //    {

    //    }


    //    // top left
    //    Vertices.Add(new Vector3(bottomX, 0, bottomY));
    //    // top right
    //    Vertices.Add(new Vector3(bottomX, 0, bottomY));
    //    // bottom right
    //    Vertices.Add(new Vector3(bottomX, 0, bottomY));
    //}
    //static void generateFloorMesh()
    //{
    //    bool[,] done = new bool[LevelSize + maxSize, LevelSize + maxSize];
    //    for (int x = 0; x < LevelSize + maxSize; x++)
    //    {
    //        for (int y = 0; y < LevelSize + maxSize; y++)
    //        {
    //            if (filled[x, y] == true && done[x, y] == false)
    //            {
    //                int travelerX = x;
    //                int travelerY = y;

    //                // current triangle number we're up to
    //                //int currentTriangle = 0;

    //                // triangles with less than 3 vertices
    //                //List<int> uncompletedTriangles = new List<int>();

    //                for (int i = 0; i < filled.Length; i++)
    //                {

    //                    bool above = filled[travelerX, travelerY + 1];
    //                    bool below = filled[travelerX, travelerY - 1];
    //                    bool left = filled[travelerX - 1, travelerY];
    //                    bool right = filled[travelerX + 1, travelerY];

    //                    // amount of consective turns we've done
    //                    int turns = 0;

    //                    if (left != true || done[x,y] == false)
    //                    {
    //                        // check y is not at max cord to prevent error with checking a position
    //                        // that doesn't exist
    //                        while (y != LevelSize + maxSize || above)
    //                        {
    //                            travelerY++;
    //                            above = filled[travelerx, travelery + 1];
    //                            below = filled[travelerX, travelerY - 1];
    //                            left = filled[travelerX - 1, travelerY];
    //                            right = filled[travelerX + 1, travelerY];

    //                            if (y != LevelSize + maxSize ||
    //                                (above == true && below == true &&
    //                                left == true && right == true))
    //                            {
    //                                //This means there is a square on the left
    //                                // so we put a vertice above to the top
    //                                // and a vertice to the right edge
    //                                // and we place a vertice at current position
    //                                // this makes it so the vertice positions are
    //                                // subdivided correctly, and the convex hull is a union
    //                                // of all the square

    //                                int offset = 0;
    //                                while (y != LevelSize + maxSize || above)
    //                                {
    //                                    offset++;
    //                                    above = filled[travelerX, travelerY + offset + 1];
    //                                }
    //                                Vertices.Add(new Vector3(x, 0, y + offset));
    //                                offset = 0;

    //                                while (x != LevelSize + maxSize || right)
    //                                {
    //                                    offset++;
    //                                    right = filled[travelerX + offset + 1, travelerY];
    //                                }
    //                                Vertices.Add(new Vector3(x + offset, 0, y));
    //                                break;
    //                            }
    //                            done[x, y] = true;
    //                        }

    //                        Vertices.Add(new Vector3(x, 0, y));
    //                        if (y != LevelSize + maxSize &&
    //                            above == true && below == true &&
    //                            left == true && right == true)
    //                        {
    //                            // offsets to the left so its on a tile with
    //                            // a unfilled bottom
    //                            x++;
    //                        }
    //                        else
    //                        {

    //                        }
    //                            done[x,y] = true;
    //                    }
    //                }

    //            }
    //        }
    //    }
    //}
    static Vector3[] getVertices(int x,int y)
    {
        int offsetX = 0;
        int offsetY = 0;

        Vector3 topLeft = new Vector3(x, 0, y);
        // go from top left to top right
        while (checkFilled(x + offsetX + 1, y, true))
        {
            offsetX++;
            int s = x + offsetX;
            Debug.Log("dool: " + s + " " + filled[x + offsetX, y]);
            // if there's a vertice already on this coordinate
            if (Vertices.Exists(tri => tri == new Vector3(x + offsetX, 0, y)))
            {
                Debug.Log("vertice on dool, proceed to break");
                break;
            }
        }
        int o = x + offsetX;
        Debug.Log("toprightOFFSET: " + offsetX);
        Debug.Log("topright: " + o);
        Vector3 topRight = new Vector3(x + offsetX, 0, y);
        if (offsetX == 0)
        {
            Debug.LogError("could not found square vertices\n" + topLeft + " and " + topRight);
            return new Vector3[4] { topLeft, topRight, Vector3.zero, Vector3.zero };
        }
        // go back and forth like mowing a lawn
        // to accurately find the bottom vertice positions
        for (int j = 0; j < maxSize + LevelSize; j++)
        {
            offsetY--;
            Debug.Log(offsetX + ", " + offsetY);
            Debug.Log(topLeft.x - topRight.x);
            for (int i = 0; i < topRight.x - topLeft.x; i++)
            {
                // if a vertice is already there
                if (Vertices.Exists(tri => tri == new Vector3(topRight.x, 0, y+offsetY)))
                {
                    Vector3 bottomRight = new Vector3(topRight.x, 0, y + offsetY);
                    Vector3 bottomLeft = new Vector3(topLeft.x, 0, y + offsetY);

                    Vector3[] squareVertices = new Vector3[4] { topLeft, topRight, bottomRight, bottomLeft };

                    unFillSquare(squareVertices);

                    return squareVertices;


                }
                // if left and bottom are filled and bottom left diagonial is unfilled
                if (checkFilled(x + offsetX - 1, y + offsetY, true) &&
                    checkFilled(x + offsetX, y + offsetY - 1, true) &&
                    checkFilled(x + offsetX - 1, y + offsetY - 1, false))
                {
                    Vector3 bottomRight = new Vector3(topRight.x, 0, y + offsetY);
                    Vector3 bottomLeft = new Vector3(topLeft.x, 0, y + offsetY);

                    Vector3[] squareVertices = new Vector3[4] { topLeft, topRight, bottomRight, bottomLeft };

                    unFillSquare(squareVertices);

                    return squareVertices;

                }
                // if right and bottom are filled and bottom right diagonal is unfilled
                if (checkFilled(x + offsetX + 1, y + offsetY, true) &&
                    checkFilled(x + offsetX, y + offsetY - 1, true) &&
                    checkFilled(x + offsetX + 1, y + offsetY -1, false))
                {
                    Vector3 bottomRight = new Vector3(topRight.x, 0, y + offsetY);
                    Vector3 bottomLeft = new Vector3(topLeft.x, 0, y + offsetY);

                    Vector3[] squareVertices = new Vector3[4] { topLeft, topRight, bottomRight, bottomLeft };

                    unFillSquare(squareVertices);

                    return squareVertices;

                }
                // if left and top is filled but top left diagonal is unfilled
                if (checkFilled(x + offsetX - 1, y + offsetY, true) &&
                    checkFilled(x + offsetX, y + offsetY + 1, true) &&
                    checkFilled(x + offsetX - 1, y + offsetY + 1, false))
                {
                    Vector3 bottomRight = new Vector3(topRight.x, 0, y + offsetY);
                    Vector3 bottomLeft = new Vector3(topLeft.x, 0, y + offsetY);

                    Vector3[] squareVertices = new Vector3[4] { topLeft, topRight, bottomRight, bottomLeft };

                    unFillSquare(squareVertices);

                    return squareVertices;

                }
                // if right and top is filled but top right diagonal is unfilled
                if (checkFilled(x + offsetX + 1, y + offsetY, true) &&
                    checkFilled(x + offsetX, y + offsetY + 1, true) &&
                    checkFilled(x + offsetX + 1, y + offsetY + 1, false))
                {
                    Vector3 bottomRight = new Vector3(topRight.x, 0, y + offsetY);
                    Vector3 bottomLeft = new Vector3(topLeft.x, 0, y + offsetY);

                    Vector3[] squareVertices = new Vector3[4] {topLeft, topRight, bottomRight, bottomLeft};

                    unFillSquare(squareVertices);

                    return squareVertices;
                }
                // if below isn't filled that means we're at the bottom
                // of the rectangle
                if (checkFilled(x + offsetX, y + offsetY - 1, false))
                {
                    Vector3 bottomRight = new Vector3(topRight.x, 0, y + offsetY);
                    Vector3 bottomLeft = new Vector3(topLeft.x, 0, y + offsetY);

                    Vector3[] squareVertices = new Vector3[4] { topLeft, topRight, bottomRight, bottomLeft };

                    unFillSquare(squareVertices);

                    return squareVertices;


                }
                offsetX--;
            }
            offsetY--;
            for (int i = 0; i < topRight.x - topLeft.x; i++)
            {
                // if a vertice is already there
                if (Vertices.Exists(tri => tri == new Vector3(x + offsetX, 0, y + offsetY)))
                {
                    Vector3 bottomRight = new Vector3(topRight.x, 0, y + offsetY);
                    Vector3 bottomLeft = new Vector3(topLeft.x, 0, y + offsetY);
                    
                    Vector3[] squareVertices = new Vector3[4] { topLeft, topRight, bottomRight, bottomLeft };

                    unFillSquare(squareVertices);

                    return squareVertices;


                }
                // if left and bottom are filled and bottom left diagonial is unfilled
                if (checkFilled(x + offsetX - 1, y + offsetY, true) &&
                    checkFilled(x + offsetX, y + offsetY - 1, true) &&
                    checkFilled(x + offsetX - 1, y + offsetY - 1, false))
                {
                    Vector3 bottomRight = new Vector3(topRight.x, 0, y + offsetY);
                    Vector3 bottomLeft = new Vector3(topLeft.x, 0, y + offsetY);

                    Vector3[] squareVertices = new Vector3[4] { topLeft, topRight, bottomRight, bottomLeft };

                    unFillSquare(squareVertices);

                    return squareVertices;

                }
                // if right and bottom are filled and bottom right diagonal is unfilled
                if (checkFilled(x + offsetX + 1, y + offsetY, true) &&
                    checkFilled(x + offsetX, y + offsetY - 1, true) &&
                    checkFilled(x + offsetX + 1, y + offsetY -1, false))
                {
                    Vector3 bottomRight = new Vector3(topRight.x, 0, y + offsetY);
                    Vector3 bottomLeft = new Vector3(topLeft.x, 0, y + offsetY);

                    Vector3[] squareVertices = new Vector3[4] { topLeft, topRight, bottomRight, bottomLeft };

                    unFillSquare(squareVertices);

                    return squareVertices;

                }
                // if left and top is filled but top left diagonal is unfilled
                if (checkFilled(x + offsetX - 1, y + offsetY, true) &&
                    checkFilled(x + offsetX, y + offsetY + 1, true) &&
                    checkFilled(x + offsetX - 1, y + offsetY + 1, false))
                {
                    Vector3 bottomRight = new Vector3(topRight.x, 0, y + offsetY);
                    Vector3 bottomLeft = new Vector3(topLeft.x, 0, y + offsetY);

                    Vector3[] squareVertices = new Vector3[4] { topLeft, topRight, bottomRight, bottomLeft };

                    unFillSquare(squareVertices);

                    return squareVertices;

                }
                // if right and top is filled but top right diagonal is unfilled
                if (checkFilled(x + offsetX + 1, y + offsetY, true) &&
                    checkFilled(x + offsetX, y + offsetY + 1, true) &&
                    checkFilled(x + offsetX + 1, y + offsetY + 1, false))
                {
                    Vector3 bottomRight = new Vector3(topRight.x, 0, y + offsetY);
                    Vector3 bottomLeft = new Vector3(topLeft.x, 0, y + offsetY);

                    Vector3[] squareVertices = new Vector3[4] {topLeft, topRight, bottomRight, bottomLeft};

                    unFillSquare(squareVertices);

                    return squareVertices;
                }
                // if below isn't filled that means we're at the bottom
                // of the rectangle
                if (checkFilled(x + offsetX, y + offsetY - 1, false) || y + offsetY == 0)
                {
                    Vector3 bottomRight = new Vector3(topRight.x, 0, y + offsetY);
                    Vector3 bottomLeft = new Vector3(topLeft.x, 0, y + offsetY);

                    Vector3[] squareVertices = new Vector3[4] { topLeft, topRight, bottomRight, bottomLeft };

                    unFillSquare(squareVertices);

                    return squareVertices;


                }
                offsetX++;
            }
        }
        Debug.LogError("could not found square vertices\n" + topLeft + " and " + topRight);
        return new Vector3[4] {topLeft, topRight, Vector3.zero, Vector3.zero};
    }
    static bool checkPointNeighbors(int x,int y, int[] dx, int[] dy, Vector3 topLeft, Vector3 bottomRight)
    {
        for (int i = 0; i < dx.Length; i++)
        {
            // check if the point is filled
            // and if the point is not out of bounds
            int ox = x + dx[i];
            int oy = y + dy[i];

            if (
                checkFilled(ox, oy, true) &&
                (ox > bottomRight.x || ox < topLeft.x ||
                oy > topLeft.z || oy < bottomRight.z)
                )
            {
                return true;
            }
        }
        return false;
    }
    static void unFillSquare(Vector3[] sqVertices)
    {
        // keep points that neighbor adjacent rectangles filled
        // unfill the rest

        Vector3 topLeft = sqVertices[0];
        Vector3 topRight = sqVertices[1];
        Vector3 bottomRight = sqVertices[2];
        Vector3 bottomLeft = sqVertices[3];

        List<int> xCords = new List<int>();
        List<int> yCords = new List<int>();

        int[] dx = { 0, -1, 1, 0 };
        int[] dy = { -1, 0, 0, 1 };


        //int[] dx = { -1,  0,  1, -1, 1, -1, 0, 1 };
        //int[] dy = { -1, -1, -1,  0, 0,  1, 1, 1 };

        for (int x = (int)topLeft.x; x <= (int)topRight.x; x++)
        {
            // Z axis is the where the 2d y coords are
            for (int y = (int)bottomRight.z; y <= (int)topLeft.z; y++)
            {
                if (checkPointNeighbors(x,y,dx,dy, topLeft, bottomRight) == false)
                {
                    xCords.Add(x);
                    yCords.Add(y);
                }
                // checking right side
                // if the point to the right is false
                //if (checkFilled(x + 1, y, false))
                //{
                //    xCords.Add(x);
                //    yCords.Add(y);
                //}
                //// checking left side
                //else if (checkFilled(x - 1, y, false))
                //{
                //    xCords.Add(x);
                //    yCords.Add(y);
                //}
                //// checking bottom
                //else if (checkFilled(x, y - 1, false))
                //{
                //    xCords.Add(x);
                //    yCords.Add(y);

                //}
                //// checking top
                //else if (checkFilled(x, y + 1, false))
                //{
                //    xCords.Add(x);
                //    yCords.Add(y);
                //}
            }
        }
        // do the unfilling after because if done during
        // the previously unfilled tiles mess up the following ones
        for (int i = 0; i < xCords.Count; i++)
        {
            filled[xCords[i], yCords[i]] = false;
            Debug.Log("unfilled: " +  xCords[i] + ", " + yCords[i]);
        }    
        
    }
    static bool checkFilled(int x, int y, bool condition)
    {
        // if x or y is out of bounds
        // this is to prevent out of bounds runtime error
        if (x >= maxSize + LevelSize || y >= maxSize + LevelSize)
        {
            return false;
        }
        if (x < 0 || y < 0)
        {
            return false;
        }
        return filled[x,y] == condition;
    }
    // BETTER IDEA
    static void generateFloorMesh()
    {
        for (int y = maxSize + LevelSize - 1; y >= 0; y--)
        {
            for (int x = 0; x < maxSize + LevelSize; x++)
            {
                Debug.Log("cord:" + x + ", " + y);
                if (filled[x, y] == true)
                {
                    Vector3[] squareVertices = getVertices(x, y);
                    Debug.Log("sq vert: " + squareVertices[0]);
                    Debug.Log("sq vert: " + squareVertices[1]);
                    Debug.Log("sq vert: " + squareVertices[2]);
                    Debug.Log("sq vert: " + squareVertices[3]);

                    Vertices.AddRange(squareVertices);
                    Vertices = Vertices.Distinct().ToList();

                    for (int i = 0; i < 4; i++)
                    {
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                        cube.transform.localPosition = new Vector3(squareVertices[i].x, 0.5f, squareVertices[i].z);

                        Material newMaterial = Resources.Load<Material>("SpatialMappingWireframe");

                        // Get the object's renderer
                        Renderer renderer = cube.GetComponent<Renderer>();
                        // Apply the material to the renderer
                        renderer.material = newMaterial;

                    }
                }
            }
        }
    }

    static void generateDebugGrid()
    {
        for (int i = 0; i < LevelSize + maxSize; i++)
        {
            for(int j = 0; j < LevelSize + maxSize; j++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                cube.transform.localPosition = new Vector3(i, 0, j);

                if (i % 5 == 0 || j % 5 ==0)
                {
                    cube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                }
                if (i % 10 == 0 || j % 10 == 0)
                {
                    cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                }

                if (filled[i, j] == true)
                {
                    Material newMaterial = Resources.Load<Material>("MushToon");

                    // Get the object's renderer
                    Renderer renderer = cube.GetComponent<Renderer>();
                    // Apply the material to the renderer
                    renderer.material = newMaterial;

                    cube.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
                }
            }
        }
    }

    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void OnBeforeSplashScreen()
    {
        for (int i = 0; i < Rooms; i++)
        {

            int cube = -1;
            int attempt = 0;
            while (cube == -1)
            {
                attempt++;
                if (attempt == 200)
                {
                    break;
                }
                cube = generateFloor(
                    Random.Range(0, LevelSize),
                    Random.Range(0, LevelSize),
                    Random.Range(minSize, maxSize),
                    Random.Range(minSize, maxSize)
                    
                );
            }
        }
        generateFloorMesh();
        generateDebugGrid();
    }

    // Update is called once per frame
    //void Update()
    //{

    //}
}
