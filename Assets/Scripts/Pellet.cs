using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pellet : MonoBehaviour
{
    private PelletManager pm;
    private GameManager gm;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        pm = transform.parent.GetComponent<PelletManager>();
        gm = FindObjectOfType<GameManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.GetComponent<Loki>().PlayWakaWakaSound();
            spriteRenderer.enabled = false;
            gm.AddScore(pm.GetPelletPointWorth());
            pm.RemovePellet();
            Destroy(gameObject);
        }
    }
}
