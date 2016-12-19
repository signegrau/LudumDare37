using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
	public Sprite[] anim;
    public float interval = 0.06f;
    private float animationTimer;

    private SpriteRenderer spriteRenderer;

    private bool paused;

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        GameManager.paused += GameManagerOnPaused;
        GameManager.resume += GameManagerOnResume;
    }

    private void OnDisable()
    {
        GameManager.paused -= GameManagerOnPaused;
        GameManager.resume -= GameManagerOnResume;
    }

    private void GameManagerOnResume(float timeStart, float timeAdd)
    {
        paused = false;
    }

    private void GameManagerOnPaused()
    {
        paused = true;
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

		    animationTimer = 0;
		    while (animationTimer < interval)
		    {
		        if (!paused)
		        {
		            animationTimer += Time.deltaTime;
		        }

		        yield return null;
		    }
		}
    }
}
