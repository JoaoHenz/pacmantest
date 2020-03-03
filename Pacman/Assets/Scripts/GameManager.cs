using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Tilemap tilemapBase;
    public Tilemap tilemapWalls;
    public Tilemap edible;
    public int currentStage = 1;
    public Tile pelletTile;
    public Tile powerUpTile;
    public GridLayout grid;
    public int seconds =0;
    private float timer = 0f;
    public int pelletsEaten = 0;
    private bool intoPowerMode = false;
    public bool IntoPowerMode
    {
        get
        {
            return intoPowerMode;
        }
        set
        {
            intoPowerMode = value;
            StartCoroutine(PowerModeTimer());
        }
    }
    public Text scoreText;
    public int ghostEatenPowerMode = 0;
    public bool gameIsPaused = false;
    public bool forcedPause = false;
    public bool mustUnpause = false;
    public Player player;
    public List<Ghost> ghosts;
    private int score = 0;
    public int Score
    {
        get
        {
            return score;
        }
        set
        {
            scoreText.text = value.ToString();
            score = value;

            if(score/pointsPerLife > lifesExtended)
            {
                lifeExtension.Play();
                lifesExtended += 1;
                lives += 1;
                updateLives();

            }

            if (value > highScore)
            {
                HighScore = value;
            }
        }
    }
    public Text highScoreText;
    private int highScore;
    public int HighScore
    {
        get
        {
            return highScore;
        }
        set
        {
            highScoreText.text = value.ToString();
            highScore = value;
        }
    }
    public int lives = 3;
    public string animatorBlueGhost;
    public string animatorEyesGhost;
    public List<Vector3Int> doors = new List<Vector3Int>();
    public Vector3Int ghostHouseCell = new Vector3Int(0, 5, 0);
    private float powerModeLimit = 5f;
    private SoundManager soundManager;
    private Vector3 begginingFruit = new Vector3(15,-7,0);
    private Vector3 lastFruit = new Vector3(25, -7, 0);
    private Vector3 begginingLife = new Vector3(15, -0.2f, 0);
    private Vector3 lastLife = new Vector3(25, -0.2f, 0);
    private Vector3 nextFruit = new Vector3(15, -7, 0);
    private Vector3 nextLife = new Vector3(15, -0.2f, 0);
    public Tilemap tileUI;
    public Tile liveTile;
    public Text gameOverText;
    public Text readyText;
    public AudioSource death1;
    public AudioSource death2;
    private bool beginGame = true;
    public bool deathIsHandled = true;
    private int maxPellets = 242; //242
    public GameObject ediblePrefab;
    public GameObject edibleInstance;
    public GameObject grid_gameobject;
    public int stage = 1;
    public float stageSpeedInc = 0.1f;
    public List<GameObject> fruitPrefabs;
    public List<int> fruitRewards;
    public int fruitThreshhold;
    public Vector3 fruitSpawnPos = new Vector3(0,-0.5f,1.5f);
    public bool fruitHasSpawned = false;
    public int currentFruit = 0;
    public Text scoreGained;
    public GameObject fruit = null;
    private List<SpriteRenderer> ghostsRenderers = new List<SpriteRenderer>();
    private int timeLastPowerMode = 0;
    public float ghostSpeed;
    public List<Vector3> fruitPositions;
    public int nextFruitPos = 0;
    private int lifesExtended= 0;
    public int pointsPerLife = 10000;
    public AudioSource lifeExtension;
    public float powerModeDecrease = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        GameObject bsGameObject = GameObject.FindWithTag("SoundManager");
        soundManager = bsGameObject.GetComponent(typeof(SoundManager)) as SoundManager;
        edible = edibleInstance.GetComponent(typeof(Tilemap)) as Tilemap;

        Score = 0;

        if (PlayerPrefs.HasKey("highScore"))
        {
            HighScore = PlayerPrefs.GetInt("highScore");
        }
        else
        {
            HighScore = 0;
        }

        doors.Add(new Vector3Int(-1, 4, 0));
        doors.Add(new Vector3Int(0, 4, 0));

        //doorsEyes.Add(new Vector3Int(-2, 3, 0));
        //doorsEyes.Add(new Vector3Int(1, 3, 0));
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        player = playerGameObject.GetComponent(typeof(Player)) as Player;
        updateLives();

        int i;

        for(i=0;i<ghosts.Count;i++)
            ghostsRenderers.Add(ghosts[i].GetComponent(typeof(SpriteRenderer)) as SpriteRenderer);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (beginGame)
        {
            StartCoroutine(beginGameSequence(false));
            beginGame = false;
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!gameIsPaused)
                StartCoroutine(pauseGame(false));
            else
            {
                if (!forcedPause)
                    mustUnpause = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("MainMenu");

        if (!gameIsPaused)
        {
            timer += Time.deltaTime;
            seconds = (int)(timer % 60);

            if(pelletsEaten == maxPellets)
                StartCoroutine(beginGameSequence(false,true));

        }
    }

    public void checkFruitSpawn()
    {
        if (!fruitHasSpawned)
        {
            if (pelletsEaten > fruitThreshhold)
            {
                fruit = Instantiate(fruitPrefabs[currentFruit]);
                fruit.transform.position = fruitSpawnPos;

                fruitHasSpawned = true;
            }
        }
    }

    public int fruitReward()
    {
        return fruitRewards[currentFruit];
    }

    public IEnumerator ShowScoreGained(int score, Vector3 pos)
    {
        scoreGained.gameObject.transform.position = pos;

        scoreGained.text = score.ToString();
        scoreGained.enabled = true;

        yield return new WaitForSeconds(1f);

        scoreGained.enabled = false;

    }

    private IEnumerator beginGameSequence(bool lostLife=false, bool newStage = false)
    {
        int i;

        gameIsPaused = true;
        this.forcedPause = true;
        player.enabled = false;

        for (i = 0; i < ghosts.Count; i++)
            ghosts[i].enabled = false;

        soundManager.StopSiren();

        if (lostLife)
        {
            intoPowerMode = false;
            player.isDead = true;
            player.transform.eulerAngles = new Vector3(0, 0, 0);

            yield return new WaitForSeconds(0.5f);

            
            for (i = 0; i < ghostsRenderers.Count; i++)
                ghostsRenderers[i].enabled = false;
            

            yield return new WaitForSeconds(0.5f);

            death1.Play();
            player.animator.SetBool("isDead", true);
            player.bufferedDir = Character.Direction.none;

            yield return new WaitForSeconds(2.7f);
            death2.Play();
            yield return new WaitForSeconds(0.2f);
            death2.Play();
            yield return new WaitForSeconds(1f);

            player.transform.position = player.startingPosition;
            player.haveTeleported = true;

            for (i = 0; i < ghosts.Count; i++)
            {
                ghosts[i].transform.position = ghosts[i].startingPosition;
                ghosts[i].haveTeleported = true;
            }


            for (i = 0; i < ghostsRenderers.Count; i++)
                ghostsRenderers[i].enabled = true;
            


            player.isDead = false;
            player.animator.SetBool("isDead", false);
            player.bufferedDir = Character.Direction.none;

            ghosts[0].currentMode = Ghost.Mode.Chase;
            for (i = 1; i < ghosts.Count; i++)
                ghosts[i].currentMode = Ghost.Mode.Wait;


            readyText.enabled = true;
        }

        if (newStage)
        {
            intoPowerMode = false;
            soundManager.StopAll();
            yield return new WaitForSeconds(0.5f);

            for (i = 0; i < ghostsRenderers.Count; i++)
                ghostsRenderers[i].enabled = false;

            if (fruit)
                Destroy(fruit);

            yield return new WaitForSeconds(2f);


            player.transform.position = player.startingPosition;
            player.haveTeleported = true;

            for (i = 0; i < ghosts.Count; i++)
            {
                ghosts[i].transform.position = ghosts[i].startingPosition;
                ghosts[i].haveTeleported = true;
            }

            for (i = 0; i < ghostsRenderers.Count; i++)
                ghostsRenderers[i].enabled = true;


            readyText.enabled = true;
            Vector3 localTransEdible = edibleInstance.transform.localPosition;
            Destroy(edibleInstance);
            edibleInstance =  Instantiate(ediblePrefab);
            edibleInstance.transform.parent = grid_gameobject.transform;
            edibleInstance.transform.localPosition = localTransEdible;
            edible = edibleInstance.GetComponent(typeof(Tilemap)) as Tilemap;
            
            pelletsEaten = 0;
            stage += 1;

            player.speed += stageSpeedInc;


            ghostSpeed += stageSpeedInc;

            fruitHasSpawned = false;

            if ((int)(stage / 2) < fruitPrefabs.Count)
                currentFruit = (int)(stage / 2);
            else
                currentFruit = fruitPrefabs.Count-1;

            if (powerModeLimit - powerModeDecrease > 0)
                powerModeLimit -= powerModeDecrease;

            ghosts[0].currentMode = Ghost.Mode.Chase;
            for (i = 1; i < ghosts.Count; i++)
                ghosts[i].currentMode = Ghost.Mode.Wait;

            soundManager.CheckSiren(0);
            soundManager.StopSiren();

        }


        if (lostLife || newStage)
            yield return new WaitForSeconds(2f);
        else
        {
            yield return new WaitForSeconds(4.5f);
        }

        player.enabled = true;

        for (i = 0; i < ghosts.Count; i++)
            ghosts[i].enabled = true;

        if (lostLife || newStage)
        {
            for (i = 0; i < ghosts.Count; i++)
                ghosts[i].restoreGhost();
        }

        readyText.enabled = false;

        if(newStage)
            edibleInstance.name = "Edible";

        mustUnpause = false;
        gameIsPaused = false;
        this.forcedPause = false;
        soundManager.PlaySiren();
        deathIsHandled = true;


    }

    private IEnumerator gameOverSequence()
    {
        int i;

        gameIsPaused = true;
        this.forcedPause = true;
        player.enabled = false;
       
        for (i = 0; i < ghostsRenderers.Count; i++)
            ghosts[i].enabled = false;


        soundManager.StopSiren();


        player.isDead = true;
        player.transform.eulerAngles = new Vector3(0, 0, 0);

        yield return new WaitForSeconds(0.5f);

        for (i = 0; i < ghostsRenderers.Count; i++)
            ghostsRenderers[i].enabled = false;

        yield return new WaitForSeconds(0.5f);

        death1.Play();
        player.animator.SetBool("isDead", true);
        player.bufferedDir = Character.Direction.none;

        yield return new WaitForSeconds(2.7f);
        death2.Play();
        yield return new WaitForSeconds(0.2f);
        death2.Play();
        yield return new WaitForSeconds(1f);


        for (i = 0; i < ghostsRenderers.Count; i++)
            ghostsRenderers[i].enabled = true;


        gameOverText.enabled = true;

        PlayerPrefs.SetInt("highScore", highScore);

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("MainMenu");

    }

    public void updateLives()
    {
        int i;
        for (i = 15; i <= 25; i += 2)
        {
            tileUI.SetTile(grid.WorldToCell(new Vector3(i,begginingLife.y,begginingLife.z)), null);
        }
        i = 15;
        int j = 0;
        while(i<=25 && j < lives)
        {

            tileUI.SetTile(grid.WorldToCell(new Vector3(i, begginingLife.y, begginingLife.z)), liveTile);
            i += 2;
            j += 1;
        }
    }

    //if there are more lives, restart, else, go to the main menu
    public void handlePlayerDeath()
    {
        lives -= 1;
        updateLives();

        if (lives >= 0)
        {
            StartCoroutine(beginGameSequence(true));
        }
        else {

            StartCoroutine(gameOverSequence());
        }
    }

    //stop the movement of everything - forced pause is a pause by the system
    public IEnumerator pauseGame(bool forcedPause,bool soundToo=false)
    {
        int i;

        gameIsPaused = true;
        this.forcedPause = forcedPause;

        player.enabled = false;

        for (i = 0; i < ghostsRenderers.Count; i++)
            ghosts[i].enabled = false;

        if (soundToo)
            soundManager.enabled = false ;

        while(!mustUnpause)
            yield return new WaitForSeconds(0.1f);

        player.enabled = true;

        for (i = 0; i < ghostsRenderers.Count; i++)
            ghosts[i].enabled = true;

        if (soundToo)
            soundManager.enabled = true;

        mustUnpause = false;
        gameIsPaused = false;
        this.forcedPause = false;
    }

    public void timedPauseGame(float pauseSeconds, bool soundToo = false)
    {
        StartCoroutine(coTimedPauseGame(pauseSeconds,soundToo));


    }

    public IEnumerator coTimedPauseGame(float pauseSeconds, bool soundToo = false)
    {

        StartCoroutine(pauseGame(true, soundToo));

        yield return new WaitForSeconds(pauseSeconds);

        soundManager.enabled = true;
        mustUnpause = true;
    }

    private IEnumerator PowerModeTimer()
    {
        int begginingSeconds = seconds;
        ghostEatenPowerMode = 0;
        soundManager.PlayPower(true);

        bool powerModeEnding = false;
        while (seconds - begginingSeconds < powerModeLimit)
        {
            if (!powerModeEnding && seconds - begginingSeconds > 2)
            {
                powerModeEnding = true;
                StartCoroutine(BlinkGhosts());
            }
            yield return new WaitForSeconds(0.1f);
        }
            

        soundManager.PlayPower(false);
        intoPowerMode = false;
        timeLastPowerMode = seconds;
    }

    private IEnumerator BlinkGhosts()
    {
        int i;
        while (IntoPowerMode)
        {
            if (IntoPowerMode)
            {
                for (i = 0; i < ghostsRenderers.Count; i++)
                {
                    if(ghosts[i].currentMode==Ghost.Mode.Frightened)
                        ghostsRenderers[i].color = Color.red;
                }
                    
            }
            yield return new WaitForSeconds(0.2f);
            if (intoPowerMode)
            {
                for (i = 0; i < ghostsRenderers.Count; i++)
                {
                    if (ghosts[i].currentMode == Ghost.Mode.Frightened)
                        ghostsRenderers[i].color = Color.white;
                }
            }
            yield return new WaitForSeconds(0.2f);

        }

        for (i = 0; i < ghostsRenderers.Count; i++)
            ghostsRenderers[i].color = Color.white;

    }

    public int secondsSincePowerMode()
    {
        return seconds - timeLastPowerMode;
    }
}
