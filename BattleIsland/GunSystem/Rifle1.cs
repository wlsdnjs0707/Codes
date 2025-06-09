using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle1 : Gun
{
    private void Awake()
    {
        canShoot = true;
        gunType = GunType.Rifle1;

        damage = gunDatas[(int)gunType].damage;
        coolDown = gunDatas[(int)gunType].timebetFire;
        magSize = gunDatas[(int)gunType].magCapcity;
    }

    public override void PlayerShoot()
    {
        base.PlayerShoot();
    }

    public override void EnemyShoot(AIAgent agent)
    {
        base.EnemyShoot(agent);
    }
}
