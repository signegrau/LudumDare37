using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserCannon : MonoBehaviour {

	public float shootInterval;
	public Sprite[] anim;
	private SpriteRenderer spriteRenderer;
	private LineRenderer lineRenderer;

	private float shootTimer;
	private float invShootInterval;

	private static bool hasPlayedSound;

    // Use this for initialization
	void Start () {
		if (anim.Length != 3) {
			Debug.LogError("Laser cannon sprite animation array must have excactly 3 elements.");
		}
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		lineRenderer = gameObject.GetComponent<LineRenderer>();
		shootTimer = 0;
		invShootInterval = 1f/shootInterval;
		hasPlayedSound = false;

		lineRenderer.SetPositions(new Vector3[8]);
	}
	
	// Update is called once per frame
	void Update () {
		float percentReadyToShoot = ((GameManager.StartTime + GameManager.TimeElapsed) % shootInterval) / shootInterval;

		if (percentReadyToShoot > 0.9f) {
			lineRenderer.enabled = true;
			Shoot();
			if (spriteRenderer.sprite != anim[2]) {
				spriteRenderer.sprite = anim[2];
			}
			if(!hasPlayedSound) {
				hasPlayedSound = true;
				SoundManager.single.PlayLaserSound();
				StartCoroutine(LaserSoundTimeout(SoundManager.single.laserSound.length));
			}
		}
		else {
			lineRenderer.enabled = false;
			if (percentReadyToShoot > 0.7f) {
				spriteRenderer.sprite = anim[1];
			}
			else {
				if (spriteRenderer.sprite != anim[0]) {
					spriteRenderer.sprite = anim[0];
				}
			}
		}
	}

	private IEnumerator LaserSoundTimeout(float seconds) {
		yield return new WaitForSeconds(seconds);
		hasPlayedSound = false;
	}

	List<Vector3> hitLocations;

	private void Shoot() {
		hitLocations = new List<Vector3>();

		Vector3[] cardinalDirections = {Vector3.up, Vector3.right, Vector3.down, Vector3.left};

		for (var i = 0; i < cardinalDirections.Length; ++i)
		{
			var dir = cardinalDirections[i];
			var layers = 1 << LayerMask.NameToLayer("Solid") | 1 << LayerMask.NameToLayer("Player");

			RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one*0.2f, 0, dir, 100, layers);

		    hitLocations.Add(transform.position);

		    if (hit.collider != null)
		    {
		        if (hit.collider.CompareTag("Player"))
		        {
		            hit.collider.GetComponent<PlayerScript>().LaserHit();
		        }

		        hitLocations.Add(hit.point);
		    }
		    else
		    {
		        hitLocations.Add(transform.position + dir * 100);
		    }
		}

		lineRenderer.SetPositions(hitLocations.ToArray());
	}
}
