using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class EnvironmentManager : MonoBehaviour
{
    [SerializeField] private Vector2Int minMaxBoundsX;
    public Vector2Int MinMaxBoundsX => minMaxBoundsX;
    private int rows => minMaxBoundsX.y - minMaxBoundsX.x + 1;
    [SerializeField] private Vector2Int minMaxBoundsY;
    public Vector2Int MinMaxBoundsY => minMaxBoundsY;
    private int columns => minMaxBoundsY.y - minMaxBoundsY.x + 1;
    private GridCellData[,] spawnedCells;
    private List<Laser> spawnedLasers = new List<Laser>();

    [Header("Prefabs")]
    [SerializeField] private GridCellData wallPrefab;
    [SerializeField] private GridCellData gridPrefab;
    [SerializeField] private Laser laserPrefab;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement p_Movement;
    [SerializeField] private ScoreAgent agent;
    [SerializeField] private GameObject coin;

    [Header("Game Settings")]
    [SerializeField] private int startingPlayerHP = 3;
    [SerializeField] private int currentPlayerHP;
    [SerializeField] private Vector2 minMaxTimeBetweenLasers;
    [SerializeField] private Vector2Int minMaxBeforeNewLaserCoroutine;
    [SerializeField] private Vector2 minMaxStaggerNewLaserCoroutine;
    [SerializeField] private bool useLasers;
    [SerializeField] private float gameLength = 25;
    private int score;
    private float countdownTimer;

    [Header("Lasers")]
    [SerializeField] private float laserPrepTime;
    [SerializeField] private float laserDelayTime;
    [SerializeField] private int maxLaserCoroutines = 5;
    private int nextLaser;
    private int numLaserCoroutines;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI prevScoreText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private CanvasGroup gameOverCV;
    [SerializeField] private float gameOverFadeInRate;
    [SerializeField] private bool isHumanPlaying;
    [SerializeField] private Color belowScoreGoalColor;
    [SerializeField] private Color atOrAboveScoreGoalColor;
    private bool restartGame;

    [Header("Screen Shake")]
    [SerializeField] private float onDamageShakeDuration;
    [SerializeField] private float onDamageShakeStrength = 3;
    [SerializeField] private int onDamageShakeVibrato = 10;
    [SerializeField] private float onDamageShakeRandomness = 90;

    [Header("AI")]
    [SerializeField] private bool resetUponHittingScoreGoal;
    [SerializeField] private int scoreGoal;
    [SerializeField] private float mlOnHitReward = -5;
    [SerializeField] private float mlOnPickupCoinReward = 50;

    private void Awake()
    {
        // Add a function call to the agent.CallOnEpisodeBegin Callback so that when the Episode begins, as does the game
        agent.CallOnEpisodeBegin += ResetGamestate;

        // Set the Player HP
        currentPlayerHP = startingPlayerHP;

        // Set Coin References
        foreach (RequiresEnvironmentManager comp in coin.GetComponents<RequiresEnvironmentManager>())
        {
            comp.SetEnvironmentManager(this);
        }

        // Spawn the Grid
        GenerateGrid();
    }

    private void Update()
    {
        // Set UI
        scoreText.text = "Score: " + score;
        scoreText.color = score >= scoreGoal ? atOrAboveScoreGoalColor : belowScoreGoalColor;
        hpText.text = "Health: " + currentPlayerHP;

        countdownTimer -= Time.deltaTime;
        timerText.text = "Timer: " + Mathf.RoundToInt(countdownTimer);
        if (countdownTimer <= 0)
        {
            agent.EndEpisode();
            return;
        }
    }

    private void CallOnGameStart()
    {
        // Set the Coin to a new Position
        SetNewCoinPos();

        // Set HP Text to show only if there is a possibility of losing HP
        hpText.gameObject.SetActive(useLasers);

        // Fires Lasers
        if (useLasers)
        {
            numLaserCoroutines = 1;
            StartCoroutine(LaserRoutine());
            SetNextLaserCounter();
        }
    }

    public void ResetGamestate()
    {
        // Set Previous Score Text
        if (score > 0)
        {
            prevScoreText.gameObject.SetActive(true);
            prevScoreText.text = "Previous Score: " + score;
            prevScoreText.color = score >= scoreGoal ? atOrAboveScoreGoalColor : belowScoreGoalColor;
        }
        // Reset the Score for another Go
        score = 0;

        // Reset the Timer
        countdownTimer = gameLength;

        // Reset the Player Position
        p_Movement.ResetTargetPos(GetCurrentPosAsVector2Int());
        agent.transform.position = transform.position;

        // Reset the Player HP
        currentPlayerHP = startingPlayerHP;

        // 
        CallOnGameStart();
    }

    // Sets the Coins Position to some Valid Spot on the Grid
    private void SetNewCoinPos()
    {
        coin.transform.position = GetRandomUnobstructedSpotOnGrid().transform.position;

        // Coin Spawns in as inactive, so this is to turn it active the first time this function runs
        coin.SetActive(true);
    }

    // Generates our game grid
    private void GenerateGrid()
    {
        //
        spawnedCells = new GridCellData[rows, columns];

        // Spawn Cells
        for (int i = 0; i < rows; i++)
        {
            for (int p = 0; p < columns; p++)
            {
                // Outside Layer is Lasers
                if (i == 0 ||
                    i == rows - 1 ||
                    p == 0 ||
                    p == columns - 1)
                {

                    Laser spawned = Instantiate(laserPrefab, transform.position + new Vector3(i - rows / 2, p - columns / 2, 0), Quaternion.identity, transform);
                    spawned.SetEnvironmentManager(this);
                    spawnedLasers.Add(spawned);
                    spawnedCells[i, p] = spawned;
                }
                else if (i == 1 ||
                    i == rows - 2 ||
                    p == 1 ||
                    p == columns - 2) // Just Inside of the Laser Layer is the Wall Layer
                {
                    spawnedCells[i, p] = Instantiate(wallPrefab, transform.position + new Vector3(i - rows / 2, p - columns / 2, 0), Quaternion.identity, transform);

                }
                else // Anything else should just be regular Grid
                {
                    spawnedCells[i, p] = Instantiate(gridPrefab, transform.position + new Vector3(i - rows / 2, p - columns / 2, 0), Quaternion.identity, transform);
                }
                spawnedCells[i, p].CellCoordinates = new Vector2Int(i, p);
                spawnedCells[i, p].gameObject.name += "<" + i + ", " + p + ">";
            }
        }
    }

    private void SetNextLaserCounter()
    {
        nextLaser = RandomHelper.RandomIntExclusive(minMaxBeforeNewLaserCoroutine);
    }

    private IEnumerator LaserRoutine()
    {
        // Debug.Log("Laser Routine");

        yield return new WaitForSeconds(RandomHelper.RandomFloat(minMaxTimeBetweenLasers));

        Laser randomLaser = RandomHelper.GetRandomFromList(spawnedLasers);

        StartCoroutine(randomLaser.Prep(player, laserPrepTime, () => StartCoroutine(randomLaser.Fire(laserDelayTime, null))));

        yield return new WaitUntil(() => !randomLaser.IsInUse);

        StartCoroutine(LaserRoutine());
        nextLaser -= 1;

        if (nextLaser <= 0 && numLaserCoroutines < maxLaserCoroutines)
        {
            yield return new WaitForSeconds(RandomHelper.RandomFloat(minMaxStaggerNewLaserCoroutine));
            numLaserCoroutines++;
            StartCoroutine(LaserRoutine());
            SetNextLaserCounter();
        }
    }

    private Vector2Int GetCurrentPosAsVector2Int()
    {
        return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    }

    public bool CanMove(Vector2Int currentCell, Direction d)
    {
        currentCell -= GetCurrentPosAsVector2Int();

        if (currentPlayerHP <= 0) return false;

        Vector2Int nextCell = new Vector2Int(currentCell.x, currentCell.y);
        switch (d)
        {
            case Direction.Up:
                nextCell += new Vector2Int(0, 1);
                break;
            case Direction.Down:
                nextCell -= new Vector2Int(0, 1);
                break;
            case Direction.Right:
                nextCell += new Vector2Int(1, 0);
                break;
            case Direction.Left:
                nextCell -= new Vector2Int(1, 0);
                break;
            default: throw new System.Exception();
        }

        if (nextCell.x > minMaxBoundsX.y || nextCell.x < minMaxBoundsX.x
            || nextCell.y > minMaxBoundsY.y || nextCell.y < minMaxBoundsY.x) return false;

        Obstruction obs;
        int accessX = nextCell.x + rows / 2;
        int accessY = nextCell.y + columns / 2;
        return !spawnedCells[accessX, accessY].TryGetComponent<Obstruction>(out obs);
    }

    // Returns a random spot on the grid that is not obstructed
    public GridCellData GetRandomUnobstructedSpotOnGrid()
    {
        // Initialize a new list
        List<GridCellData> possibleSpots = new List<GridCellData>();

        // Loop through each spawned Cell
        for (int i = 0; i < spawnedCells.GetLength(0); i++)
        {
            for (int p = 0; p < spawnedCells.GetLength(1); p++)
            {
                // if that cell is not Obstructed and is also not the cell at the current player position,
                // it may be added to the list of options
                if (!spawnedCells[i, p].TryGetComponent<Obstruction>(out Obstruction obs)
                    && p_Movement.TargetPosV3 != spawnedCells[i, p].transform.position)
                {
                    possibleSpots.Add(spawnedCells[i, p]);
                }
            }
        }

        // Return a random element from that list
        return RandomHelper.GetRandomFromList(possibleSpots);
    }

    public int GetNumStepsToPos(Vector3 fromPos, Vector3 toPos)
    {
        return GetNumStepsToPos(fromPos, toPos, 0);
    }

    private int GetNumStepsToPos(Vector3 fromPos, Vector3 toPos, int v)
    {
        if (fromPos == toPos)
        {
            return v;
        }

        if (fromPos.x != toPos.x)
        {
            fromPos.x = Mathf.MoveTowards(fromPos.x, toPos.x, 1);
        }
        else if (fromPos.y != toPos.y)
        {
            fromPos.y = Mathf.MoveTowards(fromPos.y, toPos.y, 1);
        }
        return GetNumStepsToPos(fromPos, toPos, v + 1);
    }

    public void IncreaseScore()
    {
        // Increase the Score
        score += 1;

        // Reward the Agent
        agent.AddReward(mlOnPickupCoinReward);
    }

    public void DamagePlayer()
    {
        // Screenshake
        Camera.main.DOShakePosition(onDamageShakeDuration, onDamageShakeStrength, onDamageShakeVibrato, onDamageShakeRandomness, true, ShakeRandomnessMode.Full);

        // Reduce the players HP
        currentPlayerHP -= 1;

        // De-incentivize the Agent from getting hit
        // We hate negative reinforcement here! (Overexageration, but "positive rewards are more often helpful to shaping the desired behaviour of an agent than negative rewards") -
        // https://github.com/miyamotok0105/unity-ml-agents/blob/master/docs/Learning-Environment-Best-Practices.md
        agent.AddReward(mlOnHitReward);

        // Runs when the Player dies (their HP is less than or equal to 0)
        if (currentPlayerHP <= 0)
        {
            // Stop the CoinRoutine Coroutine and the LaserRoutine Coruoutines
            StopAllCoroutines();

            // Reset all Lasers that are Currrently Firing
            foreach (Laser laser in spawnedLasers)
            {
                laser.Reset();
            }

            if (isHumanPlaying)
            {
                StartCoroutine(GameOverSequence());
            }
            else
            {
                // An episode is essentially a run
                // Whenever some end condition is met, the episode is over
                // Since there is no way to "win" this game, the only way for the episode to end is to die
                agent.EndEpisode();
            }
        }
    }

    // This function is called from a button
    public void RestartGame()
    {
        // Sets the game to be Restarted
        restartGame = true;
    }

    // Function to use when humans are playing
    private IEnumerator GameOverSequence()
    {
        // Fade in the end game Canvas
        while (gameOverCV.alpha < 1)
        {
            gameOverCV.alpha = Mathf.MoveTowards(gameOverCV.alpha, 1, Time.deltaTime * gameOverFadeInRate);
            yield return null;
        }
        // Allow the end game Canvas to block raycast so the player can interact with it
        gameOverCV.blocksRaycasts = true;

        // Wait until the player has hit the restart button
        yield return new WaitUntil(() => restartGame);
        restartGame = false;

        // Reset Canvas Group
        gameOverCV.alpha = 0;
        gameOverCV.blocksRaycasts = false;

        // Reload the Scene
        ResetGamestate();
    }
}
