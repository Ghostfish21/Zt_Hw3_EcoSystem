using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewField : MonoBehaviour {
    public readonly Dictionary<string, Dictionary<GameObject, bool>> detectedObjs = new ();
    public int detectedObjsCount = 0;
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (!detectedObjs.ContainsKey(other.gameObject.tag))
            detectedObjs[other.gameObject.tag] = new Dictionary<GameObject, bool>();
        
        detectedObjs[other.gameObject.tag][other.gameObject] = true;
        detectedObjsCount++;
    }
        
    private void OnTriggerExit2D(Collider2D other) {
        detectedObjs[other.gameObject.tag].Remove(other.gameObject);
        detectedObjsCount--;
    }
    
    public Dictionary<GameObject, bool>.KeyCollection getDetectedObjs(string tag) {
        if (!detectedObjs.ContainsKey(tag))
            detectedObjs[tag] = new Dictionary<GameObject, bool>();
        return detectedObjs[tag].Keys;
    }
    
    public bool isDetected(string tag, GameObject obj) {
        if (!detectedObjs.ContainsKey(tag))
            detectedObjs[tag] = new Dictionary<GameObject, bool>();
        return detectedObjs[tag].ContainsKey(obj);
    }
}
