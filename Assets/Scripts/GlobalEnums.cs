using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Up,
    Down,
    Left,
    Right,
    None,
}

public struct DirectionInfo
{
    public Direction enumVal;
    public Vector2 vecVal;
}