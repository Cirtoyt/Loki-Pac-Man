using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletManager : MonoBehaviour
{
    [SerializeField] private int pelletPointWorth;
    [SerializeField] private int tesseractPointWorth;

    private GameManager gm;
    private int pelletsRemaining = 0;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        pelletsRemaining = transform.childCount;
    }

    public void RemovePellet()
    {
        pelletsRemaining--;
        if (pelletsRemaining <= 0)
        {
            gm.WinRound();
        }
    }

    public int GetPelletPointWorth()
    {
        return pelletPointWorth;
    }

    public int GetTesseractPointWorth()
    {
        return tesseractPointWorth;
    }
}
