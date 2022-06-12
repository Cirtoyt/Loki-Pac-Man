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

    private GameManager gm;
    private EnemyManager em;
    private GridMovement gridMovement;
    private Animator anim;
    private Direction queuedDirection;
    private Coroutine currentWakaWakaCoroutine;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        em = FindObjectOfType<EnemyManager>();
        gridMovement = GetComponent<GridMovement>();
        anim = GetComponent<Animator>();
        DirectionInfo enterMazeDirection = new DirectionInfo { enumVal = Direction.Left, vecVal = Vector2.left };
        gridMovement.SetSpawnPosition(enterMazeDirection, transform.position, true);
        queuedDirection = Direction.None;
        wakawakaSource.volume = 0;
    }

    private void Update()
    {
        if (gridMovement.GetCurrentDirectionInfo().vecVal.magnitude > 0.1f)
        {
            anim.SetFloat("walkingRight", gridMovement.GetCurrentDirectionInfo().vecVal.x);
            anim.SetFloat("walkingUp", gridMovement.GetCurrentDirectionInfo().vecVal.y);
        }
    }

    public void CompareNewInput(Direction inputDirection)
    {
        // Queue new input direction or instantly enact it (when reversing)
        switch (inputDirection)
        {
            case Direction.Up:
                {
                    if (gridMovement.GetCurrentDirectionInfo().enumVal == Direction.Down)
                        gridMovement.InstantReverseDirection();
                    else
                        queuedDirection = inputDirection;
                }
                break;
            case Direction.Down:
                {
                    if (gridMovement.GetCurrentDirectionInfo().enumVal == Direction.Up)
                        gridMovement.InstantReverseDirection();
                    else
                        queuedDirection = inputDirection;
                }
                break;
            case Direction.Left:
                {
                    if (gridMovement.GetCurrentDirectionInfo().enumVal == Direction.Right)
                        gridMovement.InstantReverseDirection();
                    else
                        queuedDirection = inputDirection;
                }
                break;
            case Direction.Right:
                {
                    if (gridMovement.GetCurrentDirectionInfo().enumVal == Direction.Left)
                        gridMovement.InstantReverseDirection();
                    else
                        queuedDirection = inputDirection;
                }
                break;
        }
    }

    public void ProcessDirectionChange(List<DirectionInfo> possibleDirections)
    {
        bool possibleDirectionFound = false;

        // If nothing is queued,
        if (queuedDirection == Direction.None)
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
        else
        {
            // Otherwise, Set new direction as queued direction if it's possible
            for (int i = 0; i < possibleDirections.Count; i++)
            {
                if (queuedDirection == possibleDirections[i].enumVal)
                {
                    gridMovement.SetNextTile(possibleDirections[i]);
                    possibleDirectionFound = true;

                    // Reset queued direction
                    queuedDirection = Direction.None;
                    break;
                }
            }
        }
        if (!possibleDirectionFound)
        {
            // If no possible direction is found, try to keep going straight
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
        gm.FreezeGameButNoPauseText();
        yield return new WaitForSeconds(0.6f);
        gm.UnfreezeGameButNoPauseText();
        enemy.RetreatToTVA();
        em.CaptureEnemy();
    }
}
