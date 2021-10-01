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

    public Personality personality;

    private enum MovementState
    {
        InHouse,
        Chasing,
        Scatter,
        Frightened,
    }

    private GridMovement gridMovement;
    private MovementState movementState;

    void Start()
    {
        gridMovement = GetComponent<GridMovement>();
        if (personality == Personality.Shadow)
            movementState = MovementState.Frightened;
        else
            movementState = MovementState.InHouse;
    }

    void Update()
    {
        
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
        }
    }

    private void GetChaseDirection(List<DirectionInfo> possibleDirections)
    {
        switch (personality)
        {
            case Personality.Shadow:
                {

                }
                break;
            case Personality.Speedy:
                {

                }
                break;
            case Personality.Bashful:
                {

                }
                break;
            case Personality.Pokey:
                {

                }
                break;
        }
    }

    private void GetScatterDirection(List<DirectionInfo> possibleDirections)
    {
        switch (personality)
        {
            case Personality.Shadow:
                {

                }
                break;
            case Personality.Speedy:
                {

                }
                break;
            case Personality.Bashful:
                {

                }
                break;
            case Personality.Pokey:
                {

                }
                break;
        }
    }
}
