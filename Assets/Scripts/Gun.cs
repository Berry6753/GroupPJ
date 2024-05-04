using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float range; // �����Ÿ�
    public float accuracy;  // ��Ȯ��  
    public float fireRate;  // ����ӵ�
    public float reloadTime;    // ������ �ӵ�

    public int damage;  // ���� ������

    public int reloadBulletCount;  // �Ѿ� ������ ����
    public int currentBulletCount;  // ���� ź������ �����ִ� �Ѿ��� ����
    public int maxBulletCount;  // �ִ� ���� ���� �Ѿ� ����
    public int carryBulletCount;    // ���� �����ϰ� �ִ� �Ѿ� ����

    public float retroActionFineSignForce;  // �����ؽ��� �ݵ� ����

    public Vector3 fineSightoriginPos;

    public Animator anim;

    public ParticleSystem muzzleFlash;  // �ѱ�����

    public AudioClip fire_Sound;
    public AudioClip empty_Sound;
}
