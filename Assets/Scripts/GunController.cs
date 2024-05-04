using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private Gun gun;   // ���� ��
    private float currentFireRate;  // ���� �ӵ� ���

    private AudioSource audioSource;    // ȿ����

    // ���� ����
    private bool isReload = false;
    [HideInInspector] public bool isfineSightMode = false;

    [SerializeField] private Vector3 originPos; //���� ������ ��
     
    private RaycastHit hit; // ������ �浹 ����

    
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

    // ����ӵ� ����
    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
        { 
            currentFireRate -= Time.deltaTime;
        }
    }

    // �߻� �õ�
    private void TryFire()
    { 
        if(Input.GetMouseButtonDown(0) && currentFireRate <= 0 && !isReload)
        {
            Fire();
        }
    }

    // �߻� �� ���
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

    // �߻� �� ���
    private void Shoot()
    {
        gun.muzzleFlash.Play();
        PlaySE(gun.fire_Sound);
        currentFireRate = gun.fireRate; //����ӵ� ����
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

    // �ݵ�
    private IEnumerator RetroAction()
    {
        Vector3 reciolBack = new Vector3(gun.retroActionFineSignForce, originPos.y, originPos.z);

        gun.transform.localPosition = gun.fineSightoriginPos;

        // �ݵ� ����
        while (gun.transform.localPosition.x <= gun.retroActionFineSignForce - 0.02f)
        {
            gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, reciolBack, 0.4f);
            yield return null;
        }

        // ����ġ
        while (gun.transform.localPosition != gun.fineSightoriginPos)
        {
            gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, gun.fineSightoriginPos, 0.1f);
            yield return null;
        }
    }

    // ����� �÷���
    private void PlaySE(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    // ������ �õ�
    private void TryReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReload && gun.currentBulletCount < gun.reloadBulletCount)
        {
            StartCoroutine(Reload());
        }
    }

    // ������
    private IEnumerator Reload()
    {
        if(gun.carryBulletCount > 0)
        {
            isReload = true;

            yield return new WaitForSeconds(gun.reloadTime);

            //gun.carryBulletCount += gun.currentBulletCount;   //�������� �Ѿ� ����
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
            Debug.Log("�Ѿ� ����");
        }
    }

    // ������ �õ�
    private void TryFindSight()
    {
        if (Input.GetMouseButtonDown(1) && !isReload)
        {
            FineSight();
        }
    }

    // ������ ���
    public void CancelFineSight()
    {
        if (isfineSightMode)
            FineSight();
    }

    // ������ ����
    private void FineSight()
    { 
        //���ؾִϸ��̼�
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

    // ������ Ȱ��ȭ
    private IEnumerator FineSightActive()
    {
        while (gun.transform.localPosition != gun.fineSightoriginPos)
        {
            gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, gun.fineSightoriginPos, 0.2f);
            yield return null;
        }
    }
    // ������ ��Ȱ��ȭ
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
