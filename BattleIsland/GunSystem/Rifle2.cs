using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle2 : Gun
{
    private void Awake()
    {
        canShoot = true;
        gunType = GunType.Rifle2;

        damage = gunDatas[(int)gunType].damage;
        coolDown = gunDatas[(int)gunType].timebetFire;
        magSize = gunDatas[(int)gunType].magCapcity;
    }

    public override void PlayerShoot()
    {
        base.PlayerShoot();
    }
}
