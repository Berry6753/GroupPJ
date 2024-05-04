using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGunController : MonoBehaviour
{
    [SerializeField] private Gun gun;   // ���� ��
    [SerializeField] private GameObject enemy;

    private float currentFireRate;  // ���� �ӵ� ���

    private AudioSource audioSource;    // ȿ����

    private RaycastHit hit; // ������ �浹 ����



    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        GunFireRateCalc();
        Shoot();
    }

    // ����ӵ� ����
    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
        {
            currentFireRate -= Time.deltaTime;
        }
    }

    // �߻� �� ���
    private void Shoot()
    {
        if(currentFireRate <= 0)
        {
            gun.muzzleFlash.Play();
            PlaySE(gun.fire_Sound);
            currentFireRate = gun.fireRate; //����ӵ� ����
            Hit();
        }
    }

    private void Hit()
    {
        if (Physics.Raycast(enemy.transform.position, enemy.transform.forward +
            new Vector3(Random.Range(-gun.accuracy, gun.accuracy),
            Random.Range(-gun.accuracy, gun.accuracy), 0f)
            , out hit, gun.range))
        {
            Debug.Log(hit.transform.name);
            //player ü�� ����
        }
    }

    private void PlaySE(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}
