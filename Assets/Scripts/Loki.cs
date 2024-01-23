using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Loki : MonoBehaviour
{
    [SerializeField] private AudioSource wakawakaSource;
    [SerializeField] private AudioSource deathSource;
    [SerializeField] private AudioSource enemyCaptureSource;
    [SerializeField] private AudioSource pickupTesseractSource;
    [SerializeField] private float level1SpeedMultiplier = 0.8f;
    [SerializeField] private float level2To4SpeedMultiplier = 0.9f;
    [SerializeField] private float level5To20SpeedMultiplier = 1f;
    [SerializeField] private float level21PlusSpeedMultiplier = 0.9f;
    [SerializeField] private float level1FrightSpeedMultiplierOffset = 0.1f;
    [SerializeField] private float level2To4FrightSpeedMultiplierOffset = 0.05f;

    private GameManager gm;
    private EnemyManager em;
    private GridMovement gridMovement;
    private Animator anim;
    private MuteSoundsToggle muteSoundsToggle;
    private Coroutine currentWakaWakaCoroutine;
    private int inputQueueLength = 2;
    private float frightenedDuration = 9.5f;
    private float frightenedTimer = 0;
    private bool frightened = false;
    [Header("Debug")]
    private List<Direction> queuedDirections = new List<Direction>();

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        em = FindObjectOfType<EnemyManager>();
        gridMovement = GetComponent<GridMovement>();
        anim = GetComponent<Animator>();
        muteSoundsToggle = FindObjectOfType<MuteSoundsToggle>();

        muteSoundsToggle.AddAudioSource(wakawakaSource);
        muteSoundsToggle.AddAudioSource(deathSource);
        muteSoundsToggle.AddAudioSource(enemyCaptureSource);
        muteSoundsToggle.AddAudioSource(pickupTesseractSource);
    }

    void Start()
    {
        DirectionInfo enterMazeDirection = new DirectionInfo { enumVal = Direction.Left, vecVal = Vector2.left };
        gridMovement.SetSpawnPosition(enterMazeDirection, transform.position, true);
        wakawakaSource.volume = 0;
    }

    private void Update()
    {
        if (gridMovement.GetCurrentDirectionInfo().vecVal.magnitude > 0.1f)
        {
            anim.SetFloat("walkingRight", gridMovement.GetCurrentDirectionInfo().vecVal.x);
            anim.SetFloat("walkingUp", gridMovement.GetCurrentDirectionInfo().vecVal.y);
        }

        if (frightened)
        {
            if (gm.GetGameState() == GameManager.GameState.PauseScreen) return;

            frightenedTimer += Time.deltaTime;
            if (frightenedTimer >= frightenedDuration)
            {
                frightened = false;
                UpdateLevelSpeedMultiplier(gm.level);
            }
        }
    }

    public void RegisterInput(Direction inputDirection)
    {
        // Queue new input direction or instantly enact it (when reversing)
        switch (inputDirection)
        {
            case Direction.Up:
                {
                    RegisterInputPerDirection(inputDirection, Direction.Down);
                }
                break;
            case Direction.Down:
                {
                    RegisterInputPerDirection(inputDirection, Direction.Up);
                }
                break;
            case Direction.Left:
                {
                    RegisterInputPerDirection(inputDirection, Direction.Right);
                }
                break;
            case Direction.Right:
                {
                    RegisterInputPerDirection(inputDirection, Direction.Left);
                }
                break;
        }
    }

    private void RegisterInputPerDirection(Direction inputDirection, Direction oppositeDirection)
    {
        if (gridMovement.GetCurrentDirectionInfo().enumVal == oppositeDirection)
        {
            gridMovement.InstantReverseDirection();
            queuedDirections.Clear();
        }
        else
        {
            if (queuedDirections.Count == inputQueueLength)
            {
                queuedDirections.RemoveAt(0);
            }
            queuedDirections.Add(inputDirection);

            //string debugString = "Queued Directions now: ";
            //foreach (var direction in queuedDirections)
            //{
            //    debugString += direction.ToString() + ", ";
            //}
            //Debug.Log(debugString);
        }
    }

    public void ProcessDirectionChange(List<DirectionInfo> possibleDirections)
    {
        bool possibleDirectionFound = false;

        // If nothing is queued,
        if (queuedDirections.Count == 0)
        {
            // Set new direction as the current as long as it's possible
            for (int i = 0; i < possibleDirections.Count; i++)
            {
                if (gridMovement.GetCurrentDirectionInfo().enumVal == possibleDirections[i].enumVal)
                {
                    gridMovement.SetNextTile(possibleDirections[i]);
                    possibleDirectionFound = true;
                    break;
                }
            }
        }
        else // Otherwise, 
        {
            // Set new direction as queued direction if it's possible
            bool firstQueuedDirIsPossible = false;
            bool secondQueuedDirIsPossible = false;
            DirectionInfo firstQueuedDirPossibleDirInfo = new DirectionInfo();
            DirectionInfo secondQueuedDirPossibleDirInfo = new DirectionInfo();
            for (int q = 0; q < queuedDirections.Count; q++)
            {
                for (int p = 0; p < possibleDirections.Count; p++)
                {
                    if (queuedDirections[q] == possibleDirections[p].enumVal)
                    {
                        if (q == 0)
                        {
                            firstQueuedDirPossibleDirInfo = possibleDirections[p];
                            firstQueuedDirIsPossible = true;
                        }
                        if (q == 1)
                        {
                            secondQueuedDirPossibleDirInfo = possibleDirections[p];
                            secondQueuedDirIsPossible = true;
                        }
                    }
                }
            }

            // If first queued direction is possible as well as the second, or only the second is possible, use the second input and clear the queue
            if ((firstQueuedDirIsPossible && secondQueuedDirIsPossible) || (!firstQueuedDirIsPossible && secondQueuedDirIsPossible))
            {
                queuedDirections.Clear();
                gridMovement.SetNextTile(secondQueuedDirPossibleDirInfo);
                possibleDirectionFound = true;
            }
            // Else if only first queued direction is possible (or there is only a first input), remove first input from the list and let the second queued direction become the first (if there is a second)
            else if (firstQueuedDirIsPossible && !secondQueuedDirIsPossible)
            {
                queuedDirections.RemoveAt(0);
                gridMovement.SetNextTile(firstQueuedDirPossibleDirInfo);
                possibleDirectionFound = true;
            }
        }

        // If no possible direction is found, try to keep going straight
        if (!possibleDirectionFound)
        {
            bool couldGoStraight = false;
            for (int i = 0; i < possibleDirections.Count; i++)
            {
                if (gridMovement.GetCurrentDirectionInfo().enumVal == possibleDirections[i].enumVal)
                {
                    gridMovement.SetNextTile(possibleDirections[i]);
                    couldGoStraight = true;
                    break;
                }
            }
            // And if all else fails, you're hitting a wall and you will be stopped
            if (!couldGoStraight)
            {
                gridMovement.SetNextTile(new DirectionInfo { enumVal = Direction.None, vecVal = Vector2.zero });
            }
        }
    }

    public void Freeze()
    {
        gridMovement.canMove = false;
    }

    public void Unfreeze()
    {
        gridMovement.canMove = true;
    }

    public void PlayWakaWakaSound()
    {
        if (currentWakaWakaCoroutine != null)
        {
            StopCoroutine(currentWakaWakaCoroutine);
        }
        currentWakaWakaCoroutine = StartCoroutine(PlayWakaWakaSoundCoroutine());
    }

    private IEnumerator PlayWakaWakaSoundCoroutine()
    {
        wakawakaSource.volume = 0.6f;
        yield return new WaitForSeconds(0.3f);
        wakawakaSource.volume = 0;
    }

    public void PlayDeathSound()
    {
        deathSource.Play();
    }

    public void PlayerPickupTesseractSound()
    {
        pickupTesseractSource.Play();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Enemy enemy))
        {
            if (enemy.GetMovementState() == Enemy.MovementState.Frightened)
            {
                StartCoroutine(CaptureEnemy(enemy));
            }
        }
    }

    private IEnumerator CaptureEnemy(Enemy enemy)
    {
        enemyCaptureSource.Play();
        em.RecordCaptureEnemyPoints(enemy);
        gm.FreezeGameButNoPauseText();
        yield return new WaitForSeconds(0.6f);
        gm.UnfreezeGameButNoPauseText();
        enemy.RetreatToTVA();
    }

    public void UpdateLevelSpeedMultiplier(int newLevel)
    {
        if (newLevel == 1)
        {
            gridMovement.movementSpeedLevelMultiplier = level1SpeedMultiplier;
            if (frightened) gridMovement.movementSpeedLevelMultiplier += level1FrightSpeedMultiplierOffset;
        }
        else if (newLevel >= 2 && newLevel <= 4)
        {
            gridMovement.movementSpeedLevelMultiplier = level2To4SpeedMultiplier;
            if (frightened) gridMovement.movementSpeedLevelMultiplier += level2To4FrightSpeedMultiplierOffset;
        }
        else if (newLevel >= 5 && newLevel <= 20)
        {
            gridMovement.movementSpeedLevelMultiplier = level5To20SpeedMultiplier;
        }
        else if (newLevel >= 21)
        {
            gridMovement.movementSpeedLevelMultiplier = level21PlusSpeedMultiplier;
        }
    }

    public void Frighten()
    {
        frightened = true;
        frightenedTimer = 0;
        UpdateLevelSpeedMultiplier(gm.level);
    }

    private void OnDestroy()
    {
        muteSoundsToggle.RemoveAudioSource(wakawakaSource);
        muteSoundsToggle.RemoveAudioSource(deathSource);
        muteSoundsToggle.RemoveAudioSource(enemyCaptureSource);
        muteSoundsToggle.RemoveAudioSource(pickupTesseractSource);
    }
}
