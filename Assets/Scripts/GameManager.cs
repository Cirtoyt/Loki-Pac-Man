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
    [SerializeField] private GameObject tesseractPrefab;
    [SerializeField] private GameObject lokiPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] private int defaultLives;
    [SerializeField] private int maxLives;

    private enum GameStates
    {
        BeginScreen,
        SettingUpGame,
        InGame,
        PauseScreen,
    }

    private GameStates gameState;
    private Loki loki;
    private int lives;
    private int score;

    void Start()
    {
        gameState = GameStates.BeginScreen;
        loki = null;
        lives = defaultLives;

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
            case GameStates.BeginScreen:
                StartCoroutine(BeginGame());
                break;
            case GameStates.InGame:
                PauseGame();
                break;
            case GameStates.PauseScreen:
                UnPauseGame();
                break;
        }
    }

    private IEnumerator BeginGame()
    {
        gameState = GameStates.SettingUpGame;
        beginGameText.DisableText();
        ReadyText.enabled = true;
        RandomiseLogo();
        yield return new WaitForSeconds(1);
        RandomiseLogo();
        yield return new WaitForSeconds(1);
        RandomiseLogo();
        yield return new WaitForSeconds(1);
        // Begin Loki spawn animation (takes 1 second)
        Instantiate(tesseractPrefab, spawnPoint);
        yield return new WaitUntil(() => loki);
        // Spawn enemies
        yield return new WaitForSeconds(1);
        // Game begins!
        ReadyText.enabled = false;
        loki.Unfreeze();
        gameState = GameStates.InGame;
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
        gameState = GameStates.PauseScreen;
        pauseGameText.EnableText();
        FreezeGameAndInputs();
    }

    private void FreezeGameAndInputs()
    {
        loki.Freeze();
        // Get all enemies in existance, then freeze them
    }

    private void UnPauseGame()
    {
        gameState = GameStates.InGame;
        pauseGameText.DisableText();
        UnfreezeGameAndInputs();
    }

    private void UnfreezeGameAndInputs()
    {
        loki.Unfreeze();
        // Get all enemies in existance, then unfreeze them
    }

    public IEnumerator EndRound()
    {
        yield return new WaitForSeconds(1);
        FreezeGameAndInputs();
        // Destroy enemies (except the one who caught Loki)
        StartCoroutine(CaptureLoki(/*last enemy reference?*/));
        yield return new WaitUntil(() => !loki);
        if (lives > 0)
        {
            gameState = GameStates.BeginScreen;
            beginGameText.EnableText();
        }
        else
        {
            yield return new WaitForSeconds(2);
            score = 0;
            lives = defaultLives;
            // remove any bonus items
            gameState = GameStates.BeginScreen;
            beginGameText.EnableText();
        }
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
        return spawnPoint;
    }

    public GameObject GetLokiPrefab()
    {
        return lokiPrefab;
    }

    public int GetScore()
    {
        return score;
    }
}
