using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBlinker : MonoBehaviour
{
    [SerializeField] private float blinkInterval;
    [SerializeField] private bool startVisible;

    private Text text;
    private bool showText;
    private float timer;

    void Awake()
    {
        text = GetComponent<Text>();
        timer = 0;

        if (startVisible)
        {
            text.enabled = true;
        }
        else
        {
            text.enabled = false;
        }
    }

    void Update()
    {
        if (showText && blinkInterval != 0)
        {
            timer += Time.deltaTime;

            if (timer >= blinkInterval)
            {
                text.enabled = !text.enabled;
                timer = 0;
            }
        }
    }

    public void EnableText()
    {
        showText = true;
        timer = 0;
        text.enabled = true;
    }

    public void DisableText()
    {
        showText = false;
        timer = 0;
        text.enabled = false;
    }
}
