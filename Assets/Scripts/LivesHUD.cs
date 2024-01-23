using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivesHUD : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> lives;

    private int livesCounter = 0;

    void Start()
    {
        foreach(var life in lives)
        {
            life.enabled = true;
            livesCounter++;
        }
    }

    public void RemoveLife()
    {
        livesCounter--;
        for (int i = 0; i < lives.Count; i++)
        {
            if (i < livesCounter)
            {
                lives[i].enabled = true;
            }
            else
            {
                lives[i].enabled = false;
            }
        }
    }

    public void AddLife()
    {
        livesCounter++;
        for (int i = 0; i < lives.Count; i++)
        {
            if (i < livesCounter)
            {
                lives[i].enabled = true;
            }
            else
            {
                lives[i].enabled = false;
            }
        }
    }

    public void ResetLives()
    {
        livesCounter = lives.Count;
        foreach (var life in lives)
        {
            life.enabled = true;
        }
    }
}
