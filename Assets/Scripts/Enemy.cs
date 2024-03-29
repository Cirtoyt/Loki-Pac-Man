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
        RetreatingToTVA
    }

    public enum HomeLocation
    {
        Left,
        Centre,
        Right,
        Outside,
    }

    public Personality personality;
    [SerializeField] private Color frightenedColour;
    [SerializeField] private Color retreatingColour;
    [SerializeField] private float level1SpeedMultiplier = 0.75f;
    [SerializeField] private float level2To4SpeedMultiplier = 0.85f;
    [SerializeField] private float level5To20SpeedMultiplier = 0.95f;
    [SerializeField] private float level21PlusSpeedMultiplier = 0.95f;

    [HideInInspector] public bool canMoveInTVA = true;

    [Header("Debug")]
    [SerializeField] private MovementState movementState;

    private GameManager gm;
    private EnemyManager em;
    private GridMovement gridMovement;
    private Animator anim;
    private Tilemap TVAGateTilemap;
    private HomeLocation homeLocation;
    private DirectionInfo inTVADirection;
    private bool canCapturePlayer;
    private float leavingTVAPerc = 0;
    private bool frightenIsQueued = false;
    private MovementState currentWanderState = MovementState.Scatter;
    private Color baseSpriteColour;

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        em = FindObjectOfType<EnemyManager>();
        gridMovement = GetComponent<GridMovement>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        TVAGateTilemap = GameObject.FindGameObjectWithTag("TVA Gate").GetComponent<Tilemap>();
        canCapturePlayer = true;
        baseSpriteColour = GetComponent<SpriteRenderer>().color;
    }

    void Update()
    {
        if (movementState == MovementState.InTVA && canMoveInTVA)
        {
            anim.SetFloat("walkingRight", inTVADirection.vecVal.x);
            anim.SetFloat("walkingUp", inTVADirection.vecVal.y);
            if (frightenIsQueued)
                transform.position += (Vector3)inTVADirection.vecVal * Time.deltaTime * (em.GetInTVASpeed() / 2);
            else
                transform.position += (Vector3)inTVADirection.vecVal * Time.deltaTime * em.GetInTVASpeed();
        }
        else if (movementState == MovementState.LeavingTVA && canMoveInTVA)
        {
            leavingTVAPerc += Time.deltaTime;

            switch (homeLocation)
            {
                case HomeLocation.Right:
                    {
                        anim.SetFloat("walkingRight", -1);
                        anim.SetFloat("walkingUp", 0);
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
                        anim.SetFloat("walkingRight", 0);
                        anim.SetFloat("walkingUp", 1);
                        transform.position = Vector2.Lerp(em.enemySpawnPoints[2].position, em.enemySpawnPoints[0].position, leavingTVAPerc);
                        if (leavingTVAPerc > 1)
                        {
                            leavingTVAPerc = 0;
                            transform.position = em.enemySpawnPoints[0].position;
                            DirectionInfo enterMazeDirection = new DirectionInfo { enumVal = Direction.Left, vecVal = Vector2.left };
                            gridMovement.SetSpawnPosition(enterMazeDirection, transform.position, true);
                            gridMovement.ResetMovementSpeedMultiplier();
                            gridMovement.canMove = true;
                            if (frightenIsQueued)
                            {
                                movementState = MovementState.Frightened;
                                gridMovement.HalfMovementSpeedMultiplier();
                                GetComponent<SpriteRenderer>().color = frightenedColour;
                                canCapturePlayer = false;
                                frightenIsQueued = false;
                            }
                            else
                                movementState = currentWanderState;
                        }
                    }
                    break;
                case HomeLocation.Left:
                    {
                        anim.SetFloat("walkingRight", 1);
                        anim.SetFloat("walkingUp", 0);
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
                        anim.SetFloat("walkingRight", 0);
                        anim.SetFloat("walkingUp", -1);
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
            anim.SetFloat("walkingRight", gridMovement.GetCurrentDirectionInfo().vecVal.x);
            anim.SetFloat("walkingUp", gridMovement.GetCurrentDirectionInfo().vecVal.y);
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
            anim.SetFloat("walkingRight", gridMovement.GetCurrentDirectionInfo().vecVal.x);
            anim.SetFloat("walkingUp", gridMovement.GetCurrentDirectionInfo().vecVal.y);
        }
    }

    public void ProcessDirectionChange(List<DirectionInfo> possibleDirections)
    {
        switch (movementState)
        {
            case MovementState.Chasing:
                {
                    // personality depicts target tile
                    GetChaseDirection(possibleDirections);
                }
                break;
            case MovementState.Scatter:
                {
                    // direction that is closest to their own corner
                    GetScatterDirection(possibleDirections);
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
                    Debug.DrawRay(target, Vector2.up, Color.red, 1);
                }
                break;
            case Personality.Speedy:
                {
                    target = lokiPos + (FindObjectOfType<Loki>().GetComponent<GridMovement>().GetCurrentDirectionInfo().vecVal * 4);
                    Debug.DrawRay(target, Vector2.up, Color.magenta, 1);
                }
                break;
            case Personality.Bashful:
                {
                    Vector2 direction = (lokiPos - em.GetShadowPosition()).normalized;
                    target = lokiPos + (direction * 8);
                    Debug.DrawRay(target, Vector2.up, Color.cyan, 1);
                }
                break;
            case Personality.Pokey:
                {
                    if (Vector2.Distance(transform.position, lokiPos) > 8)
                    {
                        target = lokiPos;
                        Debug.DrawRay(target, Vector2.up, Color.yellow, 1);
                    }
                    else
                    {
                        target = em.enemyCornerTargets[3].position;
                        Debug.DrawRay(target, Vector2.up, Color.yellow, 1);
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
                    Debug.DrawRay(target, Vector2.up, Color.red, 1);
                }
                break;
            case Personality.Speedy:
                {
                    target = em.enemyCornerTargets[1].position;
                    Debug.DrawRay(target, Vector2.up, Color.magenta, 1);
                }
                break;
            case Personality.Bashful:
                {
                    target = em.enemyCornerTargets[2].position;
                    Debug.DrawRay(target, Vector2.up, Color.cyan, 1);
                }
                break;
            case Personality.Pokey:
                {
                    target = em.enemyCornerTargets[3].position;
                    Debug.DrawRay(target, Vector2.up, Color.yellow, 1);
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
        DirectionInfo enterMazeDirection = new DirectionInfo { enumVal = Direction.Left, vecVal = Vector2.left };
        gridMovement.SetSpawnPosition(enterMazeDirection, transform.position, true);
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
        if (movementState == MovementState.Scatter)
        {
            movementState = MovementState.Chasing;
        }
        currentWanderState = MovementState.Chasing;
    }

    public void SwitchToScatter()
    {
        if (movementState == MovementState.Chasing)
        {
            movementState = MovementState.Scatter;
        }
        currentWanderState = MovementState.Scatter;
    }

    public void Frighten()
    {
        if (movementState == MovementState.Chasing || movementState == MovementState.Scatter)
        {
            gridMovement.InstantReverseDirection();
            movementState = MovementState.Frightened;
            gridMovement.HalfMovementSpeedMultiplier();
            GetComponent<SpriteRenderer>().color = frightenedColour;
            canCapturePlayer = false;
        }
        else if (movementState == MovementState.InTVA || movementState == MovementState.LeavingTVA)
        {
            GetComponent<SpriteRenderer>().color = frightenedColour;
            frightenIsQueued = true;
        }
    }

    public void SetSpriteAsFrightened(bool isNowFrightened)
    {
        //anim.SetBool("Frightened", isNowFrightened);
        if (isNowFrightened && (movementState == MovementState.Frightened || frightenIsQueued))
        {
            GetComponent<SpriteRenderer>().color = frightenedColour;
        }
        else if (!isNowFrightened && (movementState == MovementState.Frightened || frightenIsQueued))
        {
            GetComponent<SpriteRenderer>().color = baseSpriteColour;
        }
    }

    public void RetreatToTVA()
    {
        movementState = MovementState.RetreatingToTVA;
        gridMovement.DoubleMovementSpeedMultiplier();
        GetComponent<SpriteRenderer>().color = retreatingColour;
        // Shouldn't need this, but just in-case of weird bug where ghosts are able to catch you whilst retreating
        canCapturePlayer = false;
    }

    public void EndFrighten()
    {
        if (movementState == MovementState.RetreatingToTVA)
            return;

        if (movementState == MovementState.Frightened)
        {
            movementState = currentWanderState;
        }

        gridMovement.ResetMovementSpeedMultiplier();
        frightenIsQueued = false;
        canCapturePlayer = true;
        GetComponent<SpriteRenderer>().color = baseSpriteColour;
    }

    public MovementState GetMovementState()
    {
        return movementState;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && canCapturePlayer)
        {
            gm.LoseLife(this);
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

    public void UpdateLevelSpeedMultiplier(int newLevel)
    {
        if (newLevel == 1)
        {
            gridMovement.movementSpeedLevelMultiplier = level1SpeedMultiplier;
        }
        else if (newLevel >= 2 && newLevel <= 4)
        {
            gridMovement.movementSpeedLevelMultiplier = level2To4SpeedMultiplier;
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
}
