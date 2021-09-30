using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loki : MonoBehaviour
{
    [HideInInspector] public bool canMove;

    private Vector2 currentdirection;

    void Start()
    {
        canMove = false;
        currentdirection = new Vector2(-1, 0);
    }

    void Update()
    {
        // movement stuff
    }
}
