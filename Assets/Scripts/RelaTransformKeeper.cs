using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class RelaTransformKeeper : MonoBehaviour {
    public Transform alignFrom;      // 指定要跟随的父 GameObject
    private Vector3 offset;       // 初始的相对位置
    private Quaternion offsetRotation;   // 初始的相对旋转

    void Start() {
        // 记录初始的相对位置
        offset = transform.position - alignFrom.position;
        // 记录初始的相对旋转
        offsetRotation = Quaternion.Inverse(alignFrom.rotation) * transform.rotation;
    }

    void Update() {
        // 子对象位置等于父对象位置加上初始的相对位置 
        transform.position = alignFrom.position + offset;
        // 子对象的旋转等于父对象的旋转乘以初始的相对旋转
        transform.rotation = alignFrom.rotation * offsetRotation;
    }
}