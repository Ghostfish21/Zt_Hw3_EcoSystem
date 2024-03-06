using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkViewField : MonoBehaviour {
    public readonly Dictionary<Entity, bool> detectedEntities = new(); // 检测到的实体, The detected entities

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Entity")) return;
        Entity entity = other.gameObject.GetComponent<Entity>();
        this.detectedEntities[entity] = true;
    }
        
    private void OnTriggerExit2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Entity")) return;
        Entity entity = other.gameObject.GetComponent<Entity>();
        this.detectedEntities.Remove(entity);
    }
}
