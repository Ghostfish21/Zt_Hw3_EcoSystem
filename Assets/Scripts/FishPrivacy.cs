using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class FishPrivacy : MonoBehaviour {
    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.CompareTag("Entity")) {
            Entity entity = other.gameObject.GetComponent<Entity>();
            if (entity.getTypeName() == "Trifish") {
                Trifish trifish = (Trifish) entity;
                Vector2 pushAway = trifish.transform.position - transform.position;
                pushAway = pushAway.normalized;
                Vector2 finalVelo = pushAway * trifish.currentSpeed * Time.deltaTime * 100;
                Debug.Log(finalVelo.x + ", " + finalVelo.y);
                trifish.move(finalVelo);
            }
        }
    }
}
