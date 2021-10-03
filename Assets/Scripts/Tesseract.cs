using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tesseract : MonoBehaviour
{
    private PelletManager pm;
    private GameManager gm;
    private EnemyManager em;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        pm = transform.parent.GetComponent<PelletManager>();
        gm = FindObjectOfType<GameManager>();
        em = FindObjectOfType<EnemyManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            spriteRenderer.enabled = false;
            gm.AddScore(pm.GetTesseractPointWorth());
            pm.RemovePellet();
            em.FrightenEnemies();
            Destroy(gameObject);
        }
    }
}
