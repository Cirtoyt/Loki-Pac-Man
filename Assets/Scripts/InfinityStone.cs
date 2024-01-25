using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfinityStone : MonoBehaviour
{
    [SerializeField] private int pointsWorth;
    [SerializeField] TextMeshPro capturedInfinityStonePointTextPrefab;
    [SerializeField] private float despawnDelay = 9.5f;

    private GameManager gm;
    private SpriteRenderer spriteRenderer;
    private float despawnTimer = 0;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        despawnTimer += Time.deltaTime;

        if (despawnTimer >= despawnDelay)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.GetComponent<Loki>().PlayWakaWakaSound();
            spriteRenderer.enabled = false;
            Instantiate(capturedInfinityStonePointTextPrefab, transform.position, Quaternion.identity).text = pointsWorth.ToString();
            gm.AddScore(pointsWorth);
            Destroy(gameObject);
        }
    }
}
