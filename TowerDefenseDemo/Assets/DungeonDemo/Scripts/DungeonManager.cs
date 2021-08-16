using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : Singleton<DungeonManager>
{
    public List<Sprite> tileTextures;
    public List<Sprite> objectTextures;

    private GameObject[,] map;
    public int mapWidth, mapHeight;
    public List<GameObject> objects;

    public GameObject tilePrefab;
    public GameObject objectPrefab;
    public float cellDim = 2;

    private bool firstUpdate = true;

    private Transform _tileParent;
    private Transform _objectParent;

    public bool spawnObjects = true;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        map = new GameObject[mapWidth, mapHeight];

        _tileParent = transform.GetChild(0);
        _objectParent = transform.GetChild(1);
        objects = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (firstUpdate)
        {
            applyPGC();
            firstUpdate = false;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            applyPGC();
        }
        else if(Input.GetKeyDown(KeyCode.O))
        {
            destroyAllObjects();

            DungeonGenerator mapGen = GetComponent<DungeonGenerator>();

            DungeonObjectGenerator objGen = GetComponent<DungeonObjectGenerator>();
            objGen.generatePoisson(mapWidth, mapHeight);
            objGen.printGrid();
            objGen.filterGrid(mapGen.map);
            objGen.printFilteredGrid();
            objGen.spawnObjects();
        }
    }

    private void applyPGC()
    {
        DungeonGenerator mapGen = GetComponent<DungeonGenerator>();
        //print("Populating Rooms");
        mapGen.populateRooms();
        //print("Spawning Map");
        mapGen.spawnMap();
        //print("Map Spawned");
        mapGen.printRooms();

        if (spawnObjects)
        {
            DungeonObjectGenerator objGen = GetComponent<DungeonObjectGenerator>();
            objGen.generatePoisson(mapWidth, mapHeight);
            objGen.printGrid();
            objGen.filterGrid(mapGen.map);
            objGen.printFilteredGrid();
            objGen.spawnObjects();
        }

        print(mapGen.rooms[0].center);
        GameObject.Find("Player").transform.position = new Vector3(mapGen.rooms[0].center.x * cellDim,
                                                                   mapGen.rooms[0].center.y * cellDim, 0);
        Camera.main.GetComponent<FollowCameraBehaviour>().setMap(new Vector2((mapWidth) * cellDim, (mapHeight) * cellDim), new Vector2(cellDim / 2, cellDim / 2));
    }

    public void setMapSize(int mapWidth, int mapHeight)
    {
        destroyAllObjects();
        if (this.mapWidth == mapWidth && this.mapHeight == mapHeight)
            return;
        deleteMap();
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        map = new GameObject[mapWidth, mapHeight];
    }

    public void createBaseMap(int width, int height, int basicID, int wallID)
    {
        setMapSize(width, height);
        fillGridObject(basicID, new Vector2Int(1, 1), new Vector2Int(width - 2, height - 2));
        fillBorderObject(wallID, new Vector2Int(0, 0), new Vector2Int(width - 1, height - 1));
    }

    public void createTile(int tileID, Vector2Int pos)
    {
        if (map[pos.x, pos.y] == null)
        {
            map[pos.x, pos.y] = Instantiate(tilePrefab, new Vector3(pos.x * cellDim, pos.y * cellDim, 0), Quaternion.identity, _tileParent);
        }
        map[pos.x, pos.y].GetComponent<DungeonTile>().setTile(tileID, pos);
    }

    public void createObject(int objectID, Vector2Int pos)
    {
        GameObject newObj = Instantiate(objectPrefab, new Vector3(pos.x * cellDim, pos.y * cellDim, 0), Quaternion.identity, _objectParent);
        newObj.GetComponent<DungeonObject>().setObject(objectID, pos);
        objects.Add(newObj);
    }

    public bool isPositionOutsideMap(Vector2Int pos)
    {
        return (pos.x > mapWidth - 1 || pos.x < 0 || pos.y > mapHeight - 1 || pos.y < 0);
    }

    public void fillGridObject(int tileID, Vector2Int start, Vector2Int end)
    {
        if (isPositionOutsideMap(start) || isPositionOutsideMap(end))
        {
            Debug.Log("Error fillGridObject. Grid outside map area. (" + start.x + ", " + start.y + ") to (" + end.x + ", " + end.y + ")");
            return;
        }

        int start_X = Mathf.Min(start.x, end.x);
        int start_Z = Mathf.Min(start.y, end.y);
        int end_X = Mathf.Max(start.x, end.x);
        int end_Z = Mathf.Max(start.y, end.y);
        for (int x = start_X; x <= end_X; x++)
        {
            for (int z = start_Z; z <= end_Z; z++)
            {
                createTile(tileID, new Vector2Int(x, z));
            }
        }
    }

    public void fillBorderObject(int tileID, Vector2Int start, Vector2Int end)
    {
        if (isPositionOutsideMap(start) || isPositionOutsideMap(end))
        {
            Debug.Log("Error fillGridObject. Grid outside map area. (" + start.x + ", " + start.y + ") to (" + end.x + ", " + end.y + ")");
            return;
        }

        int start_X = Mathf.Min(start.x, end.x);
        int start_Z = Mathf.Min(start.y, end.y);
        int end_X = Mathf.Max(start.x, end.x);
        int end_Z = Mathf.Max(start.y, end.y);
        for (int x = start.x; x <= end_X; x++)
        {
            createTile(tileID, new Vector2Int(x, start_Z));
            createTile(tileID, new Vector2Int(x, end_Z));
        }
        for (int z = start.y + 1; z < end_Z; z++)
        {
            createTile(tileID, new Vector2Int(start_X, z));
            createTile(tileID, new Vector2Int(end_X, z));
        }
    }

    private void deleteMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x, y] != null)
                {
                    Destroy(map[x, y]);
                }
            }
        }
    }

    private void destroyAllObjects()
    {
        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
        objects.Clear();
    }
}
