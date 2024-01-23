using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureEnemyPointsText : MonoBehaviour
{
    [SerializeField] private float floatUpSpeed = 1;
    [SerializeField] private float destroySelfDelay = 1;

    private float destroySelfTimer = 0;

    private void Update()
    {
        transform.position += Vector3.up * floatUpSpeed * Time.deltaTime;

        destroySelfTimer += Time.deltaTime;

        if (destroySelfTimer >= destroySelfDelay)
            Destroy(gameObject);
    }
}
