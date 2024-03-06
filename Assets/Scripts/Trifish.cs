using System;
using Modules.Tween.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DefaultNamespace {
    public class Trifish : Entity {
        
        // ########## 外部组件变量 ##########
        // Variables that are the external components / files of the entity, or other game objects
        public Tilemap tilemap; // 地图, The map
        public ViewField viewField; // 视野, The field of view
        public Transform debugDot; // 调试用的点, The point for debugging
        
        // ########## 常数参数 ##########
        // Constants that are the parameters of the entity
        private const float swimSpeed = 2.5f; // 游泳速度, The speed of swimming
        private const float landSpeed = 0.5f; // 陆地速度, The speed of walking on the land
        private Wanderer foodWanderer; // 寻找食物的游荡者, The wanderer for finding food
        private Wanderer companionWanderer; // 寻找伴侣的游荡者, The wanderer for finding companion
        
        // ########## 属性变量 ##########
        // Variables that are the properties of the entity
        public float currentSpeed => isInWater ? swimSpeed : landSpeed; // 当前速度, The current speed
        
        // ########## 状态变量 ##########
        // Variables indicate the status of the entity
        private bool isInWater; // 是否在水中, Is the entity in the water?
        public bool stupidFish; // 是否是傻鱼, Is the fish stupid?
        
        // ########## 记忆变量 ##########
        // Variables that store the memory of the entity
        private Vector2 lastWaterPosition; // 上一次有充足的水的位置, The last position of the enough water
        public Vector2 wanderPosition; // 游荡的目标位置, The target position of wandering
        private Entity prey; // 正在锁定的猎物， The prey that is being locked
        public Trifish companion; // 锁定的跟随伴侣, The locked companion to follow
        private Trifish secondCompanion; // 锁定的第二个跟随伴侣, The locked second companion to follow
        public Entity predator; // 锁定的猎食者, The locked predator
        
        protected override void start() {
            // ##### 初始化需求阈值 #####
            // Initialize the needs threshold
            maxEnergyValue = 100;
            requiredEnergyValue = 35;
            currentEnergyValue = maxEnergyValue;
            
            maxOxygenValue = 10;
            requiredOxygenValue = 4;
            currentOxygenValue = maxOxygenValue;
            
            maxHealthValue = 20;
            currentHealthValue = maxHealthValue;
            
            tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
            
            // ##### 初始化行为 #####
            // Initialize the behaviors
            defineAction("back-to-water", findWaterUpdate);
            defineAction("find-food", findFoodStart, findFoodUpdate, findFoodEnd);
            defineAction("escape", escapeUpdate, () => { predator = null; });
            
            Timer findFoodTimer = new(500L);
            foodWanderer = new Wanderer(() => {
                turnBodyTimer.run(() => { startTurnTo(wanderPosition); });
                moveForward(swimSpeed);
            }, () => {
                // 检查可视的范围内是否有食物，如果有则停止游荡
                // Check if there is food in the visible range, if so, stop wandering
                float closestDistance = float.MaxValue;
                Entity closestEntity = null;
                findFoodTimer.run(() => {
                    foreach (GameObject gobj in viewField.getDetectedObjs("Seaweed")) {
                        float distance = Vector2.Distance(transform.position, gobj.transform.position);
                        if (distance < closestDistance) {
                            closestDistance = distance;
                            closestEntity = gobj.GetComponent<Entity>();
                        }
                    }
                });

                bool foundPrey = false, resetWander = false;
                if (closestEntity != null) {
                    prey = closestEntity;
                    wanderPosition = new Vector2(0, 0);
                    foundPrey = true;
                }
                    
                // 如果实体和随机位置的距离小于0.25，则设置新的随机位置
                // If the distance between the entity and the random position is less than 0.25, set a new random position
                Vector2 dist = wanderPosition - new Vector2(transform.position.x, transform.position.y);
                if (dist.magnitude < 0.25) resetWander = true;

                return (foundPrey, resetWander);
            }, setRandomWanderPositionDown);

            companionWanderer = new Wanderer(() => {
                turnBodyTimer.run(() => { startTurnTo(wanderPosition); });
                moveForward(swimSpeed);
            }, () => {
                bool companionFound = false, resetWander = false;

                Entity[] closestEntity = findClosestCompanion();
                
                if (closestEntity[0] != null) {
                    companion = (Trifish)closestEntity[0];
                    wanderPosition = new Vector2(0, 0);
                    companionFound = true;
                }
                if (closestEntity[1] != null) 
                    secondCompanion = (Trifish)closestEntity[1];
                
                Vector2 dist = wanderPosition - new Vector2(transform.position.x, transform.position.y);
                if (dist.magnitude < 0.25) resetWander = true;
                
                return (companionFound, resetWander);
            }, setRandomWanderPositionUp);
        }

        private Entity[] findClosestCompanion() {
            float closestDistance = float.MaxValue;
            float secondDistance = float.MaxValue;
            Entity closestEntity = null;
            Entity secondEntity = null;

            if (viewField.getDetectedObjs("Trifish").Count >= 10) {
                companion = null;
                secondCompanion = null;
                setRandomWanderPositionDown();
                return new Entity[] { null, null };
            }
            
            foreach (GameObject gobj in viewField.getDetectedObjs("Trifish")) {
                try {
                    if (gobj == this.gameObject) continue;
                    
                    float distance = Vector2.Distance(transform.position, gobj.transform.position);
                    if (distance < closestDistance) {
                        secondDistance = closestDistance;
                        secondEntity = closestEntity;
                        closestDistance = distance;
                        closestEntity = gobj.GetComponent<Entity>();
                    }
                    else if (distance < secondDistance) {
                        secondDistance = distance;
                        secondEntity = gobj.GetComponent<Entity>();
                    }
                    else continue;
                    
                    Trifish trifish = (Trifish)gobj.GetComponent<Entity>();
                    int iteration = 0;
                    while (trifish.companion is not null) {
                        trifish = trifish.companion;

                        iteration++;
                        if (iteration > 10) break;

                        if (trifish != this) continue;
                        break;
                    }
                }
                catch (Exception e) {
                    // ignored
                }
            }
            return new []{closestEntity, secondEntity};
        }

        protected override void updateStatus() {
            currentEnergyValue -= Time.deltaTime / 5f;
            if (currentEnergyValue < 0) Ecosystem.instance.destroy(this.gameObject);
            
            // ##### 更新是否在水中水的状态 #####
            // Update the status of whether the entity is in the water
            TileBase tile = tilemap.GetTile(tilemap.WorldToCell(transform.position));
            if (tile is not null) {
                isInWater = tile.name == "water_still_0";
            } else isInWater = false;
            if (!isInWater) r2d.gravityScale = 1;
            else r2d.gravityScale = 0;
            
            // 根据是否在水中更新当前氧气值和生命值
            // Update the current oxygen value and health value according to whether the entity is in the water
            if (!isInWater) currentOxygenValue -= Time.deltaTime;
            else currentOxygenValue = maxOxygenValue;
            if (currentOxygenValue <= 0) currentHealthValue -= Time.deltaTime;
            
            // 如果在水中，则检测生物附近3*3的区域是否至少有六格水，如果有，则更新lastWaterPosition
            // If in the water, check whether there are at least six cells of water in the 3*3 area near the entity, if so, update lastWaterPosition
            if (isInWater && lastIntPosition != getIntPosition()) {
                Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
                int waterCount = 0;
                for (int x = -1; x <= 1; x++) 
                    for (int y = -1; y <= 1; y++) {
                        TileBase tb =
                            tilemap.GetTile(new Vector3Int(cellPosition.x + x, cellPosition.y + y, cellPosition.z));
                        if (tb is null) continue;
                        if (tb.name == "water_still_0") 
                        waterCount++;
                    }
                if (waterCount >= 6) lastWaterPosition = new Vector2(cellPosition.x, cellPosition.y); 
            }
            
            // ##### 更新当前能量值 #####
            // Update the current energy value
            // 如果当前能量为0，则开始减少生命值
            // If the current energy is 0, start to reduce the health value
            if (currentEnergyValue <= 0) currentHealthValue -= Time.deltaTime;
        }

        private readonly Timer findFoodTimer = new(3000L);
        protected override void updateNeeds() {
            // 如果当前不在水中，添加需求“回到水中”，并提前终止更新需求的方法
            // If not in the water, add the need "back-to-water"
            if (!isInWater) {
                addNeed("back-to-water");
                return;
            }
            else { delayedRemoveNeed("back-to-water"); }
            
            // 如果当前能量值小于开始饥饿所需能量值，则添加需求“寻找食物”
            // If the current energy value is less than the required energy value to start hunger, add the need "find-food"
            if (currentEnergyValue < requiredEnergyValue) { addNeed("find-food"); } 
            else if (currentEnergyValue != maxEnergyValue) {
                findFoodTimer.run(() => {
                    // 生物选择进食的概率与当前能量值与开始饥饿所需能量值的差值成正比，差值越大，进食概率越大
                    // The probability of the entity to eat is proportional to the difference between the current energy value and the required energy value to start hunger. The greater the difference, the greater the probability of eating
                    float possibleNeededEnergy = maxEnergyValue - requiredEnergyValue;
                    float neededEnergy = maxEnergyValue - currentEnergyValue;
                    if (randomness.NextDouble() < neededEnergy / possibleNeededEnergy) { addNeed("find-food"); }
                });
            }
            
            // 猎食者检查，如果有的话则添加逃离需求，如果没有的话移除逃离需求
            // Predator check, if there is, add escape need
            bool hasPredator = false;
            if (viewField.getDetectedObjs("Shark").Count > 0) {
                addNeed("escape");
                hasPredator = true;
            }

            foreach (GameObject go in viewField.getDetectedObjs("Landmine")) {
                Landmine temp = go.GetComponent<Landmine>();
                if (temp.isNearWaterSurfaceInAir) continue;
                addNeed("escape");
                hasPredator = true;
                break;
            }

            if (!hasPredator) Invoke(nameof(temp), 5);
        }

        private void temp() {
            delayedRemoveNeed("escape");
        }

        public override string getTypeName() {
            return "Trifish";
        }

        // ########## 行为函数 ##########
        // Functions that define the behaviors of the entity

        public override void onIdleStart() {
            companionWanderer.setup();
        }

        private Timer findCompanionTimer = new(500L);
        public override void onIdle() {
            if (companion is null) {
                companionWanderer.step();
            }
            else {
                try {
                    findCompanionTimer.run(() => {
                        Entity[] closestEntity = findClosestCompanion();

                        if (closestEntity[0] is not null) {
                            companion = (Trifish)closestEntity[0];
                            secondCompanion = null;
                        }

                        if (closestEntity[1] is not null) secondCompanion = (Trifish)closestEntity[1];
                    });
                    
                    Vector2 targetPos = new Vector2(0.48925f, 0.1512f);
                    if (companion is not null && secondCompanion is not null)
                        targetPos = (companion.transform.position + secondCompanion.transform.position) / 2;
                    else if (companion is not null) targetPos = companion.transform.position;

                    if (targetPos == new Vector2(0.48925f, 0.1512f)) return;
                    turnBodyTimer.run(() => { startTurnTo(targetPos); });
                    float distance = Vector2.Distance(transform.position, targetPos);
                    float bonusSpeed = swimSpeed * calculateThresholdValue(distance, 2, -1, 0.3f);
                    moveForward(swimSpeed + bonusSpeed);
                } catch (Exception e) {
                    // ignored
                }
            }
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
        
        private void findWaterUpdate() {
            // 如果当前在水中，则停止寻找水的行为并移除需求。否则，朝水的方向移动
            // If currently in the water, stop the behavior of finding water and remove the need. Otherwise, move towards the water
            if (isInWater) { delayedRemoveNeed("back-to-water"); } 
            else {
                Vector2 direction = lastWaterPosition - new Vector2(transform.position.x, transform.position.y);
                move(direction.normalized * landSpeed * Time.deltaTime);
            }
        }

        private void findFoodStart() {
            // 如果生物周围通过 Trigger碰撞想（视野范围） 检测到食物，则标记食物的实体实例，如果找到多个食物，则选择最近的食物，如果没有找到食物，则开始移动到随机位置
            // If the entity detects food around it through Trigger collision (field of view), mark the entity instance of the food. If multiple foods are found,
            // select the nearest food. If no food is found, remove the need and move to a random position
            foodWanderer.setup();
        }

        private readonly Timer turnBodyTimer = new(300L);
        private void findFoodUpdate() {
            // 如果当前有猎物，则朝猎物的方向移动
            // If currently there is prey, move towards the prey
            if (prey != null) {
                turnBodyTimer.run(() => {
                    startTurnTo(prey.transform.position);
                });
                moveForward(swimSpeed);
            }
            else {
                // 否则，朝随机位置移动，如果中途检测到食物，则停止游荡并标记猎物并朝它开始移动
                // Otherwise, move towards the random position
                foodWanderer.step();
            }
        }
        
        private void findFoodEnd() {
            prey = null;
            wanderPosition = new Vector2(0, 0);
        }

        private void escapeUpdate() {
            // 遍历当前可视范围内的所有猎食者，如果有猎食者，则朝猎食者的反方向移动，如果有多个猎食者，则向最近的猎食者的反方向移动
            // Traverse all predators in the current visible range. If there is a predator, move in the opposite direction of the predator. If there are multiple predators, move in the opposite direction of the nearest predator
            float closestDistance = float.MaxValue;
            Entity closestEntity = null;
            foreach (GameObject gobj in viewField.getDetectedObjs("Shark")) {
                float distance = Vector2.Distance(transform.position, gobj.transform.position);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestEntity = gobj.GetComponent<Entity>();
                }
            }
            foreach (GameObject gobj in viewField.getDetectedObjs("Landmine")) {
                float distance = Vector2.Distance(transform.position, gobj.transform.position);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    Landmine temp = gobj.GetComponent<Landmine>();
                    if (temp.isNearWaterSurfaceInAir) continue;
                    closestEntity = temp;
                }
            }
            if (closestEntity != null) {
                predator = closestEntity;
                companion = null; secondCompanion = null;
            }
            
            if (predator is null) return;
            Vector2 direction = transform.position - predator.transform.position;
            turnBodyTimer.run(() => { startTurnTo(new Vector2(transform.position.x, transform.position.y) + direction); });
            moveForward(swimSpeed * 1.65f);
        }

        private void setRandomWanderPositionDown() {
            bool returnFlag = false;
            int iteration = 0;
            while (!returnFlag && iteration < 20) {
                iteration++;
                float x = (float)randomness.NextDouble() * 12 - 6;
                float y = (float)randomness.NextDouble() * 6 - 3f;
                wanderPosition = new Vector2(transform.position.x + x, transform.position.y + y);
                TileBase tb = tilemap.GetTile(tilemap.WorldToCell(wanderPosition));
                if (tb is null) continue;
                if (tb.name == "water_still_0") {
                    returnFlag = true;
                    if (debugDot != null) debugDot.position = wanderPosition;
                    turnBodyTimer.run(() => { startTurnTo(wanderPosition); });
                }
            }
        }
        
        private void setRandomWanderPositionUp() {
            bool returnFlag = false;
            int iteration = 0;
            while (!returnFlag && iteration < 20) {
                iteration++;
                float x = (float)randomness.NextDouble() * 12 - 6;
                float y = (float)randomness.NextDouble() * 6 - 3f;
                wanderPosition = new Vector2(transform.position.x + x, transform.position.y + y);
                TileBase tb = tilemap.GetTile(tilemap.WorldToCell(wanderPosition));
                if (tb is null) continue;
                if (tb.name == "water_still_0") {
                    returnFlag = true;
                    if (debugDot != null) debugDot.position = wanderPosition;
                    turnBodyTimer.run(() => { startTurnTo(wanderPosition); });
                }
            }
        }

        private TweenBuilder previousRotationTween;
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
                try {
                    r2d.MoveRotation(x);
                } catch (Exception e) {
                    rotationTween.addProperty("is-cancelled", "1");
                }
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
            if (stupidFish) {
                throw new NullReferenceException("Where am I");
            }
            Vector2 step = -transform.right.normalized * speed * Time.deltaTime;
            move(step);
        }

        public override void onEntityCollides(Collider2D other) {
            if (prey is null) return;
            if (other.gameObject != prey.gameObject) return;
            currentEnergyValue += Time.deltaTime * 60;
            if (Mathf.Abs(currentEnergyValue - maxEnergyValue) < 1f) 
                delayedRemoveNeed("find-food");
        }
    }
}