using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridMovement : MonoBehaviour
{
    public float movementSpeed;

    [HideInInspector] public bool canMove;

    private Tilemap wallTilemap;
    private Tilemap decisionPointTilemap;
    private Tilemap uTurnPointTilemap;
    private Tilemap tunnelTilemap;
    private Vector2 enterMazePos;
    private Vector2 previousTilePos;
    private bool mustResetDirectionSpeed;
    private float directionSpeedMultiplier;
    private DirectionInfo currentDirection;
    private Vector2 nextTilePos;
    private float tileTransitionPerc;

    void Awake()
    {
        canMove = false;
        wallTilemap = GameObject.FindGameObjectWithTag("Wall").GetComponent<Tilemap>();
        decisionPointTilemap = GameObject.FindGameObjectWithTag("Decision Point").GetComponent<Tilemap>();
        uTurnPointTilemap = GameObject.FindGameObjectWithTag("U-Turn Point").GetComponent<Tilemap>();
        tunnelTilemap = GameObject.FindGameObjectWithTag("Tunnel").GetComponent<Tilemap>();
        enterMazePos = new Vector2(-3.5f, 3);
        transform.position = enterMazePos;
        previousTilePos = enterMazePos;
        mustResetDirectionSpeed = true;
        directionSpeedMultiplier = 2;
        currentDirection.enumVal = Direction.Left;
        currentDirection.vecVal = Vector2.left / 2;
        nextTilePos = enterMazePos + currentDirection.vecVal;
        tileTransitionPerc = 0;
    }

    void Update()
    {
        if (canMove)
        {
            // Increase between tile transition percentage
            if (mustResetDirectionSpeed)
            {
                tileTransitionPerc += Time.deltaTime * movementSpeed * directionSpeedMultiplier;
            }
            else
            {
                tileTransitionPerc += Time.deltaTime * movementSpeed;
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
                if (mustResetDirectionSpeed)
                {
                    currentDirection.vecVal = Vector2.left;
                    mustResetDirectionSpeed = false;
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
        if (TryGetComponent(out Enemy enemy))
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
            && (currentDirection.enumVal != Direction.Down || canReverse))
        {
            possibleDirections.Add(new DirectionInfo { enumVal = Direction.Up, vecVal = Vector2.up });
        }

        // Down check
        Vector3Int downRightGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.down * 1.5f) + (Vector3.right / 2));
        Vector3Int downLeftGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.down * 1.5f) + (Vector3.left / 2));
        if (!wallTilemap.HasTile(downRightGridPos) && !wallTilemap.HasTile(downLeftGridPos)
            && (currentDirection.enumVal != Direction.Up || canReverse))
        {
            possibleDirections.Add(new DirectionInfo { enumVal = Direction.Down, vecVal = Vector2.down });
        }

        // Left check
        Vector3Int leftUpGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.left * 1.5f) + (Vector3.up / 2));
        Vector3Int leftDownGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.left * 1.5f) + (Vector3.down / 2));
        if (!wallTilemap.HasTile(leftUpGridPos) && !wallTilemap.HasTile(leftDownGridPos)
            && (currentDirection.enumVal != Direction.Right || canReverse))
        {
            possibleDirections.Add(new DirectionInfo { enumVal = Direction.Left, vecVal = Vector2.left });
        }

        // Right check
        Vector3Int rightUpGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.right * 1.5f) + (Vector3.up / 2));
        Vector3Int rightDownGridPos = wallTilemap.WorldToCell(transform.position + (Vector3.right * 1.5f) + (Vector3.down / 2));
        if (!wallTilemap.HasTile(rightUpGridPos) && !wallTilemap.HasTile(rightDownGridPos)
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

    public DirectionInfo GetCurrentDirectionInfo()
    {
        return currentDirection;
    }

    public void SetSpawnPosition(Vector2 position, bool isHalfStepping)
    {
        enterMazePos = position;
        transform.position = enterMazePos;
        previousTilePos = enterMazePos;
        if (isHalfStepping)
        {
            currentDirection.vecVal = Vector2.left / 2;
            mustResetDirectionSpeed = isHalfStepping;
        }
        nextTilePos = enterMazePos + currentDirection.vecVal;
    }

    public void InstantReverseDirection()
    {
        if (!mustResetDirectionSpeed)
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
}
