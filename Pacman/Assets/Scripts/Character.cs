using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public enum Direction { none, up, down, left, right };
    public float speed = 1f;
    public Vector3Int currentCell;
    private Vector3Int moveCell;

    //protected Direction bufferedDir;
    public Direction bufferedDir;
    public Animator animator;

    //private Direction currentDir;
    //public Direction CurrentDir { get; }
    public Direction currentDir;
    public bool haveTeleported=false;

    protected GameManager gameManager;
    private bool moving = false;

    protected bool crossingDoor = false;
    protected Rigidbody2D rigidBody;
    protected SoundManager soundManager;

    public bool leavingHouse = false;
    public Vector3 startingPosition;

    protected virtual void Start()
    {

        GameObject bsGameObject = GameObject.FindWithTag("SoundManager");
        soundManager = bsGameObject.GetComponent(typeof(SoundManager)) as SoundManager;

        GameObject gmGameObject = GameObject.FindWithTag("GameManager");
        gameManager = gmGameObject.GetComponent(typeof(GameManager)) as GameManager;

        rigidBody = GetComponent(typeof(Rigidbody2D)) as Rigidbody2D;
        animator = GetComponent(typeof(Animator)) as Animator;

        startingPosition = transform.position;
    }

    protected virtual void Update()
    {
        currentCell = gameManager.grid.WorldToCell(transform.position);
        if (!moving)
        {

            if (CanTurn())
            {
                currentDir = bufferedDir;
                changeAnim(true);
            }

            if (((CanMove() && (currentDir != Direction.none)) || crossingDoor) && !leavingHouse)
            { //crossing door was made so that ghosts can cross door
                //which are walls
                Move();
                crossingDoor = false;
            }
            else
            {
                changeAnim(false);
            }
        }
    }

    protected virtual void changeAnim(bool value)
    {

    }

    private void Move() //move the character in the current direction
    {
        moving = true;
        Vector3 target;
    
        target = gameManager.grid.CellToWorld(moveCell);
        target = new Vector3(target.x+0.5f, target.y + 0.5f, transform.position.z);
        StartCoroutine(MovementCoroutine(target));
        
    }

    private IEnumerator MovementCoroutine (Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.05f && !haveTeleported)
        {
            float step = speed * Time.deltaTime;

            // Move our position a step closer to the target.
            transform.position = Vector3.MoveTowards(transform.position, target, step);

            yield return null;
        }
        haveTeleported = false;
        moving = false;
    }

    private bool CanTurn() //check in the grid if the character can change direction
    {

        switch (bufferedDir)
        {
            case (Direction.up):
                moveCell = new Vector3Int(currentCell.x, currentCell.y+1, currentCell.z);
                break;
            case (Direction.down):
                moveCell = new Vector3Int(currentCell.x, currentCell.y-1, currentCell.z);
                break;
            case (Direction.right):
                moveCell = new Vector3Int(currentCell.x+1, currentCell.y, currentCell.z);
                break;
            case (Direction.left):
                moveCell = new Vector3Int(currentCell.x-1, currentCell.y, currentCell.z);
                break;
            default: //none
                return true;
        }

        
        if (gameManager.tilemapWalls.GetTile(moveCell))
            return false;
        else
        {
            return true;
        }
           
    }

    private bool CanMove() //check in the grid if the character can change direction
    {

        switch (currentDir)
        {
            case (Direction.up):
                moveCell = new Vector3Int(currentCell.x, currentCell.y + 1, currentCell.z);
                break;
            case (Direction.down):
                moveCell = new Vector3Int(currentCell.x, currentCell.y - 1, currentCell.z);
                break;
            case (Direction.right):
                moveCell = new Vector3Int(currentCell.x + 1, currentCell.y, currentCell.z);
                break;
            case (Direction.left):
                moveCell = new Vector3Int(currentCell.x - 1, currentCell.y, currentCell.z);
                break;
            default: //left
                moveCell = new Vector3Int(currentCell.x - 1, currentCell.y, currentCell.z);
                break;
        }


        if (gameManager.tilemapWalls.GetTile(moveCell))
            return false;
        else
        {
            return true;
        }

    }

}
