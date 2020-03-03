using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAI
{
    public GameManager gameManager;

    protected Vector3Int scatterCell;
    protected int[,] behaviourCronogram;
    protected int minPellets = 0;
    protected int cronogramIndex = 0;
    protected int scatterTime = 7;
    protected int chaseTime = 20;

    public GhostAI(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public virtual Ghost.Mode ChooseMode(Ghost.Mode currentMode, int seconds, int pelletsEaten, bool intoPowerMode, Vector3Int currentCell, bool wasEaten,Vector3Int ghostHouseCell, int stage)
    {
        return Ghost.Mode.Wait;
    }

    public virtual Vector3Int ChooseTarget(Ghost.Mode currentMode, Vector3Int currentCell, Vector3Int ghostHouseCell, int stage)
    {
        return new Vector3Int(0, 0, 0);
    }

}
