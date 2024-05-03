using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private Gun gun;
    private float currentFireRate;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        GunFireRateCalc();
        TryFire();
    }

    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
        { 
            currentFireRate -= Time.deltaTime;
        }
    }

    private void TryFire()
    { 
        if(Input.GetMouseButtonDown(0) && currentFireRate <= 0)
        {
            Fire();
        }
    }

    private void Fire()
    {
        currentFireRate = gun.fireRate;
        Shoot();
    }

    private void Shoot()
    {
        Debug.Log("ÃÑ¾Ë ¹ß»ç");
        gun.muzzleFlash.Play();
        PlaySE(gun.fire_Sound);
    }

    private void PlaySE(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}
