using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class RelaTransformKeeper : MonoBehaviour {
    public Transform alignFrom;      // ָ��Ҫ����ĸ� GameObject
    private Vector3 offset;       // ��ʼ�����λ��
    private Quaternion offsetRotation;   // ��ʼ�������ת

    void Start() {
        // ��¼��ʼ�����λ��
        offset = transform.position - alignFrom.position;
        // ��¼��ʼ�������ת
        offsetRotation = Quaternion.Inverse(alignFrom.rotation) * transform.rotation;
    }

    void Update() {
        // �Ӷ���λ�õ��ڸ�����λ�ü��ϳ�ʼ�����λ�� 
        transform.position = alignFrom.position + offset;
        // �Ӷ������ת���ڸ��������ת���Գ�ʼ�������ת
        transform.rotation = alignFrom.rotation * offsetRotation;
    }
}