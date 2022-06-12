using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    public float movementSpeedMultiplier;

    [HideInInspector] public bool canMove;

    private Tilemap wallTilemap;
    private Tilemap LTilemap;
    private Tilemap KTilemap;
    private Tilemap ITilemap;
    private Tilemap decisionPointTilemap;
    private Tilemap uTurnPointTilemap;
    private Tilemap tunnelTilemap;
    private Vector2 previousTilePos;
    private bool mustResetHalfStep;
    private DirectionInfo currentDirection;
    private Vector2 nextTilePos;
    private float tileTransitionPerc = 0;

    void Awake()
    {
        canMove = false;
        wallTilemap = GameObject.FindGameObjectWithTag("Wall").GetComponent<Tilemap>();
        LTilemap = GameObject.FindGameObjectWithTag("L Wall").GetComponent<Tilemap>();
        KTilemap = GameObject.FindGameObjectWithTag("K Wall").GetComponent<Tilemap>();
        ITilemap = GameObject.FindGameObjectWithTag("I Wall").GetComponent<Tilemap>();
        decisionPointTilemap = GameObject.FindGameObjectWithTag("Decision Point").GetComponent<Tilemap>();
        uTurnPointTilemap = GameObject.FindGameObjectWithTag("U-Turn Point").GetComponent<Tilemap>();
        tunnelTilemap = GameObject.FindGameObjectWithTag("Tunnel").GetComponent<Tilemap>();
    }

    public void SetSpawnPosition(DirectionInfo directionInfo, Vector2 position, bool requiresHalfStep)
    {
        currentDirection = directionInfo;
        previousTilePos = transform.position;
        nextTilePos = previousTilePos + currentDirection.vecVal;
        if (requiresHalfStep)
            PerformHalfStep();
    }

    public void PerformHalfStep()
    {
        currentDirection.vecVal *= 0.5f;
        nextTilePos = previousTilePos + currentDirection.vecVal;
        mustResetHalfStep = true;
    }

    void Update()
    {
        if (canMove)
        {
            // Increase between tile transition percentage
            if (mustResetHalfStep)
            {
                tileTransitionPerc += Time.deltaTime * movementSpeed * movementSpeedMultiplier * 2;
            }
            else
            {
                tileTransitionPerc += Time.deltaTime * movementSpeed * movementSpeedMultiplier;
            }

            // If still transitioning,
            if (tileTransitionPerc < 1)
            {
                // Keep moving towards next tile
                transform.position = Vector2.Lerp(previousTilePos, nextTilePos, tileTransitionPerc);
            }
            // Else, upon reaching new tile check if change of direction is required
            else
            {
                if (mustResetHalfStep)
                {
                    currentDirection.vecVal *= 2;
                    mustResetHalfStep = false;
                }
                tileTransitionPerc = 0;
                transform.position = nextTilePos;
                Vector3Int gridPos = decisionPointTilemap.WorldToCell(transform.position);
                if (decisionPointTilemap.HasTile(gridPos))
                {
                    ChangeDirection(false);
                }
                else if (uTurnPointTilemap.HasTile(gridPos))
                {
                    ChangeDirection(true);
                }
                else if (tunnelTilemap.HasTile(gridPos))
                {
                    // Teleport to other side of screen
                    transform.position = new Vector3(-gridPos.x - 1, 0);
                    previousTilePos = transform.position;
                    nextTilePos = transform.position += (Vector3)currentDirection.vecVal;
                }
                else
                {
                    previousTilePos = nextTilePos;
                    nextTilePos = nextTilePos += currentDirection.vecVal;
                }
            }
        }
    }

    private void ChangeDirection(bool isUTurn)
    {
        if (TryGetComponent(out Loki loki))
        {
            List<DirectionInfo> possibleDirections = GetDirectionOptions(true);
            loki.ProcessDirectionChange(possibleDirections);
        }
        else if (TryGetComponent(out Enemy enemy))
        {
            List<DirectionInfo> possibleDirections;
            if (isUTurn)
            {
                possibleDirections = GetDirectionOptions(true);
            }
            else
            {
                possibleDirections = GetDirectionOptions(false);
            }
            
            enemy.ProcessDirectionChange(possibleDirections);
        }
    }

    private List<DirectionInfo> GetDirectionOptions(bool canReverse)
    {
        List<DirectionInfo> possibleDirections = new List<DirectionInfo>();

        // Up check
        Vector3Int upRightGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.up * 1.5f) + (Vector3.right / 2));
        Vector3Int upLeftGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.up * 1.5f) + (Vector3.left / 2));
        if (!wallTilemap.HasTile(upRightGridPos) && !wallTilemap.HasTile(upLeftGridPos)
            && !LTilemap.HasTile(upRightGridPos) && !LTilemap.HasTile(upLeftGridPos)
            && !KTilemap.HasTile(upRightGridPos) && !KTilemap.HasTile(upLeftGridPos)
            && !ITilemap.HasTile(upRightGridPos) && !ITilemap.HasTile(upLeftGridPos)
            && (currentDirection.enumVal != Direction.Down || canReverse))
        {
            possibleDirections.Add(new DirectionInfo { enumVal = Direction.Up, vecVal = Vector2.up });
        }

        // Down check
        Vector3Int downRightGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.down * 1.5f) + (Vector3.right / 2));
        Vector3Int downLeftGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.down * 1.5f) + (Vector3.left / 2));
        if (!wallTilemap.HasTile(downRightGridPos) && !wallTilemap.HasTile(downLeftGridPos)
            && !LTilemap.HasTile(downRightGridPos) && !LTilemap.HasTile(downLeftGridPos)
            && !KTilemap.HasTile(downRightGridPos) && !KTilemap.HasTile(downLeftGridPos)
            && !ITilemap.HasTile(downRightGridPos) && !ITilemap.HasTile(downLeftGridPos)
            && (currentDirection.enumVal != Direction.Up || canReverse))
        {
            possibleDirections.Add(new DirectionInfo { enumVal = Direction.Down, vecVal = Vector2.down });
        }

        // Left check
        Vector3Int leftUpGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.left * 1.5f) + (Vector3.up / 2));
        Vector3Int leftDownGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.left * 1.5f) + (Vector3.down / 2));
        if (!wallTilemap.HasTile(leftUpGridPos) && !wallTilemap.HasTile(leftDownGridPos)
            && !LTilemap.HasTile(leftUpGridPos) && !LTilemap.HasTile(leftDownGridPos)
            && !KTilemap.HasTile(leftUpGridPos) && !KTilemap.HasTile(leftDownGridPos)
            && !ITilemap.HasTile(leftUpGridPos) && !ITilemap.HasTile(leftDownGridPos)
            && (currentDirection.enumVal != Direction.Right || canReverse))
        {
            possibleDirections.Add(new DirectionInfo { enumVal = Direction.Left, vecVal = Vector2.left });
        }

        // Right check
        Vector3Int rightUpGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.right * 1.5f) + (Vector3.up / 2));
        Vector3Int rightDownGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.right * 1.5f) + (Vector3.down / 2));
        if (!wallTilemap.HasTile(rightUpGridPos) && !wallTilemap.HasTile(rightDownGridPos)
            && !LTilemap.HasTile(rightUpGridPos) && !LTilemap.HasTile(rightDownGridPos)
            && !KTilemap.HasTile(rightUpGridPos) && !KTilemap.HasTile(rightDownGridPos)
            && !ITilemap.HasTile(rightUpGridPos) && !ITilemap.HasTile(rightDownGridPos)
            && (currentDirection.enumVal != Direction.Left || canReverse))
        {
            possibleDirections.Add(new DirectionInfo { enumVal = Direction.Right, vecVal = Vector2.right });
        }

        return possibleDirections;
    }

    public void SetNextTile(DirectionInfo dirInfo)
    {
        previousTilePos = nextTilePos;
        nextTilePos += dirInfo.vecVal;
        currentDirection = dirInfo;
    }

    public void InstantReverseDirection()
    {
        if (!mustResetHalfStep)
        {
            Vector2 oldNextTilePos = nextTilePos;
            nextTilePos = previousTilePos;
            previousTilePos = oldNextTilePos;

            tileTransitionPerc = 1 - tileTransitionPerc;

            switch (currentDirection.enumVal)
            {
                case Direction.Up:
                    {
                        currentDirection.enumVal = Direction.Down;
                        currentDirection.vecVal = Vector2.down;
                    }
                    break;
                case Direction.Down:
                    {
                        currentDirection.enumVal = Direction.Up;
                        currentDirection.vecVal = Vector2.up;
                    }
                    break;
                case Direction.Left:
                    {
                        currentDirection.enumVal = Direction.Right;
                        currentDirection.vecVal = Vector2.right;
                    }
                    break;
                case Direction.Right:
                    {
                        currentDirection.enumVal = Direction.Left;
                        currentDirection.vecVal = Vector2.left;
                    }
                    break;
            }
        }
    }

    public DirectionInfo GetCurrentDirectionInfo()
    {
        return currentDirection;
    }

    public void HalfMovementSpeedMultiplier()
    {
        movementSpeedMultiplier = 0.5f;
    }

    public void ResetMovementSpeedMultiplier()
    {
        movementSpeedMultiplier = 1;
    }

    public void DoubleMovementSpeedMultiplier()
    {
        movementSpeedMultiplier = 2;
    }

    public void UpdatePosition()
    {
        previousTilePos = transform.position;
        nextTilePos = previousTilePos + currentDirection.vecVal;
    }
}
