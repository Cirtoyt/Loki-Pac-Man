using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextBlinker beginGameText;
    [SerializeField] private Text ReadyText;
    [SerializeField] private TextBlinker pauseGameText;
    [SerializeField] private Text scoreValueText;
    [SerializeField] private Text hiScoreValueText;
    [SerializeField] private GameObject tesseractPortalPrefab;
    [SerializeField] private GameObject lokiPrefab;
    [SerializeField] private GameObject pelletsPrefab;
    [SerializeField] private GameObject TVAPortalPrefab;
    [SerializeField] private Transform lokiSpawnPoint;
    [SerializeField] private int defaultLives;
    [SerializeField] private List<GameObject> LTilemapList;
    [SerializeField] private List<GameObject> KTilemapList;
    [SerializeField] private List<GameObject> ITilemapList;

    public enum GameState
    {
        BeginScreen,
        SettingUpGame,
        InGame,
        PauseScreen,
    }

    private EnemyManager em;
    private LivesHUD livesHUD;
    private Loki loki;
    private GameState gameState;
    private int lives;
    private int score;
    private int hiScore = 0;
    private GameObject instantiatedPellets;
    private bool TVAPortalHasClosed = false;
    private Transform wallTilemap;
    private GameObject currentLTilemap;
    private GameObject currentKTilemap;
    private GameObject currentITilemap;

    void Start()
    {
        scoreValueText.text = "0";
        hiScoreValueText.text = "0";
        em = GetComponent<EnemyManager>();
        livesHUD = FindObjectOfType<LivesHUD>();
        loki = null;
        gameState = GameState.BeginScreen;
        lives = defaultLives;
        instantiatedPellets = Instantiate(pelletsPrefab);

        beginGameText.EnableText();
        ReadyText.enabled = false;
        pauseGameText.DisableText();
        wallTilemap = GameObject.FindGameObjectWithTag("Wall").transform;
        currentLTilemap = Instantiate(LTilemapList[0], wallTilemap.parent);
        currentKTilemap = Instantiate(KTilemapList[0], wallTilemap.parent);
        currentITilemap = Instantiate(ITilemapList[0], wallTilemap.parent);
    }

    private void OnStartPause()
    {
        switch (gameState)
        {
            case GameState.BeginScreen:
                StartCoroutine(BeginGame());
                break;
            case GameState.InGame:
                PauseGame();
                break;
            case GameState.PauseScreen:
                UnPauseGame();
                break;
        }
    }

    private IEnumerator BeginGame()
    {
        gameState = GameState.SettingUpGame;
        beginGameText.DisableText();
        ReadyText.enabled = true;
        RandomiseLogo();
        yield return new WaitForSeconds(0.5f);
        RandomiseLogo();
        yield return new WaitForSeconds(0.5f);
        RandomiseLogo();
        yield return new WaitForSeconds(0.5f);
        RandomiseLogo();
        yield return new WaitForSeconds(0.5f);
        RandomiseLogo();
        yield return new WaitForSeconds(0.5f);
        RandomiseLogo();
        yield return new WaitForSeconds(0.5f);
        // Begin Loki spawn animation (takes 1 second)
        Instantiate(tesseractPortalPrefab, lokiSpawnPoint);
        yield return new WaitUntil(() => loki);
        em.SpawnEnemies();
        yield return new WaitForSeconds(1);
        // Game begins!
        ReadyText.enabled = false;
        loki.Unfreeze();
        em.BeginRound();
        gameState = GameState.InGame;
    }

    public void RemoveLife()
    {
        lives--;
        livesHUD.RemoveLife();
    }

    public void AddLokiRef(Loki _loki)
    {
        loki = _loki;
    }

    private void RandomiseLogo()
    {
        Destroy(currentLTilemap);
        Destroy(currentKTilemap);
        Destroy(currentITilemap);

        currentLTilemap = Instantiate(LTilemapList[Random.Range(0, LTilemapList.Count)], wallTilemap.parent);
        currentKTilemap = Instantiate(KTilemapList[Random.Range(0, KTilemapList.Count)], wallTilemap.parent);
        currentITilemap = Instantiate(ITilemapList[Random.Range(0, ITilemapList.Count)], wallTilemap.parent);
    }

    private void PauseGame()
    {
        gameState = GameState.PauseScreen;
        pauseGameText.EnableText();
        FreezeGameAndInputs();
    }

    private void FreezeGameAndInputs()
    {
        loki.Freeze();
        em.FreezeEnemies();
    }

    public void FreezeGameButNoPauseText()
    {
        gameState = GameState.PauseScreen;
        loki.Freeze();
        em.FreezeEnemies();
    }

    private void UnPauseGame()
    {
        gameState = GameState.InGame;
        pauseGameText.DisableText();
        UnfreezeGameAndInputs();
    }

    private void UnfreezeGameAndInputs()
    {
        loki.Unfreeze();
        em.UnfreezeEnemies();
    }

    public void UnfreezeGameButNoPauseText()
    {
        gameState = GameState.InGame;
        loki.Unfreeze();
        em.UnfreezeEnemies();
    }

    public void WinRound()
    {
        StartCoroutine(WinRoundCoroutine());
    }

    private IEnumerator WinRoundCoroutine()
    {
        FreezeGameAndInputs();
        em.StopAllLoops();
        yield return new WaitForSeconds(2);
        em.DestroyAllEnemies();
        Destroy(loki.gameObject);
        loki = null;
        yield return new WaitForSeconds(1);
        Destroy(instantiatedPellets);
        instantiatedPellets = Instantiate(pelletsPrefab);
        StartCoroutine(BeginGame());
    }

    public void LoseRound(Enemy catcher)
    {
        StartCoroutine(LoseRoundCoroutine(catcher));
    }

    private IEnumerator LoseRoundCoroutine(Enemy catcher)
    {
        FreezeGameAndInputs();
        em.StopAllLoops();
        em.DestroyEnemiesBarOne(catcher);
        Instantiate(TVAPortalPrefab, loki.transform.position, loki.transform.rotation);
        yield return new WaitUntil(() => TVAPortalHasClosed);
        TVAPortalHasClosed = false;
        yield return new WaitForSeconds(1);
        em.DestroyEnemy(catcher);
        if (lives > 0)
        {
            // Start next round
            StartCoroutine(BeginGame());
        }
        else
        {
            // Reset score, lives, pellets & return to begin screen
            yield return new WaitForSeconds(2);
            if (score > hiScore)
            {
                hiScore = score;
                hiScoreValueText.text = hiScore.ToString();
            }
            score = 0;
            scoreValueText.text = score.ToString();
            lives = defaultLives;
            livesHUD.ResetLives();
            Destroy(instantiatedPellets);
            instantiatedPellets = Instantiate(pelletsPrefab);
            // remove any bonus items
            gameState = GameState.BeginScreen;
            beginGameText.EnableText();
        }
    }

    public void RemoveLokiRef()
    {
        loki = null;
    }

    public void CloseTVAPortal()
    {
        TVAPortalHasClosed = true;
    }

    public int GetScore()
    {
        return score;
    }

    public void AddScore(int points)
    {
        score += points;
        scoreValueText.text = score.ToString();
    }

    private void OnMouseSelect()
    {
        // mute button and reset button
    }

    private void OnMoveUp()
    {
        if (loki)
            loki.CompareNewInput(Direction.Up);
    }

    private void OnMoveDown()
    {
        if (loki)
            loki.CompareNewInput(Direction.Down);
    }

    private void OnMoveLeft()
    {
        if (loki)
            loki.CompareNewInput(Direction.Left);
    }

    private void OnMoveRight()
    {
        if (loki)
            loki.CompareNewInput(Direction.Right);
    }

    private void OnExitGame()
    {
        Application.Quit();
    }

    public Transform GetSpawnPoint()
    {
        return lokiSpawnPoint;
    }

    public GameObject GetLokiPrefab()
    {
        return lokiPrefab;
    }

    public GameState GetGameState()
    {
        return gameState;
    }
}
