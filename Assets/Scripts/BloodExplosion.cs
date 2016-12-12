using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodExplosion : MonoBehaviour {

    public GameObject bloodPrefab;
    public int amount;
    public float minLifeTime;
    public float maxLifeTime;
    public float force;
    public Sprite[] sprites;
    private GameObjectPool bloodPool;

	// Use this for initialization
	void Start () {
		bloodPool = FindObjectOfType<GameObjectPool>();

	   	for (int i = amount; i >= 0; --i) {
            var go = bloodPool.Depool();
            go.transform.position = transform.position;
            go.GetComponent<Rigidbody2D>().velocity = (Random.insideUnitCircle + Vector2.up) * force;
            go.GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Length-1)];
            StartCoroutine(killBlood(go, Random.Range(minLifeTime, maxLifeTime)));
        }
		killExplosion(maxLifeTime + 0.1f);
	}

	private IEnumerator killExplosion(float seconds) {
		yield return new WaitForSeconds(seconds);
		Destroy(gameObject);
	}

    private IEnumerator killBlood(GameObject go, float seconds) {
        yield return new WaitForSeconds(seconds);
        bloodPool.Repool(go);
    }
}
