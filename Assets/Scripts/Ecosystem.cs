using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ecosystem : MonoBehaviour {
    private static Ecosystem Instance;
    public static Ecosystem instance {
        get {
            if (Instance != null) return Instance;
            Instance = GameObject.Find("Ecosystem Overview").GetComponent<Ecosystem>();
            Instance.start();
            return Instance;
        }
    }

    [SerializeField] private int trifishCount = 0;
    [SerializeField] private int sharkCount = 0;

    private void start() { }
    
    public GameObject instantiate(GameObject prefab, Transform root) {
        GameObject newGameObject = Instantiate(prefab, root);
        if (newGameObject.CompareTag("Trifish")) trifishCount++;
        if (newGameObject.CompareTag("Shark")) sharkCount++;
        return newGameObject;
    }

    public void destroy(GameObject gameObject) {
        Destroy(gameObject);
        if (gameObject.CompareTag("Trifish")) trifishCount--;
        if (gameObject.CompareTag("Shark")) sharkCount--;
    }
}
