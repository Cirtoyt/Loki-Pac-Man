using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Loki : MonoBehaviour
{
    private EnemyManager em;
    private GridMovement gridMovement;
    private Direction queuedDirection;

    void Start()
    {
        em = FindObjectOfType<EnemyManager>();
        gridMovement = GetComponent<GridMovement>();
        gridMovement.SetSpawnPosition(transform.position);
        queuedDirection = Direction.None;
    }

    void Update()
    {
        // movement stuff
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

        // Reset queued direction
        queuedDirection = Direction.None;
    }

    public void Freeze()
    {
        gridMovement.canMove = false;
    }

    public void Unfreeze()
    {
        gridMovement.canMove = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Enemy enemy))
        {
            if (enemy.GetMovementState() == Enemy.MovementState.Frightened)
            {
                Debug.Log("Hit");
                enemy.RetreatToTVA();
                em.CaptureEnemy();
            }
        }
    }
}
