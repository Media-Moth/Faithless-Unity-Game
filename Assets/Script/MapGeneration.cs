using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;
using DelaunatorSharp;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using IPoint = DelaunatorSharp.IPoint;
using UnityEditor.PackageManager;
using Unity.Mathematics;
using UnityEditor.Rendering;

public class NewMapGeneration : MonoBehaviour
{
    static int minSize = 2;
    static int maxSize = 18;
    static int rooms = 50;
    static int radius = 40;
    static int tileSize = 1;
    static int sizeThreshold = 3;
    static int borderThreshold = 3;
    static float mapHeight = 0f;
    static int minCorriderSize = 3;

    static List<Vector3> Vertices = new List<Vector3>();
    static List<int> triangles = new List<int>();
    static List<room> nodes = new List<room>();
    static List<room> mainNodes;
    static List<room> renderNodes = new List<room>();
    static List<roomPath> roomPaths = new List<roomPath>();
    static List<List<roomPath>> roomTrees = new List<List<roomPath>>();
    static int[,] tileMap;

    static List<wall> walls = new List<wall>();

    static Vector2 mapbottomLeft = Vector2.positiveInfinity; // positon of bottom left

    struct door
    {
        public float x;
        public float y;
        public int direction;
        public int width; // how many tiles wide the door s
    }
    struct wall
    {
        public float x;
        public float y;
        public int direction;
        public int width;
    }
    struct room
    {
        public Vector2 position;
        public int width;
        public int height;
    }
    struct roomPath
    { // its an edge :O
        public Vector2 p;
        public Vector2 q;
        public float weight;
    }
    static void generateTriangles(List<room> nodes, float offset)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            room currentRoom = nodes[i];
            // clockwise direction (or so i thought)
            int[] xdir = { 1, -1, -1, 1 };
            int[] ydir = { 1, 1, -1, -1 };
            for (int j = 0; j < 4; j++)
            {
                Vector3 vert = new Vector3(
                    currentRoom.position.x + (currentRoom.width - 1) / 2 * xdir[j],
                    offset,
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
                    Debug.DrawLine(Vertices[i * 4 + 2], Vertices[i * 4 + 1], Color.green, 100f);
                    Debug.DrawLine(Vertices[i * 4 + 1], Vertices[i * 4], Color.green, 100f);
                    Debug.DrawLine(Vertices[i * 4], vert, Color.green, 100f);
                }
            }
        }
    }
    static void generateRoom()
    {
        room newRoom = new room();
        Vector2 pos = generatePoint();
        newRoom.position = new Vector2(roundToTile(pos.x), roundToTile(pos.y));
        newRoom.width = roundToTile(Random.Range(minSize, maxSize) * 2 + 1); // make it odd number
        newRoom.height = roundToTile(Random.Range(minSize, maxSize) * 2 + 1);
        nodes.Add(newRoom);
    }
    // segments AB to segments CD
    static bool doSegmentsIntersect(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
    {
        // lowkey the only part of the code where copilot was actually helpful
        // Check if segments AB and CD intersect

        // what ai didn't do is account for float error
        const float epsilon = 1e-6f;

        float denominator = (B.x - A.x) * (D.y - C.y) - (B.y - A.y) * (D.x - C.x);
        if (math.abs(denominator) < epsilon)
        {
            return false; // Segments are parallel
        }
        float ua = ((D.x - C.x) * (A.y - C.y) - (D.y - C.y) * (A.x - C.x)) / denominator;
        float ub = ((B.x - A.x) * (A.y - C.y) - (B.y - A.y) * (A.x - C.x)) / denominator;

        //return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;

        return ua >= -epsilon && ua <= 1 + epsilon &&
        ub >= -epsilon && ub <= 1 + epsilon;
    }
    static bool isRoomInsidePath(room r, roomPath path)
    {
               // check right edge
        return doSegmentsIntersect(r.position + new Vector2(r.width / 2, r.height / 2), r.position + new Vector2(r.width / 2, -r.height / 2), path.p, path.q) ||
               // check left edge
               doSegmentsIntersect(r.position + new Vector2(-r.width / 2, r.height / 2), r.position + new Vector2(-r.width / 2, -r.height / 2), path.p, path.q) ||
               // check top edge
               doSegmentsIntersect(r.position + new Vector2(-r.width / 2, r.height / 2), r.position + new Vector2(r.width / 2, r.height / 2), path.p, path.q) ||
               // check bottom edge
               doSegmentsIntersect(r.position + new Vector2(-r.width / 2, -r.height / 2), r.position + new Vector2(r.width / 2, -r.height / 2), path.p, path.q);
    }
    static bool isRoomColliding(room a, room b)
    { // detects overlaps
        bool yCollide = a.position.y - (a.height - 1) / 2 < b.position.y + (b.height - 1) / 2 && b.position.y - (b.height - 1) / 2 < a.position.y + (a.height - 1) / 2;
        bool xCollide = a.position.x - (a.width - 1) / 2 < b.position.x + (b.width - 1) / 2 && b.position.x - (b.width - 1) / 2 < a.position.x + (a.width - 1) / 2;
        return xCollide && yCollide;
    }
    static bool isRoomBordering(room a, room b)
    { // literally isRoomColliding but each room is expanded by the borderthreshold in each direction
        bool yCollide = a.position.y - (a.height - 1) / 2 - borderThreshold < b.position.y + (b.height - 1) / 2 + borderThreshold && b.position.y - (b.height - 1) / 2 - borderThreshold < a.position.y + (a.height - 1) / 2 + borderThreshold;
        bool xCollide = a.position.x - (a.width - 1) / 2 - borderThreshold < b.position.x + (b.width - 1) / 2 + borderThreshold && b.position.x - (b.width - 1) / 2 - borderThreshold < a.position.x + (a.width - 1) / 2 + borderThreshold;
        return xCollide && yCollide;
    }

    static bool moveRooms()
    {
        bool noOverlaps = true;
        for (int i = 0; i < nodes.Count; i++)
        {
            Vector2 velocity = new Vector2(0, 0);
            room currentRoom = nodes[i];
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue;
                room otherRoom = nodes[j];
                if (isRoomColliding(currentRoom, otherRoom))
                {
                    noOverlaps = false;
                    Vector2 dir = new Vector2(
                        currentRoom.position.x - otherRoom.position.x,
                        currentRoom.position.y - otherRoom.position.y
                    );
                    dir = dir.normalized * tileSize;
                    if (dir.x == 0 && dir.y == 0)
                    {
                        Vector2 rand = Random.value > 0.5 ? new Vector2(1, 0) : new Vector2(0, 1);
                        dir = rand;
                    }
                    velocity += dir;
                }
            }
            currentRoom.position += velocity;
            currentRoom.position = new Vector2(
                roundToTile(currentRoom.position.x),
                roundToTile(currentRoom.position.y)
            );
            nodes[i] = currentRoom;
        }
        return noOverlaps;
    }
    static void removeSmallRooms()
    {
        sizeThreshold = sizeThreshold * tileSize + 1;
        for (int i = 0; i < mainNodes.Count; i++)
        {
            room currentRoom = mainNodes[i];
            if (currentRoom.width < sizeThreshold || currentRoom.height < sizeThreshold)
            {
                mainNodes.RemoveAt(i);
                i--;
            }
        }
    }
    static void removeBorderingRooms()
    {
        for (int i = 0; i < mainNodes.Count; i++)
        {
            room currentRoom = mainNodes[i];
            bool isBordering = false;
            int j = 0; // to remove the other room if it is smaller
            for (j = 0; j < mainNodes.Count; j++)
            {
                if (i == j) continue;
                room otherRoom = mainNodes[j];
                if (isRoomBordering(currentRoom, otherRoom))
                {
                    isBordering = true;
                    // remove smaller room
                    if (currentRoom.width < otherRoom.width || currentRoom.height < otherRoom.height)
                    {
                        j = i;
                    }
                    break;
                }
            }
            if (isBordering)
            {
                mainNodes.RemoveAt(j);
                if (j < i) i--; // if j is behind then it should move i back to  
            }
        }
    }
    static void seperateBigRoomsFromSmallRoomList()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            foreach (room mainRoom in mainNodes)
            {
                if (nodes[i].position == mainRoom.position)
                {
                    renderNodes.Add(nodes[i]);
                    nodes.RemoveAt(i);
                    i--;
                    break;

                }

            }
        }
    }
    static void generateDelaunayTrig()
    {
        IPoint[] points = new IPoint[mainNodes.Count];
        for (int i = 0; i < mainNodes.Count; i++)
        {
            Vector2 position = mainNodes[i].position;
            points[i] = new Point(position.x, position.y);
        }
        Delaunator dn = new Delaunator(points.ToArray());

        foreach (IEdge edge in dn.GetEdges())
        { // loop through edges in delaunay triangle
            roomPath path = new roomPath();
            IPoint p = edge.P;
            IPoint q = edge.Q;

            path.p = new Vector2((float)p.X, (float)p.Y);
            path.q = new Vector2((float)q.X, (float)q.Y);

            //Debug.DrawLine(new Vector3(path.p.x, 0, path.p.y), new Vector3(path.q.x, 0, path.q.y), Color.red, 100f);

            path.weight = Vector2.Distance(path.p, path.q);
            roomPaths.Add(path);
        }
    }
    static int findTreeIndex(Vector2 position)
    {
        for (int i = 0; i < roomTrees.Count; i++)
        {
            for (int j = 0; j < roomTrees[i].Count; j++)
            {
                roomPath path = roomTrees[i][j];
                if (path.p == position || path.q == position)
                {
                    return i;
                }
            }
        }
        return -1; // not found
    }
    static void generateMinimumSpanningTree()
    {
        foreach (roomPath path in roomPaths.OrderBy(roomP => roomP.weight)) // smallest first
        {
            int pIndex = findTreeIndex(path.p);
            int qIndex = findTreeIndex(path.q);

            if (pIndex == -1 && qIndex == -1)
            { // both points not in any trees so create a new one
                roomTrees.Add(new List<roomPath> { path });
            }
            else if (pIndex != -1 && qIndex == -1)
            { // path.p is in a tree but path.q is not
                roomTrees[pIndex].Add(path);
            }
            else if (pIndex == -1 && qIndex != -1)
            { // path.q is in a tree but path.p is not
                roomTrees[qIndex].Add(path);
            }
            else if (pIndex != qIndex)
            { // both points are in different trees
                roomTrees[pIndex].AddRange(roomTrees[qIndex]); // merge trees into pIndex one
                roomTrees[pIndex].Add(path);
                roomTrees.RemoveAt(qIndex);
            } // if both points are in the same tree ignore them to prevent branch loops
        }
        foreach (roomPath branch in roomTrees[0])
        {
            Debug.DrawLine(new Vector3(branch.p.x, 2, branch.p.y), new Vector3(branch.q.x, 2, branch.q.y), Color.blue, 100f);
        }
    }
    static void generateFloor(Vector2 mapSize)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.localScale = new Vector3(mapSize.x, 1, mapSize.y);
        floor.transform.position = new Vector3(mapbottomLeft.x + mapSize.x / 2, mapHeight, mapbottomLeft.y + mapSize.y / 2);
    }
    static void generateTileGrid()
    {
        foreach (roomPath path in roomTrees[0])
        {
            foreach (room smallRoom in nodes)
            {
                if (isRoomInsidePath(smallRoom, path))
                {
                    renderNodes.Add(smallRoom);
                }
            }
        }
        Vector2 mapSize = Vector2.negativeInfinity;

        foreach (room renderRoom in renderNodes)
        {
            //find bottom left
            mapbottomLeft.x = math.min(renderRoom.position.x - renderRoom.width / 2, mapbottomLeft.x);
            mapbottomLeft.y = math.min(renderRoom.position.y - renderRoom.height / 2, mapbottomLeft.y);
            //find top right
            mapSize.x = math.max(renderRoom.position.x + renderRoom.width / 2, mapSize.x);
            mapSize.y = math.max(renderRoom.position.y + renderRoom.height / 2, mapSize.y);
        }
        mapSize -= mapbottomLeft;
        mapSize.x = Mathf.CeilToInt(mapSize.x) + 10; // offset by five (double that for size) cuz idk stuff gets cut of
        mapSize.y = Mathf.CeilToInt(mapSize.y) + 10;

        mapbottomLeft -= new Vector2(5, 5);

        tileMap = new int[(int)mapSize.x, (int)mapSize.y];
        Debug.Log("map Size: " + mapSize);
        Debug.Log("map bottom left: " + mapbottomLeft);

        foreach (room renderRoom in renderNodes)
        {
            // 1: main room floor
            // 2: corridor floor
            int tileType = sizeThreshold < renderRoom.width && sizeThreshold < renderRoom.height ? 1 : 2;

            int posX = (int)(renderRoom.position.x - renderRoom.width / 2 - mapbottomLeft.x);
            // +20 -33
            int posY = (int)(renderRoom.position.y - renderRoom.height / 2 - mapbottomLeft.y);
            int width = renderRoom.width;
            int height = renderRoom.height;

            for (int x = 0; x < width; x++)
            {
                if (posX + x >= (int)mapSize.x-1) break;
                if (posX + x < 0) continue;
                for (int y = 0; y < height; y++)
                {
                    if (posY + y >= (int)mapSize.y-1) break;
                    if (posY + y < 0) continue;
                    int roomX = posX + x;
                    int roomY = posY + y;
                    Debug.Log("roomX: " + roomX + " roomY: " + roomY + " tileType: " + tileType);
                    if (tileMap[roomX, roomY] == 1) continue;
                    tileMap[roomX, roomY] = tileType;
                }
            }

        }
        generateFloor(mapSize);
        // 0: nothing
        // 1: main room floor
        // 2: corridor floor
        // border between nothing and floor wil be a wall
        // border between main room and corrider will be a door
    }
    static room findMainRoomFromPosition(Vector2 position)
    {
        foreach (room r in mainNodes)
        {
            if (r.position == position) // it should be exact
            {
                return r;
            }
        }
        Debug.LogWarning("ts pmo dawg no crib");
        return new room(); // empty room
    }
    static Vector2 worldToTilePosition(Vector2 worldpos)
    {
        worldpos -= mapbottomLeft; // move to 0,0
        return worldpos;
    }
    static void generateCorridors()
    {
        // after generate tile grid
        foreach (roomPath path in roomTrees[0])
        {
            room p = findMainRoomFromPosition(path.p);
            room q = findMainRoomFromPosition(path.q);

            Vector2 corridorPosition = new Vector2(math.min(p.position.x, q.position.x), math.min(p.position.y, q.position.y));
            Vector2 corriderSize = new Vector2(math.max(p.position.x, q.position.x) - corridorPosition.x, math.max(p.position.y, q.position.y) - corridorPosition.y);

            corriderSize.x = math.max(corriderSize.x, minCorriderSize);
            corriderSize.y = math.max(corriderSize.y, minCorriderSize);

            bool isHorizontal = corriderSize.x > corriderSize.y;
            if (isHorizontal)
            {   // horizontal corridor
                // clamp horizontally
                float pR = p.position.x + p.width / 2;
                float qR = q.position.x + q.width / 2;
                float pL = p.position.x - p.width / 2;
                float qL = q.position.x - q.width / 2;

                math.clamp(corridorPosition.x, Mathf.Min(pR, qR), Mathf.Max(pR, qR));
                math.clamp(corridorPosition.x, Mathf.Min(pL, qL), Mathf.Max(pL, qL));

                math.clamp(corriderSize.x, Mathf.Min(pR, qR) - corridorPosition.x, Mathf.Max(pR, qR) - corridorPosition.x);
                math.clamp(corriderSize.x, Mathf.Min(pL, qL) - corridorPosition.x, Mathf.Max(pL, qL) - corridorPosition.x);
            }
            else
            {   // vertical corridor
                // clamp vertically
                float pT = p.position.y + p.height / 2;
                float qT = q.position.y + q.height / 2;
                float pB = p.position.y - p.height / 2;
                float qB = q.position.y - q.height / 2;

                math.clamp(corridorPosition.y, Mathf.Min(pT, qT), Mathf.Max(pT, qT));
                math.clamp(corridorPosition.y, Mathf.Min(pB, qB), Mathf.Max(pB, qB));

                math.clamp(corriderSize.y, Mathf.Min(pT, qT) - corridorPosition.y, Mathf.Max(pT, qT) - corridorPosition.y);
                math.clamp(corriderSize.y, Mathf.Min(pB, qB) - corridorPosition.y, Mathf.Max(pB, qB) - corridorPosition.y);
            }
            //Debug.DrawLine(new Vector3(corriderSize.x + corridorPosition.x, 0, corriderSize.y + corridorPosition.y), new Vector3(corridorPosition.x, 0, corridorPosition.y), Color.green, 100f);

            // draw corrider to tile map

            corridorPosition -= mapbottomLeft;
            corriderSize += corridorPosition;

            for (int x = (int)corridorPosition.x; x < corriderSize.x; x++)
            {
                for (int y = (int)corridorPosition.y; y < corriderSize.y; y++)
                {
                    if (x < 0 || y < 0 || x >= tileMap.GetLength(0) || y >= tileMap.GetLength(1)) continue; // out of bounds
                    if (tileMap[x, y] == 1) continue;
                    tileMap[x, y] = 2;
                }
            }

        }
    }
    static void generateWallList()
    {
        // imagine each tile as a square
        // the walls will be aligned to one of those squares
        // so the wall is able to face 4 different directions
        // the integers on this map determine that direction
        // so just generate it as a list

        // the walls face inward towards the tile center so you don't have to check empty tiles

        // we need to loop through the tilemap on the y axis and strech out any vertical walls then place them
        // then do the same on the x axis

        // loop through y axis
        for (int x = 0; x < tileMap.GetLength(0); x++)
        {
            for (int y = 0; y < tileMap.GetLength(1); y++)
            {
                int tileType = tileMap[x, y];

                if (tileType == 0) continue; // empty tile

                int[] xDir = { 1, -1};

                // 0 = right
                // 1 = left

                for (int i = 0; i < xDir.Length; i++)
                {
                    int neighbourX = x + xDir[i];
                    if (neighbourX < 0 || y < 0 || neighbourX >= tileMap.GetLength(0) || y >= tileMap.GetLength(1))
                    {
                        continue; // out of bounds
                    }
                    if (tileMap[neighbourX, y] == 0)
                    {
                        int size = 1;
                        while (y < tileMap.GetLength(1) && tileMap[neighbourX, y] == 0 && tileMap[x,y] != 0)
                        {
                            size++;
                            y++;
                        }
                        y--;
                        size--;

                        wall newWall = new wall();
                        newWall.x = x;
                        newWall.y = y- ((float)size-1)/2;
                        newWall.width = size;
                        newWall.direction = i;
                        walls.Add(newWall);
                    }
                }
            }
        }
        // 1, 0
        // 2, 0.5
        // 3, 1
        // | -
        // | - \
        // | - /
        // | -
        for (int y = 0; y < tileMap.GetLength(1); y++)
        {
            for (int x = 0; x < tileMap.GetLength(0); x++)
            {
                int tileType = tileMap[x, y];

                if (tileType == 0) continue; // empty tile

                int[] yDir = { 1, -1};

                // 0 = right
                // 1 = left

                for (int i = 0; i < yDir.Length; i++)
                {
                    int neighbourY = y + yDir[i];
                    if (x < 0 || neighbourY < 0 || x >= tileMap.GetLength(0) || neighbourY >= tileMap.GetLength(1))
                    {
                        continue; // out of bounds
                    }
                    if (tileMap[x, neighbourY] == 0)
                    {
                        int size = 1;
                        while (x < tileMap.GetLength(0) && tileMap[x, neighbourY] == 0 && tileMap[x,y] != 0)
                        {
                            size++;
                            x++;
                        }
                        x--;
                        size--;

                        wall newWall = new wall();
                        newWall.x = x-((float)size - 1) /2;
                        newWall.y = y;
                        newWall.width = size;
                        newWall.direction = i+2;
                        walls.Add(newWall);
                    }
                }
            }
        }

        // loop through x axis
        Debug.Log(walls.Count + " walls have been added");
        Debug.Log(walls.Count * 12 + " triangles are generated by teh walls");
    }
    static List<Vector2> getRoomPaths(room currentRoom)
    {
        // there's something wrong with this fix it now
        List<Vector2> paths = new List<Vector2>();
        foreach (roomPath path in roomTrees[0])
        {
            if (isRoomInsidePath(currentRoom, path))
            {
                paths.Add(path.p);
                paths.Add(path.q);
            }
        }
        return paths;
    }
    static bool doesSegmentIntersectWithRoomsPaths(Vector2 A, Vector2 B, List<Vector2> paths)
    { // every even (at 0 base index) indice of paths is A of a new path
        for (int i = 0; i < paths.Count; i += 2)
        {
            Vector2 C = paths[i];
            Vector2 D = paths[i + 1];
            if (doSegmentsIntersect(A, B, C, D))
            {
                paths.RemoveAt(i + 1);
                paths.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    static bool isOutOfBounds(int x, int y)
    {
        return x < 0 || y < 0 || x >= tileMap.GetLength(0) || y >= tileMap.GetLength(1);
    }
    static void generateDoors()
    {
        // new idea
        // count how many edges the room has
        // find the closest tile on covex hull to edge
        // place door weighted away from corners if possibles
        // loop through edge count

        foreach (room mainroom in mainNodes)
        {
            int tileCentreX = (int)(mainroom.position.x - mapbottomLeft.x);
            int tileCentreY = (int)(mainroom.position.y - mapbottomLeft.y);

            Debug.DrawLine(new Vector3(tileCentreX, 0, tileCentreY) + new Vector3(mapbottomLeft.x, 5f, mapbottomLeft.y), new Vector3(tileCentreX, 0, tileCentreY + 1) + new Vector3(mapbottomLeft.x, 5f, mapbottomLeft.y), Color.black, 100f);

            int probeX = tileCentreX + mainroom.width / 2;
            int probeY = tileCentreY + mainroom.height / 2;

            //while (tileMap[probeX, probeY] == 1) // find a tile that is not a main room floor
            //{
            //    probeX++;
            //    if (probeX >= tileMap.GetLength(0))
            //    {
            //        break;
            //    }
            //}
            //probeX--;
            //while (tileMap[probeX, probeY] == 1)
            //{
            //    probeY++;
            //    if (probeY >= tileMap.GetLength(1))
            //    {
            //        break;
            //    }
            //}
            //probeY -= 2; // minus two so we don't check top right tile twice when we loop convex hull
            //probeY--;

            Debug.DrawLine(new Vector3(probeX, 0, probeY) + new Vector3(mapbottomLeft.x, 3f, mapbottomLeft.y), new Vector3(probeX, 0, probeY + 1) + new Vector3(mapbottomLeft.x, 3f, mapbottomLeft.y), Color.green, 100f);

            // anticlockwise starting from top right (-1 on y position) side
            int[] xdir = { 0, -1, 0, 1 };
            int[] ydir = { -1, 0, 1, 0 };
            int[] max = {mainroom.height-1, mainroom.width-1, mainroom.height-1, mainroom.width-1};
            int doorsPlaced = 0;
            List<Vector2> paths = getRoomPaths(mainroom);

            for (int i = 0; i < xdir.Length; i++)
            {
                for (int j = 0; j < max[i]; j++)
                {
                    probeX += xdir[i];
                    probeY += ydir[i];
                    int neighborX = probeX - ydir[i]; // penpendicular neighbor on the outside
                    int neighborY = probeY + xdir[i];

                    if (isOutOfBounds(probeX, probeY) || isOutOfBounds(neighborX, neighborY)) continue;

                    Vector2 A = new Vector2(probeX, probeY);
                    Vector2 B = new Vector2(probeX - xdir[i], probeY - ydir[i]);

                    A += mapbottomLeft;
                    B += mapbottomLeft;

                    if (doesSegmentIntersectWithRoomsPaths(A,B, paths)) // tileMap[neighborX, neighborY] == 2 &&
                    {
                        Debug.DrawLine(new Vector3(A.x, 5.5f, A.y), new Vector3(B.x, 5.5f, B.y), Color.yellow, 100f);
                    }
                    else
                    {
                        if (tileMap[neighborX, neighborY] != 0)
                        {
                            wall newWall = new wall();
                            newWall.x = probeX;
                            newWall.y = probeY;
                            newWall.width = 1;
                            int dir = i;
                            if (dir == 1) dir = 3;
                            if (dir == 2) dir = 1;
                            if (dir == 3) dir = 2;
                            if (dir == 4) dir = 4;
                            newWall.direction = dir;
                            walls.Add(newWall);
                        }
                        
                    }

                    Debug.DrawLine(new Vector3(probeX, 0, probeY) + new Vector3(mapbottomLeft.x, 5f, mapbottomLeft.y), new Vector3(probeX - xdir[i], 0, probeY - ydir[i]) + new Vector3(mapbottomLeft.x, 5f, mapbottomLeft.y), Color.red, 100f);
                }
            }
        }
    }
    static void generateWalls()
    {
        GameObject wall = Resources.Load<GameObject>("Wall");
        
        for (int i = 0; i < walls.Count; i++)
        {
            wall currentWall = walls[i];
            float rotation = 0;

            switch (currentWall.direction)
            {
                case 0: // radians cuz its autistic
                    rotation = math.PIHALF; // right
                    break;
                case 1:
                    rotation = -math.PIHALF; // left
                    break;
                case 2:
                    rotation = 0; // up
                    break;
                case 3:
                    rotation = -Mathf.PI; // down
                    break;
            }

            GameObject wallClone = Instantiate(wall, new Vector3(currentWall.x * tileSize, 0.5f, currentWall.y * tileSize) + new Vector3(mapbottomLeft.x, mapHeight, mapbottomLeft.y), quaternion.EulerXYZ(0f,rotation,0f));
            wallClone.transform.localScale = new Vector3(currentWall.width, 1f, 1f);
        }
        
    }
    static int roundToTile(float value)
    {
        return Mathf.RoundToInt(value / tileSize) * tileSize;
    }
    static Vector2 generatePoint()
    {
        float t = 2f * (float)Math.PI * Random.value;
        float r = radius * (float)Math.Sqrt(Random.value);
        return new Vector2(r * Mathf.Cos(t), r * Mathf.Sin(t));
    }
    static void generateMesh(string material)
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
        Material newMaterial = Resources.Load<Material>(material);

        Renderer renderer = meshObject.GetComponent<Renderer>();
               
        renderer.material = newMaterial;

        MeshCollider collider = meshObject.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnBeforeSplashScreen()
    { // main script, runs when the game starts
        for (int i = 0; i < rooms; i++)
        {
            generateRoom();
        }
        //while (!moveRooms());
        int maxIterations = 3500;
        int iteration = 0;
        while (!moveRooms() && iteration < maxIterations)
        {
            iteration++;
        }
        if (iteration == maxIterations)
        {
            Debug.LogWarning("max iterations");
        }
        mainNodes = new List<room>(nodes);
        removeSmallRooms();
        removeBorderingRooms();
        seperateBigRoomsFromSmallRoomList();
        generateDelaunayTrig();
        generateMinimumSpanningTree();
        generateTileGrid();
        generateCorridors();
        


        generateWallList();
        generateDoors();
        generateWalls();

        triangles = new List<int>();
        Vertices = new List<Vector3>();
        //TrianglizeTileGrid();
        //Debug.Log("Vertices: " + Vertices.Count + " Triangles: " + triangles.Count);


        //generateTriangles(renderNodes, 2f);
        //generateMesh("MushToon");


        //triangles = new List<int>();
        //Vertices = new List<Vector3>();

        //generateTriangles(mainNodes, 0f);
        //generateMesh("MushToon");

        //triangles = new List<int>();
        //Vertices = new List<Vector3>();

        //generateTriangles(nodes, -1f);
        //generateMesh("SpatialMappingWireframe");
    }

}