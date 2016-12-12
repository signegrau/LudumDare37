using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class Spring : MonoBehaviour
{
	public AudioSource audioSource;
	public AudioClip springSound;
	public Sprite loaded;
    public Sprite unloaded;
    public float loadTime = 0.5f;

    private SpriteRenderer spriteRenderer;

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnPlayerCollision()
    {
        StartCoroutine(Animation());
    }

    private IEnumerator Animation()
    {
		audioSource.PlayOneShot(springSound);
        spriteRenderer.sprite = unloaded;
        yield return new WaitForSeconds(loadTime);
        spriteRenderer.sprite = loaded;
    }
}
