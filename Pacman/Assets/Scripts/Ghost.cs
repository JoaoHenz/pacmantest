using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : Character
{
    public enum Nickname {Clyde, Pinky, Blinky, Inky};
    public Nickname nickname;
    public GhostAI ghostAI;
    public enum Mode {Wait, Frightened, Eyes, Scatter, Chase };
    public Mode currentMode;
    private Mode oldMode;
    private bool changedMode = false;

    

    private string nameAnimator;

    private bool wasEaten;

    private Vector3Int targetCell;


    protected override void Start()
    {
        
        base.Start();
        switch (nickname)
        {
            case Nickname.Clyde:
                ghostAI = new Clyde(gameManager);
                nameAnimator = "Clyde";
                break;
            case Nickname.Pinky:
                ghostAI = new Pinky(gameManager);
                nameAnimator = "Pinky";
                break;
            case Nickname.Blinky:
                ghostAI = new Blinky(gameManager);
                nameAnimator = "Blinky";
                break;
            case Nickname.Inky:
                ghostAI = new Inky(gameManager);
                nameAnimator = "Inky";
                break;

        }
        bufferedDir = Direction.left;

        speed = gameManager.ghostSpeed;
    }

    protected override void Update()
    {
        base.Update();

        oldMode = currentMode;
        changedMode = false;
        currentMode = ghostAI.ChooseMode(currentMode,gameManager.secondsSincePowerMode(), gameManager.pelletsEaten,gameManager.IntoPowerMode,currentCell,wasEaten, gameManager.ghostHouseCell, gameManager.stage);


        if (currentMode != Mode.Wait)
        {

            if (oldMode != currentMode)
                changedMode = true;

            if (changedMode)
            {
                if (oldMode == Mode.Eyes)
                {
                    animator.runtimeAnimatorController = Resources.Load(nameAnimator) as RuntimeAnimatorController;
                    wasEaten = false;
                    speed = gameManager.ghostSpeed;
                    soundManager.retreating.Stop();
                }
                if (currentMode == Mode.Frightened)
                    animator.runtimeAnimatorController = Resources.Load( gameManager.animatorBlueGhost) as RuntimeAnimatorController;
                if (oldMode == Mode.Frightened)
                    animator.runtimeAnimatorController = Resources.Load(nameAnimator) as RuntimeAnimatorController;
                if (currentMode == Mode.Eyes)
                {
                    animator.runtimeAnimatorController = Resources.Load(gameManager.animatorEyesGhost) as RuntimeAnimatorController;
                    speed = gameManager.ghostSpeed * 2;
                    soundManager.eatGhost.Play();
                    gameManager.ghostEatenPowerMode += 1;
                    StartCoroutine(gameManager.ShowScoreGained((int)(100 * Mathf.Pow(2, gameManager.ghostEatenPowerMode)), transform.position));
                    gameManager.timedPauseGame(0.5f);
                    soundManager.retreating.Play();

                    
                    gameManager.Score += (int)(100 * Mathf.Pow(2, gameManager.ghostEatenPowerMode));

                }
                if(oldMode == Mode.Wait)
                {

                    leavingHouse = true;
                    haveTeleported = true;
                    StartCoroutine(LeaveHouse());
                }
            }

            if (atIntersec() || changedMode)
            {
                targetCell = ghostAI.ChooseTarget(currentMode, currentCell,gameManager.ghostHouseCell, gameManager.stage); //each AI has a different target for each mode
                bufferedDir = ChooseDir();

            }
        }
    }

    public void restoreGhost()
    {
        animator.runtimeAnimatorController = Resources.Load(nameAnimator) as RuntimeAnimatorController;
        wasEaten = false;
        speed = gameManager.ghostSpeed;
        soundManager.retreating.Stop();
    }


    private IEnumerator LeaveHouse()
    {
        Vector3 target;

        target = new Vector3(0.5f, 2.5f, 1);
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            float step = speed * Time.deltaTime;

            // Move our position a step closer to the target.
            transform.position = Vector3.MoveTowards(transform.position, target, step);

            yield return null;
        }

        target = new Vector3(0.5f, 5.5f, 1);
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            float step = speed * Time.deltaTime;

            // Move our position a step closer to the target.
            transform.position = Vector3.MoveTowards(transform.position, target, step);

            yield return null;
        }

        leavingHouse = false;
    }



    private Direction ChooseDir() //the ghost chooses the best way to reach its target cell
    {
        Vector3 targetCellPos = gameManager.grid.CellToWorld(targetCell);
        targetCellPos = new Vector3(targetCellPos.x + 0.5f, targetCellPos.y + 0.5f, targetCellPos.z);
        Direction bestDir = Direction.none;
        Vector3 pos = transform.position;

        Tilemap walls = gameManager.tilemapWalls;

        float smallestDist = 99999f;

        float[] distances = new float[4];

        distances[0] = Vector3.Distance(new Vector3(pos.x, pos.y + 1, pos.z), targetCellPos); // get the distance for every direction
        distances[1] = Vector3.Distance(new Vector3(pos.x, pos.y - 1, pos.z), targetCellPos);
        distances[2] = Vector3.Distance(new Vector3(pos.x + 1, pos.y, pos.z), targetCellPos);
        distances[3] = Vector3.Distance(new Vector3(pos.x - 1, pos.y, pos.z), targetCellPos);

        if (walls.GetTile(new Vector3Int(currentCell.x, currentCell.y + 1, currentCell.z))) //stop the ghost from getting stuck at walls
        {
            if (!gameManager.doors.Contains(new Vector3Int(currentCell.x, currentCell.y + 1, currentCell.z)))
                distances[0] = 99999f; //this test was made so the ghosts can leave the ghosthouse
            else
            {
                crossingDoor = true;
            }
        }
        if (walls.GetTile(new Vector3Int(currentCell.x, currentCell.y - 1, currentCell.z)))
        {
            if (currentMode != Mode.Eyes)
                distances[1] = 99999f; // this one was made so the ghost eyes can re-enter it
            else
            {
                if(gameManager.doors.Contains(new Vector3Int(currentCell.x, currentCell.y - 1, currentCell.z)))
                {
                    crossingDoor = true;
                }
                else
                {
                    distances[1] = 99999f;
                }
            }
        }
        if (walls.GetTile(new Vector3Int(currentCell.x + 1, currentCell.y, currentCell.z)))
            distances[2] = 99999f;
        if (walls.GetTile(new Vector3Int(currentCell.x - 1, currentCell.y, currentCell.z)))
            distances[3] = 99999f;


        switch (currentDir)   //stop the ghost from turning back
        {
            case Direction.up:
                distances[1] = 99999f;
                break;
            case Direction.down:
                distances[0] = 99999f;
                break;
            case Direction.right:
                distances[3] = 99999f;
                break;
            case Direction.left:
                distances[2] = 99999f;
                break;
        }
        
        if (smallestDist > distances[0]) //get the best direction
        {
            smallestDist = distances[0];
            bestDir = Direction.up;
        }
        if (smallestDist > distances[1])
        {
            smallestDist = distances[1];
            bestDir = Direction.down;
        }
        if (smallestDist > distances[2])
        {
            smallestDist = distances[2];
            bestDir = Direction.right;
        }
        if (smallestDist > distances[3])
        {
            smallestDist = distances[3];
            bestDir = Direction.left;
        }


        return bestDir;

    }

    private bool atIntersec() // test if the ghost is at a intersection
    {
        Tilemap walls = gameManager.tilemapWalls;

        switch (currentDir)
        {
            case Direction.up:
                if (walls.GetTile(new Vector3Int(currentCell.x + 1, currentCell.y, currentCell.z))
                    && walls.GetTile(new Vector3Int(currentCell.x - 1, currentCell.y, currentCell.z)))
                {
                    return false; // parede de ambos os lados
                }
                break;
            case Direction.down:
                if (walls.GetTile(new Vector3Int(currentCell.x + 1, currentCell.y, currentCell.z))
                    && walls.GetTile(new Vector3Int(currentCell.x - 1, currentCell.y, currentCell.z)))
                {
                    return false;
                }
                break;
            case Direction.right:
                if (walls.GetTile(new Vector3Int(currentCell.x, currentCell.y+1, currentCell.z))
                    && walls.GetTile(new Vector3Int(currentCell.x, currentCell.y-1, currentCell.z))){
                    return false; // parede em cima e em baixo
                }
                break;
            case Direction.left:
                if (walls.GetTile(new Vector3Int(currentCell.x, currentCell.y+1, currentCell.z))
                    && walls.GetTile(new Vector3Int(currentCell.x, currentCell.y-1, currentCell.z))){
                    return false;
                }
                break;
        }
        return true;
    }

    protected override void changeAnim(bool value)
    {

        switch (currentDir)
        {
            case (Direction.up):
                animator.SetInteger("Direction", 0);
                break;
            case (Direction.down):
                animator.SetInteger("Direction", 1);
                break;
            case (Direction.right):
                animator.SetInteger("Direction", 2);
                break;
            case (Direction.left):
                animator.SetInteger("Direction", 3);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent(typeof(Player)) && currentMode != Mode.Frightened && currentMode != Mode.Eyes && gameManager.deathIsHandled == true)
        {
            gameManager.deathIsHandled = false;
            gameManager.handlePlayerDeath();
        }
        if (collision.gameObject.GetComponent(typeof(Player)) && currentMode == Mode.Frightened)
        {
            wasEaten = true;
        }


    }

}
