using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletManager : MonoBehaviour
{
    [SerializeField] private int pelletPointWorth;
    [SerializeField] private int tesseractPointWorth;
    [SerializeField] private Transform infinityStoneSpawnPoint;
    [SerializeField] private InfinityStone realityStone;
    [SerializeField] private InfinityStone powerStone;
    [SerializeField] private InfinityStone mindStone;
    [SerializeField] private InfinityStone timeStone;
    [SerializeField] private InfinityStone soulStone;
    [SerializeField] private InfinityStone thanosGauntlet;
    [SerializeField] private InfinityStone mjolnir;
    [SerializeField] private InfinityStone tempad;

    private GameManager gm;
    private int pelletsRemaining = 0;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        pelletsRemaining = transform.childCount - 1;
    }

    public void RemovePellet()
    {
        pelletsRemaining--;

        if (pelletsRemaining == 185)
        {
            SpawnInfinityStone();
        }

        if (pelletsRemaining == 130)
        {
            gm.IncreaseMainBackgroundMusicPitchToFaster();
        }

        if (pelletsRemaining == 95)
        {
            SpawnInfinityStone();
        }

        if (pelletsRemaining == 70)
        {
            gm.IncreaseMainBackgroundMusicPitchToFastest();
        }

        if (pelletsRemaining <= 0)
        {
            gm.WinRound();
        }
    }

    private void SpawnInfinityStone()
    {
        if (gm.level == 1)
            Instantiate(realityStone, infinityStoneSpawnPoint.position, Quaternion.identity);

        else if (gm.level == 2)
            Instantiate(powerStone, infinityStoneSpawnPoint.position, Quaternion.identity);

        else if (gm.level == 3 || gm.level == 4)
            Instantiate(mindStone, infinityStoneSpawnPoint.position, Quaternion.identity);

        else if (gm.level == 5 || gm.level == 6)
            Instantiate(timeStone, infinityStoneSpawnPoint.position, Quaternion.identity);

        else if (gm.level == 7 || gm.level == 8)
            Instantiate(soulStone, infinityStoneSpawnPoint.position, Quaternion.identity);

        else if (gm.level == 9 || gm.level == 10)
            Instantiate(thanosGauntlet, infinityStoneSpawnPoint.position, Quaternion.identity);

        else if (gm.level == 11 || gm.level == 12)
            Instantiate(mjolnir, infinityStoneSpawnPoint.position, Quaternion.identity);

        else if (gm.level >= 13)
            Instantiate(tempad, infinityStoneSpawnPoint.position, Quaternion.identity);
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
