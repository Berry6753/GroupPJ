using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    // ÃÑ ÄÄÆÛ³ÍÆ®
    [SerializeField] private GunController controller;
    private Gun gun;


    [SerializeField] GameObject BulletHUD;

    [SerializeField] private TextMeshProUGUI[] text_bullet;

    private void Update()
    {
        CheckBullet();
    }

    private void CheckBullet()
    { 
        gun = controller.GetGun();
        text_bullet[0].text = gun.reloadBulletCount.ToString();
        text_bullet[1].text = gun.currentBulletCount.ToString();
        text_bullet[2].text = gun.carryBulletCount.ToString();
    }
}
