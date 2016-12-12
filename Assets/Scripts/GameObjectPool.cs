using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoBehaviour {

    public GameObject prefab;
    public int addAmount = 256;
    private Queue<GameObject> poolQueue;
    private int firstActive;

    void Start() {
        poolQueue = new Queue<GameObject>();
        Upscale();
    }

    public GameObject Depool() {
        if (!poolQueue.Peek()) {
            Upscale();
        }
        var result =  poolQueue.Dequeue();
        result.SetActive(true);
        return result;
    }

    public void Repool(GameObject go) {
        go.SetActive(false);
        poolQueue.Enqueue(go);
    }

    void Upscale() {
        for (int i = 0; i < addAmount; ++i) {
            var go = Instantiate(prefab);
			go.transform.parent = this.transform;
            go.SetActive(false);
            poolQueue.Enqueue(go);
        }
    }

}
