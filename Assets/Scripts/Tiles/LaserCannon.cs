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
		
		//shootTimer += Time.deltaTime;
//		float percentReadyToShoot = invShootInterval*shootTimer;

		float percentReadyToShoot = (Time.time % shootInterval) / shootInterval;

		if (percentReadyToShoot > 0.9f) {
			lineRenderer.enabled = true;
			Shoot();
			if(!hasPlayedSound) {
				hasPlayedSound = true;
				SoundManager.single.PlayLaserSound();
				StartCoroutine(LaserSoundTimeout(SoundManager.single.laserSound.length));
			}
			if (spriteRenderer.sprite != anim[2]) {
				spriteRenderer.sprite = anim[2];
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
		Vector3[] cardinalDirections = new Vector3[] {Vector3.up, Vector3.right, Vector3.down, Vector3.left};
		for (int i = 0; i < cardinalDirections.Length; ++i) {
			Vector3 dir = cardinalDirections[i];
			int layers = 1 << LayerMask.NameToLayer("Solid") | 1 << LayerMask.NameToLayer("Player");
			RaycastHit2D hit = Physics2D.Raycast(transform.position	, (Vector2)dir, Mathf.Infinity, layers);
			hitLocations.Add(transform.position);
			if (hit.collider != null) {
				if (hit.collider.CompareTag("Player")) {
					hit.collider.GetComponent<PlayerScript>().LaserHit();
				}
				hitLocations.Add(hit.point);
			}
			else {
				hitLocations.Add(transform.position + 1000*dir);
			}
		}

		lineRenderer.SetPositions(hitLocations.ToArray());
	}
}
