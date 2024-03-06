using System;
using System.Collections.Generic;
using Modules.Tween.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DefaultNamespace {
    public class Shark : Entity {
        // ########## 全局变量 ##########
        private static Vector2 reproducePosition = new Vector2(0, 0);
        private static int reproducedAmount = 0;
        
        // ########## 外部组件变量 ##########
        // Variables that are the external components / files of the entity, or other game objects
        public Tilemap tilemap; // 地图, The map
        public ViewField viewField; // 视野, The field of view
        public Transform debugDot; // 调试用的点, The point for debugging
        private SpriteRenderer waterSurface; // 水面, The water surface
        private SpriteRenderer reproduceArea; // 繁殖区域, The area for reproduction
        public GameObject sharkPrefab; // 鲨鱼预制体, The shark prefab
        private Transform sharkRoot; // 鲨鱼根节点, The root node of the shark
        
        // ########## 常数参数 ##########
        // Constants that are the parameters of the entity
        private const float swimSpeed = 3.25f; // 游泳速度, The speed of swimming
        private const float landSpeed = 1.25f; // 陆地速度, The speed of walking on the land
        private const int neededFishToReproduce = 10; // 繁殖所需的吃掉的鱼的数量, The count of the fish needed to reproduce
        private Wanderer foodWanderer; // 寻找食物的游荡者, The wanderer for finding food
        private Wanderer funWanderer; // 寻找乐趣的游荡者, The wanderer for finding fun
        private Wanderer mateAwaitWanderer; // 等待伴侣的游荡者, The wanderer for waiting for the mate
        
        // ########## 属性变量 ##########
        // Variables that are the properties of the entity
        public float currentSpeed => isInWater ? swimSpeed : landSpeed; // 当前速度, The current speed
        public int eatenFishCount = 0;// 吃掉的鱼的数量, The count of the eaten fish
        
        // ########## 状态变量 ##########
        // Variables indicate the status of the entity
        private bool isInWater; // 是否在水中, Is the entity in the water?
        public bool stupidFish; // 是否是傻鱼, Is the fish stupid?
        
        // ########## 记忆变量 ##########
        // Variables that store the memory of the entity
        private Vector2 lastWaterPosition; // 上一次有充足的水的位置, The last position of the enough water
        public Vector2 wanderPosition; // 游荡的目标位置, The target position of wandering
        private Entity prey; // 正在锁定的猎物， The prey that is being locked
        public Shark partner; // 主动找的伴侣, The mate that is found
        public Shark partnered; // 被那个伴侣找, The mate that is found
        
        protected override void start() {
            // ##### 初始化需求阈值 #####
            // Initialize the needs threshold
            maxEnergyValue = 300;
            requiredEnergyValue = 130;
            currentEnergyValue = maxEnergyValue;
            
            maxOxygenValue = 10;
            requiredOxygenValue = 4;
            currentOxygenValue = maxOxygenValue;
            
            maxHealthValue = 150;
            currentHealthValue = maxHealthValue;
            
            tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
            waterSurface = GameObject.Find("Water Surface").GetComponent<SpriteRenderer>();
            reproduceArea = GameObject.Find("Shark Reproduce Spot").GetComponent<SpriteRenderer>();
            sharkRoot = GameObject.Find("Sharks").transform;
            sharkPrefab = Resources.Load<GameObject>("Tile Textures/Entities/Shark");
            
            // ##### 初始化行为 #####
            // Initialize the behaviors
            defineAction("back-to-water", findWaterUpdate);
            defineAction("find-food", findFoodStart, findFoodUpdate, findFoodEnd);
            defineAction("go-to-reproduce", goToReproduceUpdate);
            defineAction("reproduce-await", reproduceAwaitUpdate);
            defineAction("reproducing", reproduceUpdate, reproduceEnd);
            
            foodWanderer = new Wanderer(() => {
                turnBodyTimer.run(() => { startTurnTo(wanderPosition); });
                moveForward(swimSpeed);
            }, () => {
                Entity closestEntity = findClosestPrey();

                bool foundPrey = false, resetWander = false;
                if (closestEntity != null) {
                    prey = closestEntity;
                    wanderPosition = new Vector2(0, 0);
                    foundPrey = true;
                }
                    
                // 如果实体和随机位置的距离小于0.25，则设置新的随机位置
                // If the distance between the entity and the random position is less than 0.25, set a new random position
                Vector2 dist = wanderPosition - new Vector2(transform.position.x, transform.position.y);
                if (dist.magnitude < 2) resetWander = true;

                return (foundPrey, resetWander);
            }, setRandomWanderPosition);
            
            funWanderer = new Wanderer(() => {
                turnBodyTimer.run(() => { startTurnTo(wanderPosition); });
                moveForward(swimSpeed);
            }, () => {
                bool resetWander = false;
                // 如果实体和随机位置的距离小于0.25，则设置新的随机位置
                // If the distance between the entity and the random position is less than 0.25, set a new random position
                Vector2 dist = wanderPosition - new Vector2(transform.position.x, transform.position.y);
                if (dist.magnitude < 2) resetWander = true;
                return (false, resetWander);
            }, findWaterSurfaceRandomPosition);
            
            mateAwaitWanderer = new Wanderer(() => {
                turnBodyTimer.run(() => { startTurnTo(wanderPosition); });
                moveForward(swimSpeed);
            }, () => {
                if (reproducePosition == new Vector2(0, 0)) {
                    reproducePosition = getReproducePosition();
                    setRandomReproduceWaterPosition();
                    return (false, false);
                }
                
                Shark shark = findAvailableMate();

                bool foundMate = false, resetWander = false;
                if (shark is not null) {
                    // 设置当前实体为主动追的伴侣
                    this.partner = shark;
                    shark.partnered = this;

                    // 如果主动追的鲨鱼在追...(经过了很多不同的鲨鱼)...追自己的鲨鱼，则通知所有鲨鱼开始繁殖
                    int iteration = 0;
                    bool foundTail = false;
                    Shark iteratedShark = this;
                    while (iteratedShark.partner is not null && iteration < 10) {
                        iteration++;
                        iteratedShark = iteratedShark.partner;
                        if (iteratedShark == this) {
                            foundTail = true;
                            break;
                        }
                    }
                    if (iteration >= 10) 
                        throw new Exception("Too many iterations when looping sharks");
                    
                    if (foundTail) {
                        delayedRemoveNeed("reproduce-await");
                        delayedAddNeed("reproducing");
                        Shark currentShark = this.partner;
                        while (currentShark != this) {
                            currentShark.delayedRemoveNeed("reproduce-await");
                            currentShark.delayedAddNeed("reproducing");
                            currentShark = currentShark.partner;
                        }
                    }
                    // 停止游荡
                    wanderPosition = new Vector2(0, 0);
                    foundMate = true;
                }
                else {
                    // 如果实体和随机位置的距离小于2，则设置新的随机位置
                    // If the distance between the entity and the random position is less than 0.25, set a new random position
                    Vector2 dist = wanderPosition - new Vector2(transform.position.x, transform.position.y);
                    if (dist.magnitude < 2) resetWander = true;
                }
                
                return (foundMate, resetWander);
            }, setRandomReproduceWaterPosition);
        }

        protected override void updateStatus() {
            currentEnergyValue -= Time.deltaTime / 0.75f;
            if (currentEnergyValue < 0) {
                if (partner is null && partnered is null) Ecosystem.instance.destroy(this.gameObject);
            }
            
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
            if (currentOxygenValue <= 0) currentHealthValue -= Time.deltaTime * 5f;
            
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
            if (currentEnergyValue <= 0) currentHealthValue -= Time.deltaTime * 2f;
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

            // 如果当前吃掉的鱼的数量大于繁殖所需的吃掉的鱼的数量
            if (eatenFishCount > neededFishToReproduce) {
                // 如果繁殖的位置为0，则重新生成它的位置
                if (reproducePosition == new Vector2(0, 0)) 
                    reproducePosition = getReproducePosition();
                
                // 抑制所有其他需求
                clearNeeds();
                // 添加需求“前往繁殖”
                addNeed("go-to-reproduce");
                // 重置吃掉的鱼的数量
                eatenFishCount = 0;
            }

            if (hasNeed("go-to-reproduce")) {
                clearNeeds();
                addNeed("go-to-reproduce");
            }
            if (hasNeed("reproduce-await")) {
                clearNeeds();
                addNeed("reproduce-await");
            }
            if (hasNeed("reproducing")) {
                clearNeeds();
                addNeed("reproducing");
            }
        }

        public override string getTypeName() {
            return "Shark";
        }

        // ########## 行为函数 ##########
        // Functions that define the behaviors of the entity
        
        private void reproduceAwaitUpdate() {
            if (partner is null) mateAwaitWanderer.step();
            else {
                turnBodyTimer.run(() => { startTurnTo(partner.transform.position); });
                float distance = Vector2.Distance(transform.position, partner.transform.position);
                float bonusSpeed = swimSpeed * calculateThresholdValue(distance, 2, -0.5f, 0.3f);
                moveForward(swimSpeed + bonusSpeed);
            }
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

        private readonly Timer addEnergyTimer = new Timer(1000L);
        private readonly Timer reproduceTurnTimer = new(300L);
        private void reproduceUpdate() {
            addEnergyTimer.run(() => { currentEnergyValue += 0.5f; });
            
            reproduceTurnTimer.run(() => { startTurnTo(partner.transform.position); });
            float distance = Vector2.Distance(transform.position, partner.transform.position);
            float bonusSpeed = swimSpeed * calculateThresholdValue(distance, 2, -0.4f, 0.3f);
            moveForward(swimSpeed + bonusSpeed);
            
            float random = (float) randomness.NextDouble() * 100000f / (reproducedAmount + 1);
            if (random < 30f) {
                delayedRemoveNeed("reproducing");
                Shark currentShark = this.partner;
                while (currentShark != this) {
                    currentShark.delayedRemoveNeed("reproducing");
                    currentShark = currentShark.partner;
                }
                return;
            }
            
            if (!(randomness.NextDouble() * 100000f < 60f)) return;
            
            GameObject newShark = Ecosystem.instance.instantiate(sharkPrefab, sharkRoot);
            newShark.transform.position = new Vector3(transform.position.x - 0.3f, transform.position.y - 0.3f, transform.position.z - 0.3f);
            reproducedAmount++;
        }

        private void reproduceEnd() {
            reproducePosition = new Vector2(0, 0);
            reproducedAmount = 0;
            partner = null;
            partnered = null;
        }

        private void goToReproduceUpdate() {
            wanderPosition = reproducePosition;
            turnBodyTimer.run(() => { startTurnTo(wanderPosition); });
            moveForward(swimSpeed);
            
            // 如果当前位置和繁殖位置的距离小于2，则开始等待伴侣
            if (Vector2.Distance(transform.position, reproducePosition) < 2) {
                delayedRemoveNeed("go-to-reproduce");
                delayedAddNeed("reproduce-await");
            }
        }

        public override void onIdleStart() {
            funWanderer.setup();
        }

        public override void onIdle() {
            funWanderer.step();
        }

        public override float getFoodValue() {
            return 250f;
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
        private readonly Timer findPreyTimer = new(1000L);
        private void findFoodUpdate() {
            if (wanderPosition == new Vector2(0, 0)) setRandomWanderPosition();
            // 如果当前有猎物，则朝猎物的方向移动
            // If currently there is prey, move towards the prey
            if (prey != null) {
                findPreyTimer.run(() => {
                    Entity closestPrey = findClosestPrey();
                    if (closestPrey is null) return;
                    prey = closestPrey;
                });
                turnBodyTimer.run(() => { startTurnTo(prey.transform.position); });
                moveForward(swimSpeed);
            }
            else {
                foodWanderer.step();
            }
        }
        
        private Entity findClosestPrey() {
            try {
                // 检查可视的范围内是否有食物，如果有则停止游荡
                // Check if there is food in the visible range, if so, stop wandering
                float closestDistance = float.MaxValue;
                Entity closestEntity = null;
                foreach (GameObject gobj in viewField.getDetectedObjs("Trifish")) {
                    float distance = Vector2.Distance(transform.position, gobj.transform.position);
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        Entity temp = gobj.GetComponent<Entity>();
                        if (((Trifish)temp).predator is not null) continue;
                        closestEntity = temp;
                    }
                }

                return closestEntity;
            } catch (Exception e) {
                return null;
            }
        }
        
        private Shark findAvailableMate() {
            Shark target = null;
            foreach (GameObject gobj in viewField.getDetectedObjs("Shark")) {
                if (gobj == gameObject) continue;
                Shark shark = gobj.GetComponent<Shark>();
                // 如果当前实体不需要等待伴侣，则跳过
                if (!shark.hasNeed("reproduce-await")) continue;
                // 如果当前实体已经有追他的伴侣，则跳过
                if (shark.partnered is not null) continue;

                // 查询当前正在考虑要不要追的伴侣，他在追求链中是第几条鲨鱼
                // 也就是说，如果一条鲨鱼没有追求的对象，那么他是第1条鲨鱼
                // 如果他1有，而且他1追的鲨鱼没有追求的对象，那么他1是第2条鲨鱼
                // 如果他1追的鲨鱼他2有，而且他2追的鲨鱼他3没有追求的对象，那么他1是第3条鲨鱼，他2是第2条鲨鱼
                int iteration = 0;
                bool foundTail = false;
                Shark iteratedShark = shark;
                while (iteratedShark.partner is not null) {
                    iteration++;
                    iteratedShark = iteratedShark.partner;
                    // 如果迭代的鲨鱼是当前实体，则跳过
                    if (iteratedShark == this) {
                        foundTail = true;
                        break;
                    }
                }

                if (foundTail) if (iteration < 4) continue;
                // 如果当前追求的鲨鱼所在的追求链中已经有6条鲨鱼，则跳过
                if (!foundTail) if (iteration >= 6) continue;

                target = shark;
            }

            return target;
        }

        private void findFoodEnd() {
            prey = null;
            wanderPosition = new Vector2(0, 0);
        }

        private void setRandomWanderPosition() {
            bool returnFlag = false;
            int iteration = 0;
            while (!returnFlag && iteration < 20) {
                iteration++;
                float x = (float)randomness.NextDouble() * 24 - 12;
                float y = (float)randomness.NextDouble() * 12 - 6;
                wanderPosition = new Vector2(transform.position.x + x, transform.position.y + y);
                TileBase tb = tilemap.GetTile(tilemap.WorldToCell(wanderPosition));
                if (tb is null) continue;
                if (tb.name == "water_still_0") {
                    returnFlag = true;
                    if (debugDot != null) debugDot.position = wanderPosition;
                    turnBodyTimer.run(() => {startTurnTo(wanderPosition); });
                }
            }
        }
        
        private void setRandomReproduceWaterPosition() {
            if (reproducePosition == new Vector2(0, 0)) 
                reproducePosition = getReproducePosition(); 
            if (reproducePosition == new Vector2(0, 0)) return;
            
            bool returnFlag = false;
            int iteration = 0;
            while (!returnFlag && iteration < 20) {
                iteration++;
                float x = (float)randomness.NextDouble() * 6 - 3;
                float y = (float)randomness.NextDouble() * 6 - 3;
                wanderPosition = new Vector2(reproducePosition.x + x, reproducePosition.y + y);
                TileBase tb = tilemap.GetTile(tilemap.WorldToCell(wanderPosition));
                if (tb is null) continue;
                if (tb.name == "water_still_0") {
                    returnFlag = true;
                    if (debugDot != null) debugDot.position = wanderPosition;
                    turnBodyTimer.run(() => {startTurnTo(wanderPosition); });
                }
            }
        }

        private Vector2 getReproducePosition() {
            float startX = reproduceArea.bounds.min.x;
            float startY = reproduceArea.bounds.min.y;
            float endX = reproduceArea.bounds.max.x;
            float endY = reproduceArea.bounds.max.y;
            
            float x = smoothRandomnessByTime() * (endX - startX) + startX;
            float y = smoothRandomnessByTime() * (endY - startY) + startY;
            return new Vector2(x, y);
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

        private void findWaterSurfaceRandomPosition() {
            float startX = waterSurface.bounds.min.x;
            float startY = waterSurface.bounds.min.y;
            float endX = waterSurface.bounds.max.x;
            float endY = waterSurface.bounds.max.y;
            
            float x = smoothRandomnessByTime() * (endX - startX) + startX;
            float y = smoothRandomnessByTime() * (endY - startY) + startY;
            wanderPosition = new Vector2(x, y);
        }

        private void moveForward(float speed) {
            Vector2 step = -transform.right.normalized * speed * Time.deltaTime;
            move(step);
        }

        public override void onEntityCollides(Collider2D other) {
            if (prey is null) return;
            if (!hasNeed("find-food")) return;
            if (!other.gameObject.CompareTag("Trifish")) return;
            currentEnergyValue += other.gameObject.GetComponent<Entity>().getFoodValue();
            if (other.gameObject == prey.gameObject) prey = null;
            Ecosystem.instance.destroy(other.gameObject);
            eatenFishCount++;
            if (maxEnergyValue - currentEnergyValue < 1f) 
                delayedRemoveNeed("find-food");
        }
    }
}