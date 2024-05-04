using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGunController : MonoBehaviour
{
    [SerializeField] private Gun gun;   // 현재 총
    [SerializeField] private GameObject enemy;

    private float currentFireRate;  // 연사 속도 계산

    private AudioSource audioSource;    // 효과음

    private RaycastHit hit; // 레이저 충돌 정보



    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        GunFireRateCalc();
        Shoot();
    }

    // 연사속도 재계산
    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
        {
            currentFireRate -= Time.deltaTime;
        }
    }

    // 발사 후 계산
    private void Shoot()
    {
        if(currentFireRate <= 0)
        {
            gun.muzzleFlash.Play();
            PlaySE(gun.fire_Sound);
            currentFireRate = gun.fireRate; //연사속도 재계산
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
            //player 체력 감소
        }
    }

    private void PlaySE(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}
