using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Entity : MonoBehaviour {

    public List<string> needs = new();
    private readonly Dictionary<string, Action> actionStarts = new();
    private readonly Dictionary<string, Action> actionUpdates = new();
    private readonly Dictionary<string, Action> actionEnds = new();

    protected float maxWaterValue;
    protected float requiredWaterValue;
    protected float currentWaterValue;
    
    protected float maxEnergyValue;
    protected float requiredEnergyValue;
    [SerializeField] protected float currentEnergyValue;
    
    protected float maxOxygenValue;
    protected float requiredOxygenValue;
    protected float currentOxygenValue;
    
    protected float maxHealthValue;
    [SerializeField] protected float currentHealthValue;
    
    protected Vector2Int lastIntPosition; // 上一次的更新后位置, The position of the entity in the last update

    protected List<string> needsToRemove = new();
    protected List<string> needsToAdd = new();
    
    protected System.Random randomness = new();

    protected Rigidbody2D r2d;

    protected int updateCount = 0;

    protected float smoothRandomnessByTime() {
        int seconds = (int)Time.time;
        int tenthSeconds = seconds / 10;
        float currentTenSecondRandomness = (float)new System.Random(tenthSeconds).NextDouble();
        float nextTenSecondRandomness = (float)new System.Random(tenthSeconds + 1).NextDouble();
        float lerp = Mathf.Lerp(currentTenSecondRandomness, nextTenSecondRandomness, (seconds % 10) / 10f);
        return lerp;
    }
    
    // Start is called before the first frame update
    protected void Start() {
        r2d = GetComponent<Rigidbody2D>();
        randomness = new System.Random();
        start();
        onIdleStart();
    }

    // Update is called once per frame
    protected void Update() {
        updateCount++;
        
        // 重新采样该实体的状态
        // Resample the status of this entity
        updateStatus();
        lastIntPosition = getIntPosition();
        
        // 更新该实体的需求
        // Update the needs of this entity
        updateNeeds();
        
        // 根据需求执行相应的行为
        // Perform the corresponding behavior according to the needs
        foreach (string need in needs) 
            actionUpdates[need]?.Invoke();
        
        if (needs.Count == 0) 
            onIdle();

        for (int i = needsToRemove.Count - 1; i >= 0; i--) {
            removeNeed(needsToRemove[i]);
            if (needs.Count == 0) 
                onIdleStart();
        }
        for (int i = needsToAdd.Count - 1; i >= 0; i--) {
            addNeed(needsToAdd[i]);
            needsToAdd.RemoveAt(i);
        }
    }

    protected void defineAction(string need, Action start, Action update, Action end) {
        actionStarts.Add(need, start);
        actionUpdates.Add(need, update);
        actionEnds.Add(need, end);
    }
    
    protected void defineAction(string need, Action update, Action end) {
        defineAction(need, () => { }, update, end);
    }

    protected void defineAction(string need, Action update) {
        defineAction(need, () => { }, update, () => { });
    }

    protected void addNeed(string need) {
        if (needs.Contains(need)) return;
        // Debug.Log($"Fish: {ToString()}, Need: {need}");
        needs.Add(need);
        actionStarts[need]?.Invoke();
    }
    protected void delayedAddNeed(string need) {
        if (needs.Contains(need)) return;
        needsToAdd.Add(need);
    }

    protected void clearNeeds() {
        needs.Clear();
    }
    
    private void removeNeed(string need) {
        if (!needs.Contains(need)) return;
        if (!needsToRemove.Contains(need)) return;
        needs.Remove(need);
        actionEnds[need]?.Invoke();
        needsToRemove.Remove(need);
    }
    
    protected void delayedRemoveNeed(string need) {
        if (!needs.Contains(need)) return;
        needsToRemove.Add(need);
    }

    protected bool hasNeed(string need) {
        return needs.Contains(need);
    }

    protected int needCount() {
        return needs.Count;
    }

    protected abstract void start();

    protected abstract void updateStatus();

    protected abstract void updateNeeds();

    public abstract string getTypeName();
    
    public abstract void onIdleStart();
    public abstract void onIdle();

    public void move(float x, float y) {
        // r2d.MovePosition(new Vector2(transform.position.x, transform.position.y) + 
        //                  new Vector2(x, y));
        r2d.velocity += new Vector2(x, y);
    }
    
    public void move(Vector2 vector2) {
        move(vector2.x, vector2.y);
    }

    protected Vector2Int getIntPosition() {
        return new Vector2Int((int) transform.position.x, (int) transform.position.y);
    }

    public abstract float getFoodValue();

    public abstract void onEntityCollides(Collider2D other);
}
