using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReproductionTail : MonoBehaviour {
    private Transform entityRoot;
    public GameObject trifishPrefab;
    
    public int hitCount = 0;

    private void Start() {
        entityRoot = GameObject.Find("Trifishes").transform;
        trifishPrefab = Resources.Load<GameObject>("Tile Textures/Entities/Trifish");
    }

    private void Update() {
        if (hitCount < 500) return;
        hitCount = 0;
        GameObject newTrifish = Ecosystem.instance.instantiate(trifishPrefab, entityRoot);
        newTrifish.transform.position = new Vector3(transform.position.x - 0.3f, transform.position.y - 0.3f, transform.position.z - 0.3f);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Trifish")) return;
        hitCount++;
    }
}
