using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
	public Sprite[] anim;
    public float interval = 0.06f;

    private SpriteRenderer spriteRenderer;

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnPlayerCollision()
    {
		SoundManager.single.PlaySpringBoardSound();
        StartCoroutine(Animation());
    }

    private IEnumerator Animation()
    {
		foreach (var s in anim) {
			spriteRenderer.sprite = s;
			yield return new WaitForSeconds(interval);
		}
    }
}
