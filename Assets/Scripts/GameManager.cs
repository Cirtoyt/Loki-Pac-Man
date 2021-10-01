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
    [SerializeField] private GameObject tesseractPrefab;
    [SerializeField] private GameObject lokiPrefab;
    [SerializeField] Transform lokiSpawnPoint;
    [SerializeField] private int defaultLives;
    [SerializeField] private int maxLives;
    [SerializeField] GameObject pelletsPrefab;

    public enum GameState
    {
        BeginScreen,
        SettingUpGame,
        InGame,
        PauseScreen,
    }

    private EnemyManager em;
    private Loki loki;
    private GameState gameState;
    private int lives;
    private int score;
    private GameObject instantiatedPellets;

    void Start()
    {
        scoreValueText.text = "0";
        em = GetComponent<EnemyManager>();
        loki = null;
        gameState = GameState.BeginScreen;
        lives = defaultLives;
        instantiatedPellets = Instantiate(pelletsPrefab);

        beginGameText.EnableText();
        ReadyText.enabled = false;
        pauseGameText.DisableText();
    }

    void Update()
    {
        
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
        yield return new WaitForSeconds(1);
        RandomiseLogo();
        yield return new WaitForSeconds(1);
        RandomiseLogo();
        yield return new WaitForSeconds(1);
        // Begin Loki spawn animation (takes 1 second)
        Instantiate(tesseractPrefab, lokiSpawnPoint);
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
    }

    public void AddLokiRef(Loki _loki)
    {
        loki = _loki;
    }

    private void RandomiseLogo()
    {

    }

    private IEnumerator CaptureLoki(/*last enemy reference?*/)
    {
        yield return new WaitForSeconds(0);
        // Destroy last remaining enemy
    }

    public void RemoveLokiRef()
    {
        loki = null;
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

    public void EndRound()
    {
        StartCoroutine(EndRoundCoroutine());
    }

    private IEnumerator EndRoundCoroutine()
    {
        FreezeGameAndInputs();
        // Destroy enemies (except the one who caught Loki)
        yield return new WaitForSeconds(1);
        StartCoroutine(CaptureLoki(/*last enemy reference?*/));
        yield return new WaitUntil(() => true);
        if (lives > 0)
        {
            gameState = GameState.BeginScreen;
            beginGameText.EnableText();
            lives--;
            em.ResetEnemyRoamLoop();
        }
        else
        {
            yield return new WaitForSeconds(2);
            score = 0;
            lives = defaultLives;
            Destroy(instantiatedPellets);
            instantiatedPellets = Instantiate(pelletsPrefab);
            // remove any bonus items
            gameState = GameState.BeginScreen;
            beginGameText.EnableText();
        }
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
