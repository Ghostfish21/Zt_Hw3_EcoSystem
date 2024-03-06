using System;
using Modules.Tween.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace DefaultNamespace {
    public class Landmine : Entity {
        
        // ########## 外部组件变量 ##########
        // Variables that are the external components / files of the entity, or other game objects
        public Tilemap tilemap; // 地图, The map
        public ViewField viewField; // 视野, The field of view
        public ViewField mouseSensor; // 鼠标感应器, The mouse sensor
        public Transform debugDot; // 调试用的点, The point for debugging
        
        // ########## 常数参数 ##########
        // Constants that are the parameters of the entity
        private const float swimSpeed = 2.6f; // 游泳速度, The speed of swimming
        private const float landSpeed = 2.5f; // 陆地速度, The speed of walking on the land
        private const float jumpSpeed = 35f;
        private const float scale = 0.331742f;
        
        // ########## 属性变量 ##########
        // Variables that are the properties of the entity
        public float currentSpeed => isInWater ? swimSpeed : landSpeed; // 当前速度, The current speed
        
        // ########## 状态变量 ##########
        // Variables indicate the status of the entity
        public bool isInWater; // 是否在水中, Is the entity in the water?
        public bool isNearWaterSurfaceInWater; // 是否在水面上, Is the entity on the water?
        public bool isNearWaterSurfaceInAir; // 是否在水面上, Is the entity on the water?
        private bool isOnLand => !isInWater && !isNearWaterSurfaceInWater; // 是否在陆地上, Is the entity on the land?
        public bool stupidFish; // 是否是傻鱼, Is the fish stupid?
        
        // ########## 记忆变量 ##########
        // Variables that store the memory of the entity
        public Vector2 wanderPosition; // 游荡的目标位置, The target position of wandering
        private Entity prey; // 正在锁定的猎物， The prey that is being locked
        public Landmine companion; // 锁定的跟随伴侣, The locked companion to follow
        
        protected override void start() {
            // ##### 初始化需求阈值 #####
            // Initialize the needs threshold
            maxEnergyValue = 300;
            requiredEnergyValue = 180;
            currentEnergyValue = 225;
            
            maxOxygenValue = 10;
            requiredOxygenValue = 3;
            currentOxygenValue = maxOxygenValue;
            
            maxHealthValue = 550;
            currentHealthValue = maxHealthValue;
            
            tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
            
            setWanderPosition();
            
            // ##### 初始化行为 #####
            // Initialize the behaviors
            defineAction("find-food", findFoodUpdate);
        }

        private SpriteRenderer huntArea;
        private void findWanderPositionForHunt() {
            if (huntArea is null) huntArea = GameObject.Find("Landmine Hunt Area").GetComponent<SpriteRenderer>();
            
            float startX = huntArea.bounds.min.x;
            float startY = huntArea.bounds.min.y;
            float endX = huntArea.bounds.max.x;
            float endY = huntArea.bounds.max.y;
            
            float x = smoothRandomnessByTime() * (endX - startX) + startX;
            float y = smoothRandomnessByTime() * (endY - startY) + startY;
            wanderPosition = new Vector2(x, y);
        }

        protected override void updateStatus() {
            currentEnergyValue -= Time.deltaTime / 3.75f;
            if (currentEnergyValue < 0) Ecosystem.instance.destroy(this.gameObject);
            
            // ##### 更新是否在水中,或者水面上的状态 #####
            // Update the status of whether the entity is in the water
            TileBase tile = tilemap.GetTile(tilemap.WorldToCell(transform.position));
            if (tile is not null) {
                isInWater = tile.name == "water_still_0";
            }
            else {
                isInWater = false;
                isNearWaterSurfaceInWater = false;
            }

            if (isInWater) {
                isNearWaterSurfaceInAir = false;
                Tile tileAbove = tilemap.GetTile(new Vector3Int((int)transform.position.x, (int)transform.position.y + 1, 0)) as Tile;
                if (tileAbove is null) isNearWaterSurfaceInWater = true;
                else isNearWaterSurfaceInWater = false;
            }
            else {
                Tile tileBelow = tilemap.GetTile(new Vector3Int((int)transform.position.x, (int)(transform.position.y -
                    0.215f), 0)) as Tile;
                if (tileBelow is null) isNearWaterSurfaceInAir = false;
                else if (tileBelow.name == "water_still_0") isNearWaterSurfaceInAir = true;
                else isNearWaterSurfaceInAir = false;
            }

            if (!isInWater && !isNearWaterSurfaceInAir) {
                r2d.gravityScale = 1;
                r2d.angularDrag = 10f;
                r2d.drag = 3f;  
            }
            else {
                r2d.gravityScale = 0f;
                r2d.angularDrag = 10f;
                r2d.drag = 3f;  
            }
            
            // ##### 更新当前能量值 #####
            // Update the current energy value
            // 如果当前能量为0，则开始减少生命值
            // If the current energy is 0, start to reduce the health value
            if (currentEnergyValue <= 0) currentHealthValue -= Time.deltaTime;
        }

        private readonly Timer findFoodTimer = new(3000L);
        protected override void updateNeeds() {
            // 如果当前能量值小于开始饥饿所需能量值，则添加需求“寻找食物”
            // If the current energy value is less than the required energy value to start hunger, add the need "find-food"
            if (currentEnergyValue < requiredEnergyValue) { addNeed("find-food"); } 
        }

        private void findFoodUpdate() {
            try {
                findFoodTimer.run(() => {
                    GameObject closestFish = null;
                    float closestDist = float.MaxValue;
                    foreach (GameObject gobj in viewField.getDetectedObjs("Trifish")) {
                        float distToFish = Vector2.Distance(gobj.transform.position, transform.position);
                        if (distToFish < closestDist) {
                            closestDist = distToFish;
                            closestFish = gobj;
                        }
                    }

                    if (closestFish is not null)
                        prey = closestFish.GetComponent<Entity>();
                });

                if (prey == null) {
                    if (wanderPosition == new Vector2(0, 0))
                        findWanderPositionForHunt();
                    float dist = Vector2.Distance(wanderPosition, transform.position);
                    if (dist < 1.5f) wanderPosition = new Vector2(0, 0);
                    else {
                        turnBodyTimer.run(() => { startTurnTo(wanderPosition); });
                        moveForward(swimSpeed);
                    }
                }
                else {
                    // 获取 mouse sensor 和 本体的相对距离
                    Vector3 relativePos = transform.position - mouseSensor.transform.position;
                    // 前往 猎物 + 相对距离 的位置
                    Vector2 target = prey.transform.position + relativePos * 2f;
                    turnBodyTimer.run(() => { startTurnTo(target); });
                    moveForward(swimSpeed);
                }

                if (mouseSensor.isDetected("Trifish", prey.gameObject)) {
                    // 朝mouse sensor的方向以 2f * jumpSpeed 的速度移动
                    Vector2 direction = mouseSensor.transform.position - transform.position;
                    move (direction.normalized * 2f * jumpSpeed * Time.deltaTime);
                }
            }
            catch (Exception e) { prey = null; }
        }

        public override string getTypeName() {
            return "Landmine";
        }

        // ########## 行为函数 ##########
        // Functions that define the behaviors of the entity

        public override void onIdleStart() { }

        public override void onIdle() {
            // 如果当前没有需求，只有可能是游荡或者呆呆地
            // 如果现在在水里，无论哪种情况都必然会上浮
            // 如果离水面近，垂直上浮就可以，如果不进的话可以直接朝方向往上游
            float dist = Vector2.Distance(wanderPosition, transform.position);
            if (isInWater) {
                Vector2 forwardPos = new Vector2(transform.position.x - 5f, transform.position.y);
                if (wanderPosition == new Vector2(0, 0)) {
                    turnBodyTimer.run(() => { startTurnTo(forwardPos); });
                    move(0, swimSpeed * Time.deltaTime);
                }
                else {
                    if (isNearWaterSurfaceInWater) {
                        turnBodyTimer.run(() => { startTurnTo(forwardPos); });
                        move(0, swimSpeed * Time.deltaTime);
                    }
                    else {
                        turnBodyTimer.run(() => { startTurnTo(wanderPosition); });
                        moveForward(swimSpeed);
                    }
                }
            }
            // 如果在陆地，则将朝向调成面朝水平的方向，然后直接朝目标位置游荡
            else {
                turnBodyTimer.run(() => { startTurnTo(new Vector2(wanderPosition.x, transform.position.y)); });
                if (checkIfBlockedByTile()) move(0, jumpSpeed * Time.deltaTime);
                moveForward(landSpeed);
            }

            if (isNearWaterSurfaceInAir) {
                if (mouseSensor.getDetectedObjs("Trifish").Count > 0) 
                    move(0, -2.5f * jumpSpeed * Time.deltaTime);
            }
            
            if (wanderPosition == new Vector2(0, 0)) {
                float rand = (float)randomness.NextDouble();
                if (rand * 100f < 1) setWanderPosition();
            }
            
            if (wanderPosition != new Vector2(0, 0)) 
                if (dist < 1.5f) wanderPosition = new Vector2(0, 0);
        }
        
        private bool checkIfBlockedByTile() {
            return false;
        }

        public override float getFoodValue() {
            return 40;
        }

        private float calculateThresholdValue(float inputValue, float threshold, float min, float max) {
            float result = 0;
            
            if (inputValue < threshold) {
                // inputValue越小于threshold，返回值越接近-1
                result = -1 * ((threshold - inputValue) / threshold);
            }
            if (inputValue > threshold) {
                // inputValue越大于threshold，返回值越接近1
                result = (inputValue - threshold) / threshold;
            }

            if (result < min) result = min;
            if (result > max) result = max;
            
            // 如果inputValue等于threshold，返回0
            return result;
        }

        private readonly Timer turnBodyTimer = new(300L);

        private SpriteRenderer waterSurface;
        private void setWanderPosition() {
            if (waterSurface is null) waterSurface = GameObject.Find("Water Surface Landmines").GetComponent<SpriteRenderer>();
            float randX = (float) randomness.NextDouble() * waterSurface.bounds.size.x + waterSurface.bounds.min.x;
            float randY = (float) randomness.NextDouble() * waterSurface.bounds.size.y + waterSurface.bounds.min.y;
            wanderPosition = new Vector2(randX, randY);
        }
        
        private TweenBuilder previousRotationTween;
        private TweenBuilder previousScaleTween;
        // private void startTurnTo(Vector2 target) {
        //     float rawAngle = findSmallestAngle(r2d.rotation, calculateRotation(target));
        //     float angle = positive360Angle(rawAngle);
        //     int direction = 1;
        //     if (angle is < 270 and >= 90) direction = -1;
        //     
        //     if (previousRotationTween is not null) 
        //         previousRotationTween.addProperty("is-cancelled", "1");
        //
        //     float finalAngle = angle;
        //     if (transform.localScale.x * direction < 0) {
        //         if (previousScaleTween is not null) 
        //             previousScaleTween.addProperty("is-cancelled", "1");
        //         
        //         TweenBuilder scaleTween = new TweenBuilder().setProperties($@"
        //             start-value: {transform.localScale.x};
        //             end-value: {scale * direction};
        //             duration: 0.25;
        //             ease: InOutSine;
        //             is-cancelled: 0;
        //         ");
        //         previousScaleTween = scaleTween;
        //         scaleTween.setSetter(x => {
        //             if (scaleTween.getProperty("is-cancelled") != "0") { return; }
        //             try {
        //                 transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
        //             }
        //             catch (Exception e) {
        //                 scaleTween.addProperty("is-cancelled", "1");
        //             }
        //         });
        //         scaleTween.register<float>();
        //     }
        //     if (transform.localScale.x < 0) finalAngle = findSmallestAngle(r2d.rotation, mirrorAngle(angle));
        //
        //     TweenBuilder rotationTween = new TweenBuilder().setProperties($@"
        //             start-value: {r2d.rotation};
        //             end-value: {finalAngle};
        //             duration: 0.25;
        //             ease: InOutSine;
        //             is-cancelled: 0;
        //         ");
        //     previousRotationTween = rotationTween;
        //     rotationTween.setSetter(x => {
        //         if (rotationTween.getProperty("is-cancelled") != "0") return;
        //         try { r2d.MoveRotation(x); } 
        //         catch (Exception e) { rotationTween.addProperty("is-cancelled", "1"); }
        //     });
        //     rotationTween.register<float>();
        // }
        
        private void startTurnTo(Vector2 target) {
            if (previousRotationTween is not null) 
                previousRotationTween.addProperty("is-cancelled", "1");
            TweenBuilder rotationTween = new TweenBuilder().setProperties($@"
                    start-value: {r2d.rotation};
                    end-value: {findSmallestAngle(r2d.rotation, calculateRotation(target))};
                    duration: 0.25;
                    ease: InOutSine;
                    is-cancelled: 0;
                ");
            previousRotationTween = rotationTween;
            rotationTween.setSetter(x => {
                if (rotationTween.getProperty("is-cancelled") != "0") return;
                r2d.MoveRotation(x);
            });
            rotationTween.register<float>();
        }

        private float calculateRotation(Vector2 target) {
            Vector2 direction = target - new Vector2(transform.position.x, transform.position.y);
            float targetRotation = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
            return targetRotation;
        }

        private float findSmallestAngle(float fromAngle, float toAngle) {
            float angleDist = Mathf.Abs(toAngle - fromAngle);
            float counterAngleDist = 360 - angleDist;
            float counterToAngle = toAngle;
            if (toAngle > fromAngle) counterToAngle -= 360;
            else counterToAngle += 360;
            return angleDist < counterAngleDist ? toAngle : counterToAngle;
        }

        private void moveForward(float speed) {
            Vector2 step = -transform.right.normalized * speed * Time.deltaTime;
            if (transform.localScale.x < 0) step *= -1;
            move(step);
        }

        private Timer eatFishTimer = new(1000L);
        public override void onEntityCollides(Collider2D other) {
            if (needCount() == 0 || hasNeed("find-food")) {
                if (other.gameObject.CompareTag("Trifish")) {
                    if (randomness.NextDouble() < 0.03) {
                        eatFishTimer.run(() => {
                            currentEnergyValue += other.gameObject.GetComponent<Entity>().getFoodValue();
                            Ecosystem.instance.destroy(other.gameObject);
                        });
                    }
                }
            } else if (hasNeed("find-food")) {
                if (!other.gameObject.CompareTag("Trifish")) return;
                try {
                    if (other.gameObject != prey.gameObject) return;
                    currentEnergyValue += other.gameObject.GetComponent<Entity>().getFoodValue();
                    Ecosystem.instance.destroy(other.gameObject);
                }
                catch (Exception e) { prey = null; }
            }
        }

        private float positive360Angle(float abnormalAngle) {
            float result = abnormalAngle % 360;
            if (result < 0) result += 360;
            return result;
        }

        private float mirrorAngle(float rawAngle) {
            return positive360Angle(rawAngle - 180);
        }
    }
}