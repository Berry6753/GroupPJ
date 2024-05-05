using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AIGunController : MonoBehaviour
{
    [SerializeField] private Gun gun;   // ���� ��
    [SerializeField] private GameObject enemy;

    private float currentFireRate;  // ���� �ӵ� ���

    private AudioSource audioSource;    // ȿ����

    private RaycastHit hit; // ������ �浹 ����

    public Transform laserStartPos;
    private float laserDuration = 0.05f;
    private LineRenderer laserLine;



    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        laserLine = GetComponent<LineRenderer>();
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
            laserLine.SetPosition(0, laserStartPos.position);
            Hit();
        }
    }

    private void Hit()
    {
        float forwardX = Random.Range(-gun.accuracy, gun.accuracy);
        float forwardY = Random.Range(-gun.accuracy, gun.accuracy);
        if (Physics.Raycast(enemy.transform.position, enemy.transform.forward +
            new Vector3(forwardX, forwardY, 0f), out hit, gun.range))
        {
            laserLine.SetPosition(1, hit.point);
            Debug.Log(hit.transform.name);
            if(hit.transform.CompareTag("Player"))
            { 
                //player ü�� ����
            }
        }
        else
        {
            laserLine.SetPosition(1, (enemy.transform.forward + new Vector3(forwardX, forwardY, 0)) * gun.range);
        }
        StartCoroutine(ShootLaser());
    }

    private IEnumerator ShootLaser()
    {
        laserLine.enabled = true;
        yield return new WaitForSeconds(laserDuration);
        laserLine.enabled = false;
    }

    private void PlaySE(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}
