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
    [Header("Debug")]
    [SerializeField] private List<Enemy> enemies;

    private GameManager gm;
    private bool enemySpawnLoop;
    private bool enemyRoamLoop;
    private float enemySpawnLoopTimer;
    private float enemyRoamLoopTimer;
    private int frightenedPointTally;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        enemySpawnLoop = false;
        enemyRoamLoop = false;
        enemySpawnLoopTimer = 0;
        enemyRoamLoopTimer = 0;
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
        // enemies[0].Set texture
        enemies[0].GetComponent<GridMovement>().SetSpawnPosition(enemySpawnPoints[0].position);
        enemies[0].SetHomeLocation(Enemy.HomeLocation.Centre);

        enemies[1].personality = Enemy.Personality.Speedy;
        // enemies[1].Set texture
        enemies[1].GetComponent<GridMovement>().SetSpawnPosition(enemySpawnPoints[1].position);
        enemies[1].SetHomeLocation(Enemy.HomeLocation.Right);

        enemies[2].personality = Enemy.Personality.Bashful;
        // enemies[2].Set texture
        enemies[2].GetComponent<GridMovement>().SetSpawnPosition(enemySpawnPoints[2].position);
        enemies[2].SetHomeLocation(Enemy.HomeLocation.Centre);

        enemies[3].personality = Enemy.Personality.Pokey;
        // enemies[3].Set texture
        enemies[3].GetComponent<GridMovement>().SetSpawnPosition(enemySpawnPoints[3].position);
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
        yield return new WaitUntil(() => enemyRoamLoopTimer > 7);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToChase();
        Debug.Log("Switched to chase mode");
        yield return new WaitUntil(() => enemyRoamLoopTimer > 27);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToScatter();
        Debug.Log("Switched to scatter mode");
        yield return new WaitUntil(() => enemyRoamLoopTimer > 34);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToChase();
        Debug.Log("Switched to chase mode");
        yield return new WaitUntil(() => enemyRoamLoopTimer > 54);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToScatter();
        Debug.Log("Switched to scatter mode");
        yield return new WaitUntil(() => enemyRoamLoopTimer > 59);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToChase();
        Debug.Log("Switched to chase mode");
        yield return new WaitUntil(() => enemyRoamLoopTimer > 79);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToScatter();
        Debug.Log("Switched to scatter mode");
        yield return new WaitUntil(() => enemyRoamLoopTimer > 84);
        foreach (Enemy enemy in enemies)
            enemy.SwitchToChase();
        Debug.Log("Switched to chase mode");
    }

    public void FrightenEnemies()
    {
        ResetEnemyRoamLoop();
        foreach (Enemy enemy in enemies)
            enemy.Frighten();
        StartCoroutine(FrightenLoop());
    }

    public void CaptureEnemy()
    {
        frightenedPointTally += catchEnemyPointWorth;
        gm.AddScore(catchEnemyPointWorth);
    }

    private IEnumerator FrightenLoop()
    {
        yield return new WaitForSeconds(7.1f);
        foreach (Enemy enemy in enemies)
            enemy.SetSpriteAsFrightened(false);
        yield return new WaitForSeconds(0.4f);
        foreach (Enemy enemy in enemies)
            enemy.SetSpriteAsFrightened(true);
        yield return new WaitForSeconds(0.4f);
        foreach (Enemy enemy in enemies)
            enemy.SetSpriteAsFrightened(false);
        yield return new WaitForSeconds(0.4f);
        foreach (Enemy enemy in enemies)
            enemy.SetSpriteAsFrightened(true);
        yield return new WaitForSeconds(0.4f);
        foreach (Enemy enemy in enemies)
            enemy.SetSpriteAsFrightened(false);
        yield return new WaitForSeconds(0.4f);
        foreach (Enemy enemy in enemies)
            enemy.SetSpriteAsFrightened(true);
        yield return new WaitForSeconds(0.4f);
        foreach (Enemy enemy in enemies)
        {
            if (enemy.GetMovementState() == Enemy.MovementState.Frightened)
                enemy.EndFrighten();
        }
        enemyRoamLoop = true;
        frightenedPointTally = 0;
    }

    public void ResetEnemyRoamLoop()
    {
        enemyRoamLoop = false;
        enemyRoamLoopTimer = 0;
        StopCoroutine(EnemyRoamLoop());
        StartCoroutine(EnemyRoamLoop());
    }

    public void FreezeEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            enemy.GetComponent<GridMovement>().canMove = false;
        }
    }

    public void UnfreezeEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            enemy.GetComponent<GridMovement>().canMove = true;
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
}
