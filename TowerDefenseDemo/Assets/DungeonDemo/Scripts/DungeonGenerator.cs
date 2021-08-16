using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    private DungeonManager dungeonManager;

    public int maxRooms;
    public int width, height;
    public int minRoomWidth, minRoomHeight;
    public int maxRoomWidth, maxRoomHeight;
    public int _roomCount;

    public int wallID;
    public int basicID;
    public int fillID;

    public bool basicOnly;
    public bool makeCorridorsRooms;
    // only used if makeCorridorsRooms is true
    public int corridorID;

    public List<int> validIDs;

    public int[,] map;

    public struct Room
    {
        public int x, y, w, h;
        public int x2, y2;
        public Vector2Int center;

        public Room(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;

            x2 = x + w;
            y2 = y + h;

            center = new Vector2Int((x + x2) / 2,
                (y + y2) / 2);
        }

        // return true if this room intersects provided room
        public bool intersects(Room room)
        {
            return (x <= room.x2 && x2 >= room.x &&
                y <= room.y2 && room.y2 >= room.y);
        }
    }
    public List<Room> rooms;

    public DungeonGenerator(int maxRooms, int width, int height, int minRoomWidth, int maxRoomWidth, int minRoomHeight, int maxRoomHeight)
    {
        this.maxRooms = maxRooms;
        this.width = width;
        this.height = height;
        this.minRoomHeight = minRoomHeight;
        this.maxRoomHeight = maxRoomHeight;
        this.minRoomWidth = minRoomWidth;
        this.maxRoomWidth = maxRoomWidth;
    }

    public void populateRooms(bool applyWalls = true)
    {
        rooms = new List<Room>();
        _roomCount = 0;
        map = new int[width, height];

        // variable for tracking center of each room
        Vector2Int newCenter = Vector2Int.zero;

        for (int i = 0; i < maxRooms; i++)
        {
            int w = Random.Range(minRoomWidth, maxRoomWidth + 1);
            int h = Random.Range(minRoomHeight, maxRoomHeight + 1);
            int x = Random.Range(1, width - w);
            int y = Random.Range(1, height - h);

            // create room with randomized values
            Room newRoom = new Room(x, y, w, h);

            bool failed = false;
            foreach (Room otherRoom in rooms)
            {
                if (newRoom.intersects(otherRoom))
                {
                    failed = true;
                    break;
                }
            }
            if (!failed)
            {
                // local function to carve out new room
                _roomCount++;
                createRoom(newRoom, _roomCount);

                // store center for new room
                newCenter = newRoom.center;
                int offsetW = newRoom.w / 2;
                int offsetH = newRoom.h / 2;
                newCenter.x = Random.Range(newCenter.x - offsetW, newCenter.x + offsetW);
                newCenter.y = Random.Range(newCenter.y - offsetH, newCenter.y + offsetH);

                if (rooms.Count != 0)
                {
                    // store center of previous room
                    Vector2Int prevCenter = rooms[rooms.Count - 1].center;
                    offsetW = rooms[rooms.Count - 1].w / 2;
                    offsetH = rooms[rooms.Count - 1].h / 2;
                    prevCenter.x = Random.Range(prevCenter.x - offsetW, prevCenter.x + offsetW);
                    prevCenter.y = Random.Range(prevCenter.y - offsetH, prevCenter.y + offsetH);

                    // carve out corridors between rooms based on centers
                    // randomly start with horizontal or vertical corridors
                    if (Random.Range(0, 2) == 1)
                    {
                        hCorridor(prevCenter.x, newCenter.x, prevCenter.y);
                        vCorridor(prevCenter.y, newCenter.y, newCenter.x);
                    }
                    else
                    {
                        vCorridor(prevCenter.y, newCenter.y, prevCenter.x);
                        hCorridor(prevCenter.x, newCenter.x, newCenter.y);
                    }
                }

                // push new room into rooms array
                rooms.Add(newRoom);
            }
        }
        if(applyWalls)
        {
            this.applyWalls();
        }
    }

    public void createDefaultMap()
    {
        map = new int[width, height];
        createRoom(new Room(0, 0, width, height), 0);
    }

    public void createRoom(Room room, int roomID)
    {
        for (int x = room.x; x < room.x2; x++)
        {
            for (int y = room.y; y < room.y2; y++)
            {
                map[x, y] = roomID;
            }
        }
    }

    public void printRooms()
    {
        string result = "";
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                result += map[x, y];
            }
            result += "\n";
        }
        Debug.Log(result);
    }

    private void hCorridor(int x1, int x2, int y)
    {
        int startX = Mathf.Min(x1, x2);
        int endX = Mathf.Max(x1, x2);

        bool firstEdit = true;

        for (int x = startX; x <= endX; x++)
        {
            if (map[x, y] == 0)
            {
                if (firstEdit)
                {
                    if (makeCorridorsRooms)
                    {
                        _roomCount++;
                    }
                    firstEdit = false;
                }
                map[x, y] = makeCorridorsRooms ? _roomCount : -2;
            }
        }
    }

    private void vCorridor(int y1, int y2, int x)
    {
        int startY = Mathf.Min(y1, y2);
        int endY = Mathf.Max(y1, y2);

        bool firstEdit = true;

        for (int y = startY; y <= endY; y++)
        {
            if (map[x, y] == 0)
            {
                if (firstEdit)
                {
                    if (makeCorridorsRooms)
                    { 
                        _roomCount++;
                    }
                    firstEdit = false;
                }
                map[x, y] = makeCorridorsRooms ? _roomCount : -2;
            }
        }
    }

    private void applyWalls()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (testWallRequired(x, y))
                {
                    map[x, y] = -1;
                }
            }
        }
    }

    private bool testWallRequired(int x, int y)
    {
        int v = map[x, y];
        if (v != 0) return false;
        for (int ix = x - 1; ix <= x + 1; ix++)
        {
            for (int iy = y - 1; iy <= y + 1; iy++)
            {
                if (map[ix, iy] != 0 && map[ix, iy] != -1)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void spawnMap()
    {
        if(dungeonManager == null)
        {
            dungeonManager = DungeonManager.Instance;
        }

        dungeonManager.createBaseMap(width, height, fillID, wallID);

        Dictionary<int, int> roomTilePairs = new Dictionary<int, int>();

        // Wall
        roomTilePairs.Add(-1, wallID);
        // Corridor Override
        if (!makeCorridorsRooms)
        {
            roomTilePairs.Add(-2, corridorID);
        }
        // Set random tile types for each room
        for (int i = 1; i <= _roomCount; i++)
        {
            roomTilePairs.Add(i, basicOnly ? basicID : validIDs[Random.Range(0, validIDs.Count)]);
        }

        // Populate blocks with assigned 
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] != 0)
                {
                    dungeonManager.createTile(roomTilePairs[map[x, y]], new Vector2Int(x, y));
                }
            }
        }
    }
}
