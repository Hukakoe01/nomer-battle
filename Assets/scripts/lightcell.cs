using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightcell : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float timer = 0f;
    private bool isVisible = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void StartBlinking()
    {
        enabled = true;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 0.4f)
        {
            isVisible = !isVisible;
            if (spriteRenderer != null)
                spriteRenderer.color = isVisible ? Color.white : new Color(1, 1, 1, 0.5f);
            timer = 0f;
        }
    }
}
