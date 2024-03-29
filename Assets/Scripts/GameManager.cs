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
    [SerializeField] private Text levelValueText;
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
    [SerializeField] private AudioSource mainBackgroundMusic;
    [SerializeField] private float mainBackgroundMusicFasterPitch = 1.2f;
    [SerializeField] private float mainBackgroundMusicFastestPitch = 1.4f;

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
    private int lifeBonusScore;
    public int level;
    private int hiScore;
    private GameObject instantiatedPellets;
    private bool TVAPortalHasClosed = false;
    private Transform wallTilemap;
    private GameObject currentLTilemap;
    private GameObject currentKTilemap;
    private GameObject currentITilemap;

    void Start()
    {
        level = 1;
        levelValueText.text = level.ToString();
        score = 0;
        scoreValueText.text = "0";
        hiScore = PlayerPrefs.GetInt("HiScore");
        hiScoreValueText.text = hiScore.ToString();
        em = GetComponent<EnemyManager>();
        livesHUD = FindObjectOfType<LivesHUD>();
        loki = null;
        gameState = GameState.BeginScreen;
        lives = defaultLives;
        lifeBonusScore = 0;
        instantiatedPellets = Instantiate(pelletsPrefab);

        beginGameText.EnableText();
        ReadyText.enabled = false;
        pauseGameText.DisableText();
        wallTilemap = GameObject.FindGameObjectWithTag("Wall").transform;
        currentLTilemap = Instantiate(LTilemapList[0], wallTilemap.parent);
        currentKTilemap = Instantiate(KTilemapList[0], wallTilemap.parent);
        currentITilemap = Instantiate(ITilemapList[0], wallTilemap.parent);
        ResetMainBackgroundMusicPitch();
    }

    private void OnStartPause()
    {
        switch (gameState)
        {
            case GameState.BeginScreen:
                gameState = GameState.SettingUpGame;
                StartCoroutine(BeginRound(true));
                break;
            case GameState.InGame:
                PauseGame();
                break;
            case GameState.PauseScreen:
                UnPauseGame();
                break;
        }
    }

    private IEnumerator BeginRound(bool loseLifeOnSpawn)
    {
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
        TesseractPortal tesseractPortal = Instantiate(tesseractPortalPrefab, lokiSpawnPoint).GetComponent<TesseractPortal>();
        tesseractPortal.SetShouldRemoveLife(loseLifeOnSpawn);
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

    public void TryAddLife()
    {
        if (lives + 1 > defaultLives)
            return;

        lives++;
        livesHUD.AddLife();
    }

    public void AddLokiRef(Loki _loki)
    {
        loki = _loki;
        loki.UpdateLevelSpeedMultiplier(level);
    }

    public void FrightenLoki()
    {
        loki.Frighten();
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
        gameState = GameState.SettingUpGame;
        
        // If somehow someone has reached and completed level 99
        if (level >= 99)
        {
            FreezeGameAndInputs();
            em.StopAllLoops();
            yield return new WaitForSeconds(2);
            em.DestroyAllEnemies();
            Destroy(loki.gameObject);
            RemoveLokiRef();

            yield return new WaitForSeconds(2);
            if (score > hiScore)
            {
                hiScore = score;
                PlayerPrefs.SetInt("HiScore", score);
                PlayerPrefs.Save();
                hiScoreValueText.text = hiScore.ToString();
            }

            // Reset score, lives, pellets & return to begin screen
            score = 0;
            scoreValueText.text = score.ToString();
            level = 1;
            levelValueText.text = level.ToString();
            lives = defaultLives;
            livesHUD.ResetLives();
            lifeBonusScore = 0;
            Destroy(instantiatedPellets);
            instantiatedPellets = Instantiate(pelletsPrefab);
            // remove any bonus items
            gameState = GameState.BeginScreen;
            ResetMainBackgroundMusicPitch();
            RemoveAllInfinityStones();
            beginGameText.EnableText();
        }
        // Load a new round
        else
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
            level++;
            levelValueText.text = level.ToString();
            ResetMainBackgroundMusicPitch();
            RemoveAllInfinityStones();
            StartCoroutine(BeginRound(false));
        }
    }

    public void LoseLife(Enemy catcher)
    {
        StartCoroutine(LoseLifeCoroutine(catcher));
    }

    private IEnumerator LoseLifeCoroutine(Enemy catcher)
    {
        gameState = GameState.SettingUpGame;
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
            StartCoroutine(BeginRound(true));
        }
        else
        {
            // Reset score, lives, pellets & return to begin screen
            yield return new WaitForSeconds(2);
            if (score > hiScore)
            {
                hiScore = score;
                PlayerPrefs.SetInt("HiScore", score);
                PlayerPrefs.Save();
                hiScoreValueText.text = hiScore.ToString();
            }
            score = 0;
            scoreValueText.text = score.ToString();
            level = 1;
            levelValueText.text = level.ToString();
            lives = defaultLives;
            livesHUD.ResetLives();
            lifeBonusScore = 0;
            Destroy(instantiatedPellets);
            instantiatedPellets = Instantiate(pelletsPrefab);
            // remove any bonus items
            gameState = GameState.BeginScreen;
            ResetMainBackgroundMusicPitch();
            RemoveAllInfinityStones();
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

        // Check to reward player with extra life after each 10,000 points earned
        lifeBonusScore += points;
        if (lifeBonusScore >= 10_000)
        {
            TryAddLife();
            lifeBonusScore = 0;
        }
    }

    private void OnMouseSelect()
    {
        // mute button and reset button
    }

    private void OnMoveUp()
    {
        if (loki)
            loki.RegisterInput(Direction.Up);
    }

    private void OnMoveDown()
    {
        if (loki)
            loki.RegisterInput(Direction.Down);
    }

    private void OnMoveLeft()
    {
        if (loki)
            loki.RegisterInput(Direction.Left);
    }

    private void OnMoveRight()
    {
        if (loki)
            loki.RegisterInput(Direction.Right);
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

    private void RemoveAllInfinityStones()
    {
        var activeinfinityStones = FindObjectsOfType<InfinityStone>();
        foreach (InfinityStone infinityStone in activeinfinityStones)
        {
            Destroy(infinityStone.gameObject);
        }
    }

    public void ResetMainBackgroundMusicPitch()
    {
        mainBackgroundMusic.pitch = 1;
    }

    public void IncreaseMainBackgroundMusicPitchToFaster()
    {
        mainBackgroundMusic.pitch = mainBackgroundMusicFasterPitch;
    }

    public void IncreaseMainBackgroundMusicPitchToFastest()
    {
        mainBackgroundMusic.pitch = mainBackgroundMusicFastestPitch;
    }
}
