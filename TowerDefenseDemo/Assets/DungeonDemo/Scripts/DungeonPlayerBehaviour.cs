using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonPlayerBehaviour : MonoBehaviour
{

    public float timeToMove;

    // Read only variables
    public float moveProgress;
    public bool isMoving;
    public Vector2Int moveAction;
    public Vector2 start;
    public Vector2 target;
    private DungeonManager dungeonManager;


    // Start is called before the first frame update
    void Start()
    {
        isMoving = false;
        moveProgress = 0;
        moveAction = new Vector2Int();
    }

    // Update is called once per frame
    void Update()
    {
        if(dungeonManager == null)
        {
            dungeonManager = DungeonManager.Instance;
        }

        if(isMoving)
        {
            updateMovement();
        } else
        {
            updateMoveInput();
        }
    }

    public void updateMoveInput()
    {
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            move(new Vector2Int(0, 1));
        }
        else if(Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            move(new Vector2Int(-1, 0));
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            move(new Vector2Int(0, -1));
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            move(new Vector2Int(1, 0));
        }
    }

    public void move(Vector2Int moveAction)
    {
        this.moveAction = moveAction;
        if(moveAction.x != 0 || moveAction.y != 0)
        {
            moveProgress = 0;
            isMoving = true;
            start = new Vector2(transform.position.x, transform.position.y);
            target = new Vector2(transform.position.x + moveAction.x * dungeonManager.cellDim, 
                                 transform.position.y + moveAction.y * dungeonManager.cellDim);
        }
    }

    public void updateMovement()
    {
        moveProgress += Time.deltaTime / timeToMove;
        if(moveProgress >= 1)
        {
            moveProgress = 1;
            isMoving = false;
        }
        transform.position = new Vector3(Mathf.Lerp(start.x, target.x, moveProgress),
                                         Mathf.Lerp(start.y, target.y, moveProgress), 0);
    }
}
