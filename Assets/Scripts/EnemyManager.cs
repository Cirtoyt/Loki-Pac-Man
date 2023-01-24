using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    
    [SerializeField] GameObject enemyPrefab;
    public List<Transform> enemySpawnPoints;
    [SerializeField] private float inTVASpeed;
    public List<Transform> enemyCornerTargets;
    [SerializeField] private int catchEnemyPointWorth;
    [SerializeField] private RuntimeAnimatorController enemy1Controller;
    [SerializeField] private RuntimeAnimatorController enemy2Controller;
    [SerializeField] private RuntimeAnimatorController enemy3Controller;
    [SerializeField] private RuntimeAnimatorController enemy4Controller;
    [Header("Debug")]
    [SerializeField] private List<Enemy> enemies;

    private GameManager gm;
    private bool enemySpawnLoop;
    private bool enemyRoamLoop;
    private float enemySpawnLoopTimer;
    private float enemyRoamLoopTimer;
    private Coroutine currentFrightenLoopCoroutine;
    [SerializeField] private float frightenLoopDelayTimer;
    private int frightenedPointTally;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        enemySpawnLoop = false;
        enemyRoamLoop = false;
        enemySpawnLoopTimer = 0;
        enemyRoamLoopTimer = 0;
        currentFrightenLoopCoroutine = null;
        frightenLoopDelayTimer = 0;
        frightenedPointTally = 0;
    }

    void Update()
    {
        if (enemySpawnLoop && gm.GetGameState() != GameManager.GameState.PauseScreen)
            enemySpawnLoopTimer += Time.deltaTime;
        if (enemyRoamLoop && gm.GetGameState() != GameManager.GameState.PauseScreen)
            enemyRoamLoopTimer += Time.deltaTime;
    }

    public void SpawnEnemies()
    {
        if (enemies.Count > 0)
        {
            Debug.LogWarning("Enemies failed to be destroyed before new are spawned.");
        }

        for (int i = 0; i < 4; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            enemies.Add(enemy.GetComponent<Enemy>());
        }

        enemies[0].personality = Enemy.Personality.Shadow;
        enemies[0].GetComponent<Animator>().runtimeAnimatorController = enemy1Controller;
        enemies[0].transform.position = enemySpawnPoints[0].position;
        enemies[0].SetHomeLocation(Enemy.HomeLocation.Centre);

        enemies[1].personality = Enemy.Personality.Speedy;
        enemies[1].GetComponent<Animator>().runtimeAnimatorController = enemy2Controller;
        enemies[1].transform.position = enemySpawnPoints[1].position;
        enemies[1].SetHomeLocation(Enemy.HomeLocation.Right);

        enemies[2].personality = Enemy.Personality.Bashful;
        enemies[2].GetComponent<Animator>().runtimeAnimatorController = enemy3Controller;
        enemies[2].transform.position = enemySpawnPoints[2].position;
        enemies[2].SetHomeLocation(Enemy.HomeLocation.Centre);

        enemies[3].personality = Enemy.Personality.Pokey;
        enemies[3].GetComponent<Animator>().runtimeAnimatorController = enemy4Controller;
        enemies[3].transform.position = enemySpawnPoints[3].position;
        enemies[3].SetHomeLocation(Enemy.HomeLocation.Left);
    }

    public void BeginRound()
    {
        enemies[0].SetupOutsideTVA();
        enemies[1].SetupInTVA(new DirectionInfo { enumVal = Direction.Up, vecVal = Vector2.up });
        enemies[2].SetupInTVA(new DirectionInfo { enumVal = Direction.Down, vecVal = Vector2.down });
        enemies[3].SetupInTVA(new DirectionInfo { enumVal = Direction.Up, vecVal = Vector2.up });
        enemySpawnLoop = true;
        enemyRoamLoop = true;
        StartCoroutine(EnemySpawnLoop());
        StartCoroutine(EnemyRoamLoop());
    }

    private IEnumerator EnemySpawnLoop()
    {
        yield return new WaitUntil(() => enemySpawnLoopTimer > 4);
        enemies[1].LeaveTVA(enemySpawnPoints[1].position);
        yield return new WaitUntil(() => enemySpawnLoopTimer > 10);
        enemies[2].LeaveTVA(enemySpawnPoints[2].position);
        yield return new WaitUntil(() => enemySpawnLoopTimer > 20);
        enemies[3].LeaveTVA(enemySpawnPoints[3].position);
    }

    private IEnumerator EnemyRoamLoop()
    {
        foreach (Enemy enemy in enemies)
            enemy.SwitchToScatter();
        //Debug.Log("Switched to scatter mode");
        yield return new WaitUntil(() => enemyRoamLoopTimer > 7);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToChase();
        //Debug.Log("Switched to chase mode");
        yield return new WaitUntil(() => enemyRoamLoopTimer > 27);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToScatter();
        //Debug.Log("Switched to scatter mode");
        yield return new WaitUntil(() => enemyRoamLoopTimer > 34);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToChase();
        //Debug.Log("Switched to chase mode");
        yield return new WaitUntil(() => enemyRoamLoopTimer > 54);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToScatter();
        //Debug.Log("Switched to scatter mode");
        yield return new WaitUntil(() => enemyRoamLoopTimer > 61);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToChase();
        //Debug.Log("Switched to chase mode");
        yield return new WaitUntil(() => enemyRoamLoopTimer > 81);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToScatter();
        //.Log("Switched to scatter mode");
        yield return new WaitUntil(() => enemyRoamLoopTimer > 88);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToChase();
        //Debug.Log("Switched to chase mode");
    }

    public void FrightenEnemies()
    {
        ResetEnemyRoamLoop();
        foreach (Enemy enemy in enemies)
            enemy.Frighten();
        if (currentFrightenLoopCoroutine != null) StopCoroutine(currentFrightenLoopCoroutine);
        currentFrightenLoopCoroutine = StartCoroutine(FrightenLoop());
    }

    public void CaptureEnemy()
    {
        if (frightenedPointTally == 0)
            frightenedPointTally = catchEnemyPointWorth;
        else
            frightenedPointTally *= 2;

        gm.AddScore(frightenedPointTally);
    }

    private IEnumerator FrightenLoop()
    {
        frightenedPointTally = 0;
        frightenLoopDelayTimer = 0;
        while (frightenLoopDelayTimer < 7.1f)
        {
            while (gm.GetGameState() == GameManager.GameState.PauseScreen) yield return null;
            frightenLoopDelayTimer += Time.deltaTime; yield return null;
        }
        foreach (Enemy enemy in enemies)
        {
            enemy.SetSpriteAsFrightened(false);
        }

        frightenLoopDelayTimer = 0;
        while (frightenLoopDelayTimer < 0.4f)
        {
            while (gm.GetGameState() == GameManager.GameState.PauseScreen) yield return null;
            frightenLoopDelayTimer += Time.deltaTime; yield return null;
        }
        foreach (Enemy enemy in enemies)
        {
            enemy.SetSpriteAsFrightened(true);
        }

        frightenLoopDelayTimer = 0;
        while (frightenLoopDelayTimer < 0.4f)
        {
            while (gm.GetGameState() == GameManager.GameState.PauseScreen) yield return null;
            frightenLoopDelayTimer += Time.deltaTime; yield return null;
        }
        foreach (Enemy enemy in enemies)
        {
            enemy.SetSpriteAsFrightened(false);
        }

        frightenLoopDelayTimer = 0;
        while (frightenLoopDelayTimer < 0.4f)
        {
            while (gm.GetGameState() == GameManager.GameState.PauseScreen) yield return null;
            frightenLoopDelayTimer += Time.deltaTime; yield return null;
        }
        foreach (Enemy enemy in enemies)
        {
            enemy.SetSpriteAsFrightened(true);
        }

        frightenLoopDelayTimer = 0;
        while (frightenLoopDelayTimer < 0.4f)
        {
            while (gm.GetGameState() == GameManager.GameState.PauseScreen) yield return null;
            frightenLoopDelayTimer += Time.deltaTime; yield return null;
        }
        foreach (Enemy enemy in enemies)
        {
            enemy.SetSpriteAsFrightened(false);
        }

        frightenLoopDelayTimer = 0;
        while (frightenLoopDelayTimer < 0.4f)
        {
            while (gm.GetGameState() == GameManager.GameState.PauseScreen) yield return null;
            frightenLoopDelayTimer += Time.deltaTime; yield return null;
        }
        foreach (Enemy enemy in enemies)
        {
            enemy.SetSpriteAsFrightened(true);
        }

        frightenLoopDelayTimer = 0;
        while (frightenLoopDelayTimer < 0.4f)
        {
            while (gm.GetGameState() == GameManager.GameState.PauseScreen) yield return null;
            frightenLoopDelayTimer += Time.deltaTime; yield return null;
        }
        foreach (Enemy enemy in enemies)
        {
            enemy.EndFrighten();
        }
        enemyRoamLoop = true;
        currentFrightenLoopCoroutine = null;
    }

    public void ResetEnemyRoamLoop()
    {
        enemyRoamLoop = false;
        enemyRoamLoopTimer = 0;
        StopCoroutine(EnemyRoamLoop());
        StartCoroutine(EnemyRoamLoop());
    }

    public void StopAllLoops()
    {
        enemyRoamLoop = false;
        enemySpawnLoop = false;
        enemyRoamLoopTimer = 0;
        enemySpawnLoopTimer = 0;
        StopAllCoroutines();
    }

    public void FreezeEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy.GetMovementState() != Enemy.MovementState.InTVA && enemy.GetMovementState() != Enemy.MovementState.LeavingTVA)
            {
                enemy.GetComponent<GridMovement>().canMove = false;
            }
            else
            {
                enemy.canMoveInTVA = false;
            }
        }
    }

    public void UnfreezeEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy.GetMovementState() != Enemy.MovementState.InTVA && enemy.GetMovementState() != Enemy.MovementState.LeavingTVA)
            {
                enemy.GetComponent<GridMovement>().canMove = true;
            }
            else
            {
                enemy.canMoveInTVA = true;
            }
        }
    }

    public float GetInTVASpeed()
    {
        return inTVASpeed;
    }

    public Vector2 GetShadowPosition()
    {
        return enemies[3].transform.position;
    }

    public void DestroyAllEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
        enemies.Clear();
    }

    public void DestroyEnemiesBarOne(Enemy excludedEnemy)
    {
        for (var i = enemies.Count - 1; i > -1; i--)
        {
            if (enemies[i] != excludedEnemy)
            {
                Destroy(enemies[i].gameObject);
                enemies.RemoveAt(i);
            }
        }
    }

    public void DestroyEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
        Destroy(enemy.gameObject);
    }
}
