using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinky : GhostAI
{
    private new Vector3Int scatterCell = new Vector3Int(14, 17, 0);
    private new int minPellets = 0;

    public Blinky(GameManager gameManager) : base(gameManager) { }

    public override Ghost.Mode ChooseMode(Ghost.Mode currentMode, int seconds, int pelletsEaten, bool intoPowerMode, Vector3Int currentCell, bool wasEaten, Vector3Int ghostHouseCell, int stage)
    {
        if (currentMode == Ghost.Mode.Eyes && currentCell == ghostHouseCell)
            return Ghost.Mode.Scatter;
        if (pelletsEaten < (minPellets / (1 + 0.25 * stage)))
            return Ghost.Mode.Wait;
        if (wasEaten)
            return Ghost.Mode.Eyes;
        if (intoPowerMode)
            return Ghost.Mode.Frightened;

        int scatterTime = this.scatterTime;
        int chaseTime = this.chaseTime;

        if (stage < 7) scatterTime = 7 - stage / 2; else scatterTime = 1;
        chaseTime = 20 + stage / 2;

        cronogramIndex = seconds / (scatterTime + chaseTime);
        if (cronogramIndex > 4)
        {
            return Ghost.Mode.Chase;
        }
        else
        {
            if (seconds % (scatterTime + chaseTime) > scatterTime)
                return Ghost.Mode.Chase;
            else
                return Ghost.Mode.Scatter;
        }
    }

    public override Vector3Int ChooseTarget(Ghost.Mode currentMode, Vector3Int currentCell, Vector3Int ghostHouseCell, int stage)
    {
        switch (currentMode)
        {
            case Ghost.Mode.Scatter:
                return scatterCell;
            case Ghost.Mode.Chase:
                return gameManager.player.currentCell;
            case Ghost.Mode.Frightened:
                return new Vector3Int((int)Random.Range(-13f,12f), 
                    (int)Random.Range(-13f, 15f),
                    currentCell.z);
            case Ghost.Mode.Eyes:
                return ghostHouseCell;
            default: //wait
                return currentCell;
        }
   
    }
}
