using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemy : MonoBehaviour
{
    public enum Personality
    {
        Shadow,
        Speedy,
        Bashful,
        Pokey,
    }

    public enum MovementState
    {
        InTVA,
        Chasing,
        Scatter,
        Frightened,
        LeavingTVA,
        RetreatingToTVA,
    }

    public enum HomeLocation
    {
        Left,
        Centre,
        Right,
        Outside,
    }

    public Personality personality;
    
    [HideInInspector] public bool frightenIsQueued = false;

    private GameManager gm;
    private EnemyManager em;
    private GridMovement gridMovement;
    private Animator anim;
    private Tilemap TVAGateTilemap;
    private MovementState movementState;
    private HomeLocation homeLocation;
    private DirectionInfo inTVADirection;
    private MovementState preFrightenState;
    private bool canCapturePlayer;
    private float leavingTVAPerc = 0;
    private MovementState currentWanderState = MovementState.Scatter;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        em = FindObjectOfType<EnemyManager>();
        gridMovement = GetComponent<GridMovement>();
        anim = GetComponent<Animator>();
        TVAGateTilemap = GameObject.FindGameObjectWithTag("TVA Gate").GetComponent<Tilemap>();
        canCapturePlayer = true;
    }

    void Update()
    {
        if (movementState == MovementState.InTVA)
        {
            //anim.SetFloat(inHouseDirection
            transform.position += (Vector3)inTVADirection.vecVal * Time.deltaTime * em.GetInTVASpeed();
        }
        else if (movementState == MovementState.LeavingTVA)
        {
            leavingTVAPerc += Time.deltaTime;

            switch (homeLocation)
            {
                case HomeLocation.Right:
                    {
                        transform.position = Vector2.Lerp(em.enemySpawnPoints[1].position, em.enemySpawnPoints[2].position, leavingTVAPerc);
                        if (leavingTVAPerc > 1)
                        {
                            leavingTVAPerc = 0;
                            transform.position = em.enemySpawnPoints[2].position;
                            homeLocation = HomeLocation.Centre;
                        }
                    }
                    break;
                case HomeLocation.Centre:
                    {
                        transform.position = Vector2.Lerp(em.enemySpawnPoints[2].position, em.enemySpawnPoints[0].position, leavingTVAPerc);
                        if (leavingTVAPerc > 1)
                        {
                            leavingTVAPerc = 0;
                            transform.position = em.enemySpawnPoints[0].position;
                            gridMovement.UpdatePosition();
                            gridMovement.canMove = true;
                            if (frightenIsQueued)
                                movementState = MovementState.Frightened;
                            else
                                movementState = currentWanderState;
                        }
                    }
                    break;
                case HomeLocation.Left:
                    {
                        transform.position = Vector2.Lerp(em.enemySpawnPoints[3].position, em.enemySpawnPoints[2].position, leavingTVAPerc);
                        if (leavingTVAPerc > 1)
                        {
                            leavingTVAPerc = 0;
                            transform.position = em.enemySpawnPoints[2].position;
                            homeLocation = HomeLocation.Centre;
                        }
                    }
                    break;
                case HomeLocation.Outside:
                    {
                        transform.position = Vector2.Lerp(em.enemySpawnPoints[0].position, em.enemySpawnPoints[2].position, leavingTVAPerc);
                        if (leavingTVAPerc > 1)
                        {
                            leavingTVAPerc = 0;
                            transform.position = em.enemySpawnPoints[2].position;
                            homeLocation = HomeLocation.Centre;
                            EndFrighten();
                        }
                    }
                    break;
            }
        }
        else if (movementState == MovementState.RetreatingToTVA)
        {
            Vector3Int gridPos = TVAGateTilemap.WorldToCell(transform.position);
            if (TVAGateTilemap.HasTile(gridPos))
            {
                gridMovement.canMove = false;
                homeLocation = HomeLocation.Outside;
                movementState = MovementState.LeavingTVA;
            }
        }
        else
        {
            //anime.SetFloat(gridMovement.GetCurrentDirectionInfo
        }
    }

    public void ProcessDirectionChange(List<DirectionInfo> possibleDirections)
    {
        switch (movementState)
        {
            case MovementState.Chasing:
                {
                    GetChaseDirection(possibleDirections);
                    // personality depictants target tile
                }
                break;
            case MovementState.Scatter:
                {
                    GetScatterDirection(possibleDirections);
                    // direction that is closest to their own corner
                }
                break;
            case MovementState.Frightened:
                {
                    // randomised direction from options
                    int directionIndex = 0;
                    if (possibleDirections.Count > 1)
                    {
                        directionIndex = Random.Range(0, possibleDirections.Count);
                    }
                    gridMovement.SetNextTile(possibleDirections[directionIndex]);
                }
                break;
            case MovementState.RetreatingToTVA:
                {
                    // run fastest route towards TVA Gate entrance way
                    float closestDistance = Mathf.Infinity;
                    int closestIndex = 0;
                    for (int i = 0; i < possibleDirections.Count; i++)
                    {
                        if (Vector2.Distance((Vector2)transform.position + possibleDirections[i].vecVal, em.enemySpawnPoints[0].position) < closestDistance)
                        {
                            closestDistance = Vector2.Distance((Vector2)transform.position + possibleDirections[i].vecVal, em.enemySpawnPoints[0].position);
                            closestIndex = i;
                        }    
                    }
                    gridMovement.SetNextTile(possibleDirections[closestIndex]);
                }
                break;
        }
    }

    private void GetChaseDirection(List<DirectionInfo> possibleDirections)
    {
        Vector2 lokiPos = FindObjectOfType<Loki>().transform.position;
        Vector2 target = Vector2.zero;
        switch (personality)
        {
            case Personality.Shadow:
                {
                    target = lokiPos;
                }
                break;
            case Personality.Speedy:
                {
                    target = lokiPos + (FindObjectOfType<Loki>().GetComponent<GridMovement>().GetCurrentDirectionInfo().vecVal * 2);
                }
                break;
            case Personality.Bashful:
                {
                    Vector2 direction = (lokiPos - em.GetShadowPosition()).normalized;
                    target = lokiPos + (direction * 2);
                }
                break;
            case Personality.Pokey:
                {
                    if (Vector2.Distance(transform.position, lokiPos) > 8)
                    {
                        target = lokiPos;
                    }
                    else
                    {
                        target = em.enemyCornerTargets[3].position;
                    }
                }
                break;
        }

        float closestDistance = Mathf.Infinity;
        int closestIndex = 0;
        for (int i = 0; i < possibleDirections.Count; i++)
        {
            if (Vector2.Distance((Vector2)transform.position + possibleDirections[i].vecVal, target) < closestDistance)
            {
                closestDistance = Vector2.Distance((Vector2)transform.position + possibleDirections[i].vecVal, target);
                closestIndex = i;
            }
        }
        gridMovement.SetNextTile(possibleDirections[closestIndex]);
    }

    private void GetScatterDirection(List<DirectionInfo> possibleDirections)
    {
        Vector2 target = Vector2.zero;
        switch (personality)
        {
            case Personality.Shadow:
                {
                    target = em.enemyCornerTargets[0].position;
                }
                break;
            case Personality.Speedy:
                {
                    target = em.enemyCornerTargets[1].position;
                }
                break;
            case Personality.Bashful:
                {
                    target = em.enemyCornerTargets[2].position;
                }
                break;
            case Personality.Pokey:
                {
                    target = em.enemyCornerTargets[3].position;
                }
                break;
        }

        float closestDistance = Mathf.Infinity;
        int closestIndex = 0;
        for (int i = 0; i < possibleDirections.Count; i++)
        {
            if (Vector2.Distance((Vector2)transform.position + possibleDirections[i].vecVal, target) < closestDistance)
            {
                closestDistance = Vector2.Distance((Vector2)transform.position + possibleDirections[i].vecVal, target);
                closestIndex = i;
            }
        }
        gridMovement.SetNextTile(possibleDirections[closestIndex]);
    }

    public void SetupInTVA(DirectionInfo facingDirection)
    {
        inTVADirection = facingDirection;
        movementState = MovementState.InTVA;
    }

    public void SetupOutsideTVA()
    {
        movementState = currentWanderState;
        gridMovement.canMove = true;
    }

    public void SetHomeLocation(HomeLocation homeLoc)
    {
        homeLocation = homeLoc;
    }

    public void LeaveTVA(Vector3 exitPosition)
    {
        movementState = MovementState.LeavingTVA;
    }

    public void SwitchToChase()
    {
        if (currentWanderState == MovementState.Scatter)
        {
            currentWanderState = MovementState.Chasing;
            movementState = MovementState.Chasing;
        }
    }

    public void SwitchToScatter()
    {
        if (currentWanderState == MovementState.Chasing)
        {
            currentWanderState = MovementState.Scatter;
            movementState = MovementState.Scatter;
        }
    }

    public void Frighten()
    {
        if (movementState == MovementState.Chasing || movementState == MovementState.Scatter)
        {
            preFrightenState = movementState;
            gridMovement.InstantReverseDirection();
            gridMovement.HalfMovementSpeedMultiplier();
            movementState = MovementState.Frightened;
        }
        else
        {
            frightenIsQueued = true;
        }

        GetComponent<SpriteRenderer>().color = Color.blue;
        canCapturePlayer = false;
    }

    public void SetSpriteAsFrightened(bool isNowFrightened)
    {
        //anim.SetBool("Frightened", isNowFrightened);
        if (isNowFrightened)
        {
            GetComponent<SpriteRenderer>().color = Color.blue;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    public void RetreatToTVA()
    {
        movementState = MovementState.RetreatingToTVA;
        gridMovement.DoubleMovementSpeedMultiplier();
        GetComponent<SpriteRenderer>().color = Color.cyan;
    }

    public void EndFrighten()
    {
        if (movementState == MovementState.Frightened)
        {
            movementState = preFrightenState;
        }

        gridMovement.ResetMovementSpeedMultiplier();
        canCapturePlayer = true;
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    public MovementState GetMovementState()
    {
        return movementState;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && canCapturePlayer)
        {
            Debug.Log("Dead");
            gm.EndRound();
        }
        else if (movementState == MovementState.InTVA && collision.tag == "TVA")
        {
            if (inTVADirection.enumVal == Direction.Up)
            {
                inTVADirection = new DirectionInfo { enumVal = Direction.Down, vecVal = Vector2.down };
            }
            else if (inTVADirection.enumVal == Direction.Down)
            {
                inTVADirection = new DirectionInfo { enumVal = Direction.Up, vecVal = Vector2.up };
            }
        }
    }
}