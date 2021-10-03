using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVAPortal : MonoBehaviour
{
    private GameManager gm;
    private GameObject loki;

    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        loki = GameObject.FindGameObjectWithTag("Player");
    }
    private void DestroyLoki()
    {
        gm.RemoveLokiRef();
        Destroy(loki);
    }

    private void ClosePortal()
    {
        gm.CloseTVAPortal();
        Destroy(gameObject);
    }
}
