using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonObject : MonoBehaviour
{
    private DungeonManager dungeonManager;
    public int objectID;
    public Vector2Int pos;

    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setObject(int objectID, Vector2Int pos)
    {
        if (dungeonManager == null)
        {
            dungeonManager = DungeonManager.Instance;
        }
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        this.objectID = objectID;
        this.pos = pos;
        gameObject.name = "Object (" + pos.x + "," + pos.y + "): " + objectID;

        spriteRenderer.sprite = dungeonManager.objectTextures[objectID];
    }
}
