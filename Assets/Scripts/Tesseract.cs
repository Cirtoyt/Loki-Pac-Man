using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tesseract : MonoBehaviour
{
    [SerializeField] int pointWorth;

    private GameManager gm;
    private EnemyManager em;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        em = FindObjectOfType<EnemyManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            gm.AddScore(pointWorth);
            em.FrightenEnemies();
            Destroy(gameObject);
        }
    }
}
