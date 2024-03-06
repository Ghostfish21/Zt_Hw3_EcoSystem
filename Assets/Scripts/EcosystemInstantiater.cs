using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EcosystemInstantiater : MonoBehaviour {
    public SpriteRenderer trifishGenerateArea;
    public SpriteRenderer sharkGenerateArea;
    public SpriteRenderer landmineGenerateArea;
    public GameObject trifishPrefab;
    public GameObject sharkPrefab;
    public GameObject landminePrefab;
    public Transform trifishRoot;
    public Transform sharkRoot;
    public Transform landmineRoot;

    private void Start() {
        for (int i = 0; i < 200; i++) {
            GameObject newTrifish = Ecosystem.instance.instantiate(trifishPrefab, trifishRoot);
            newTrifish.transform.position = new Vector3(
                UnityEngine.Random.Range(trifishGenerateArea.bounds.min.x, trifishGenerateArea.bounds.max.x),
                UnityEngine.Random.Range(trifishGenerateArea.bounds.min.y, trifishGenerateArea.bounds.max.y),
                0
            );
        }
        
        for (int i = 0; i < 6; i++) {
            GameObject newShark = Ecosystem.instance.instantiate(sharkPrefab, sharkRoot);
            newShark.transform.position = new Vector3(
                UnityEngine.Random.Range(sharkGenerateArea.bounds.min.x, sharkGenerateArea.bounds.max.x),
                UnityEngine.Random.Range(sharkGenerateArea.bounds.min.y, sharkGenerateArea.bounds.max.y),
                0
            );
        }
        
        for (int i = 0; i < 5; i++) {
            GameObject newLandmine = Ecosystem.instance.instantiate(landminePrefab, landmineRoot);
            newLandmine.transform.position = new Vector3(
                UnityEngine.Random.Range(landmineGenerateArea.bounds.min.x, landmineGenerateArea.bounds.max.x),
                UnityEngine.Random.Range(landmineGenerateArea.bounds.min.y, landmineGenerateArea.bounds.max.y),
                0
            );
        }
    }
}
