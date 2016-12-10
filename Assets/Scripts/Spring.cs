using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class Spring : MonoBehaviour
{
    public Sprite loaded;
    public Sprite unloaded;
    public float loadTime = 0.5f;

    private SpriteRenderer spriteRenderer;

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        var other = collision2D.collider;

        if (other.CompareTag("Player"))
        {
            StartCoroutine(Animation());
        }
    }

    private IEnumerator Animation()
    {
        spriteRenderer.sprite = unloaded;
        yield return new WaitForSeconds(loadTime);
        spriteRenderer.sprite = loaded;
    }
}
