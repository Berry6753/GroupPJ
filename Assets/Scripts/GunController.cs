using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private Gun gun;   // 현재 총
    private float currentFireRate;  // 연사 속도 계산

    private AudioSource audioSource;    // 효과음

    // 상태 변수
    private bool isReload = false;
    [HideInInspector] public bool isfineSightMode = false;

    [SerializeField] private Vector3 originPos; //원래 포지션 값
     
    private RaycastHit hit; // 레이저 충돌 정보

    
    [SerializeField] private Camera cam;
    private Crosshair crosshair;

    [SerializeField] private GameObject hit_effect_prefab;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        crosshair = FindObjectOfType<Crosshair>();
    }

    void Update()
    {
        GunFireRateCalc();
        TryFire();
        TryReload();
        TryFindSight();
        Debug.DrawRay(cam.transform.position, cam.transform.forward, Color.red, gun.range);
    }

    // 연사속도 재계산
    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
        { 
            currentFireRate -= Time.deltaTime;
        }
    }

    // 발사 시도
    private void TryFire()
    { 
        if(Input.GetMouseButtonDown(0) && currentFireRate <= 0 && !isReload)
        {
            Fire();
        }
    }

    // 발사 전 계산
    private void Fire()
    {
        if(!isReload)
        {
            if (gun.currentBulletCount > 0)
            {
                Shoot();
            }
            else
            {
                PlaySE(gun.empty_Sound);
            }
        }
    }

    // 발사 후 계산
    private void Shoot()
    {
        gun.muzzleFlash.Play();
        PlaySE(gun.fire_Sound);
        currentFireRate = gun.fireRate; //연사속도 재계산
        gun.currentBulletCount--;
        Hit();
        crosshair.FireAnimation();

        StopAllCoroutines();
        StartCoroutine(RetroAction());
    }

    private void Hit()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward +
            new Vector3(Random.Range(-crosshair.GetAccuracy() - gun.accuracy, crosshair.GetAccuracy() + gun.accuracy),
            Random.Range(-crosshair.GetAccuracy() - gun.accuracy, crosshair.GetAccuracy() + gun.accuracy), 0f)
            , out hit, gun.range))
        {
            GameObject clone = Instantiate(hit_effect_prefab, hit.point, Quaternion.LookRotation(hit.normal));
            Debug.Log(hit.transform.name);
            Destroy(clone, 2f);
        }
    }

    // 반동
    private IEnumerator RetroAction()
    {
        Vector3 reciolBack = new Vector3(gun.retroActionFineSignForce, originPos.y, originPos.z);

        gun.transform.localPosition = gun.fineSightoriginPos;

        // 반동 시작
        while (gun.transform.localPosition.x <= gun.retroActionFineSignForce - 0.02f)
        {
            gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, reciolBack, 0.4f);
            yield return null;
        }

        // 원위치
        while (gun.transform.localPosition != gun.fineSightoriginPos)
        {
            gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, gun.fineSightoriginPos, 0.1f);
            yield return null;
        }
    }

    // 오디오 플레이
    private void PlaySE(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    // 재장전 시도
    private void TryReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReload && gun.currentBulletCount < gun.reloadBulletCount)
        {
            StartCoroutine(Reload());
        }
    }

    // 재장전
    private IEnumerator Reload()
    {
        if(gun.carryBulletCount > 0)
        {
            isReload = true;

            yield return new WaitForSeconds(gun.reloadTime);

            //gun.carryBulletCount += gun.currentBulletCount;   //재장전시 총알 유지
            //gun.currentBulletCount = 0;

            if (gun.carryBulletCount >= gun.reloadBulletCount)
            {
                gun.currentBulletCount = gun.reloadBulletCount;
                gun.carryBulletCount -= gun.reloadBulletCount;
            }
            else
            { 
                gun.currentBulletCount = gun.carryBulletCount;
            }

            isReload = false;
        }
        else
        {
            Debug.Log("총알 없음");
        }
    }

    // 정조준 시도
    private void TryFindSight()
    {
        if (Input.GetMouseButtonDown(1) && !isReload)
        {
            FineSight();
        }
    }

    // 정조준 취소
    public void CancelFineSight()
    {
        if (isfineSightMode)
            FineSight();
    }

    // 정조준 로직
    private void FineSight()
    { 
        //조준애니메이션
        isfineSightMode = !isfineSightMode;

        if(isfineSightMode)
        {
            StopAllCoroutines();
            StartCoroutine(FineSightActive());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FineSightDeActive());
        }
    }

    // 정조준 활성화
    private IEnumerator FineSightActive()
    {
        while (gun.transform.localPosition != gun.fineSightoriginPos)
        {
            gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, gun.fineSightoriginPos, 0.2f);
            yield return null;
        }
    }
    // 정조준 비활성화
    private IEnumerator FineSightDeActive()
    {
        while (gun.transform.localPosition != originPos)
        {
            gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, originPos, 0.2f);
            yield return null;
        }
    }

    public Gun GetGun() { return gun; }
}
