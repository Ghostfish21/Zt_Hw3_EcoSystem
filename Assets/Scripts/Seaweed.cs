using UnityEngine;

namespace DefaultNamespace {
    [RequireComponent(typeof(Collider2D))]
    public class Seaweed : Entity {
        protected override void start() {
            // ##### 初始化需求阈值 #####
            // Initialize the needs threshold
            maxEnergyValue = 100;
            requiredEnergyValue = 1;
            currentEnergyValue = maxEnergyValue;
            
            maxOxygenValue = 100;
            requiredOxygenValue = 1;
            currentOxygenValue = maxOxygenValue;
            
            maxHealthValue = 100;
            currentHealthValue = maxHealthValue;
        }

        protected override void updateStatus() {
            
        }

        protected override void updateNeeds() {
            
        }

        public override string getTypeName() {
            return "Seaweed";
        }

        public override void onIdleStart() {
            
        }

        public override void onIdle() {
            
        }

        public override float getFoodValue() {
            return 0;
        }

        public override void onEntityCollides(Collider2D other) {
            throw new System.NotImplementedException();
        }
    }
}