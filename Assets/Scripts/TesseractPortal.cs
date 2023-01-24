using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesseractPortal : MonoBehaviour
{
    private GameManager gm;
    private GameObject loki;
    private bool shouldRemoveLife = false;

    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
    }

    private void SpawnLoki()
    {
        loki = Instantiate(gm.GetLokiPrefab(), gm.GetSpawnPoint().position, gm.GetSpawnPoint().rotation);
        
        if (shouldRemoveLife)
        {
            gm.RemoveLife();
        }
    }

    private void ClosePortal()
    {
        gm.AddLokiRef(loki.GetComponent<Loki>());
        Destroy(gameObject);
    }

    public void SetShouldRemoveLife(bool value)
    {
        shouldRemoveLife = value;
    }
}
