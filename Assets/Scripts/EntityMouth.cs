using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace {
    [RequireComponent(typeof(Collider2D))]
    public class EntityMouth : MonoBehaviour {
        public Entity entity;
        
        private void OnTriggerStay2D(Collider2D other) {
            entity.onEntityCollides(other);
        }
    }
}