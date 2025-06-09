using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GunType // 총 타입
{
    Rifle1,
    Rifle2,
    Sniper1
}

public class Gun : MonoBehaviour
{
    [Header("Player")]
    private GameObject player;
    public Transform muzzleTransform;
    [SerializeField] private GameObject muzzleFlashEffectPrefab;
    private ZoomControl zoomControl;
    private CombatControl combatControl;
    private float timer;

    [Header("Enemy")]
    private float lastFireTime;

    [Header("Both")]
    [SerializeField] public GunData[] gunDatas;
    public GunType gunType; // 총 타입
    protected float damage; // 데미지
    protected float coolDown; // 발사 쿨타임
    public int magSize; // 탄창 용량
    public int currentMag; // 현재 탄창에 있는 총알 개수
    public bool canShoot; // 발사 가능 여부

    [Header("Audio")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip rifleClip;
    [SerializeField] private AudioClip sniperClip;
    private AudioClip currentClip;

    private void Update()
    {
        if (!canShoot && timer > 0)
        {
            timer -= Time.deltaTime;
        }

        if (timer < 0)
        {
            canShoot = true;
        }
    }

    private void Start()
    {
        audioSource = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
        zoomControl = player.GetComponent<ZoomControl>();
        combatControl = player.GetComponent<CombatControl>();

        if (gunType == GunType.Sniper1)
        {
            currentClip = sniperClip;
        }
        else
        {
            currentClip = rifleClip;
        }
    }

    public virtual void PlayerShoot() // 플레이어 발사 메서드
    {
        if (audioSource == null)
        {
            audioSource = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
        }

        if (canShoot && currentMag > 0)
        {
            audioSource.PlayOneShot(currentClip);

            canShoot = false;
            timer = coolDown;
            currentMag -= 1;

            UIManager.instance.UpdateAmmoText(currentMag); // Test

            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f)); // 화면 중앙 (크로스헤어 위치)에 Ray 쏘기
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 100f, ~(1 << LayerMask.NameToLayer("Player"))))
            {
                // Bullet 생성
                if (combatControl.isFirstPerson) // 1인칭 시점일 때 카메라 조금 앞에서 발사
                {
                    Vector3 forwardDirection = (raycastHit.point - zoomControl.firstPersonCamera.transform.position).normalized * 0.75f;

                    Vector3 muzzleFlashPosition = new Vector3(zoomControl.firstPersonCamera.transform.position.x, zoomControl.firstPersonCamera.transform.position.y - 0.05f, zoomControl.firstPersonCamera.transform.position.z) + forwardDirection;

                    // 오브젝트 풀링
                    if (ObjectPoolControl.instance.bulletQueue.Count > 0)
                    {
                        GameObject currentBullet = ObjectPoolControl.instance.bulletQueue.Dequeue();
                        currentBullet.transform.position = zoomControl.firstPersonCamera.transform.position + forwardDirection;
                        currentBullet.GetComponent<Bullet>().startPostion = zoomControl.firstPersonCamera.transform.position + forwardDirection;
                        currentBullet.transform.forward = forwardDirection;
                        currentBullet.GetComponent<Bullet>().bulletDamage = damage;
                        currentBullet.GetComponent<Bullet>().hit = raycastHit;
                        currentBullet.GetComponentInChildren<TrailRenderer>().enabled = false;
                        currentBullet.SetActive(true);
                    }

                    GameObject muzzleFlashEffect = Instantiate(muzzleFlashEffectPrefab, muzzleFlashPosition, Quaternion.identity);
                    muzzleFlashEffect.transform.forward = forwardDirection;
                    Destroy(muzzleFlashEffect, 0.5f);
                }
                else if (combatControl.isThirdPerson) // 3인칭 시점일 때 총구에서 발사
                {
                    // 오브젝트 풀링
                    if (ObjectPoolControl.instance.bulletQueue.Count > 0)
                    {
                        GameObject currentBullet = ObjectPoolControl.instance.bulletQueue.Dequeue();
                        currentBullet.transform.position = muzzleTransform.position;
                        currentBullet.GetComponent<Bullet>().startPostion = muzzleTransform.position;
                        currentBullet.transform.forward = raycastHit.point - muzzleTransform.position;
                        currentBullet.GetComponent<Bullet>().bulletDamage = damage;
                        currentBullet.GetComponent<Bullet>().hit = raycastHit;
                        currentBullet.GetComponentInChildren<TrailRenderer>().enabled = true;
                        currentBullet.SetActive(true);
                    }

                    GameObject muzzleFlashEffect = Instantiate(muzzleFlashEffectPrefab, muzzleTransform.position, Quaternion.identity);
                    muzzleFlashEffect.transform.forward = raycastHit.point - muzzleTransform.position;
                    Destroy(muzzleFlashEffect, 0.5f);
                }
            }
        }
    }

    public virtual void PlayerReload() // 플레이어 재장전 메서드
    {
        if (InventoryControl.instance.ammo > 0)
        {
            if (InventoryControl.instance.ammo >= magSize - currentMag)
            {
                int needAmmo = magSize - currentMag;

                InventoryControl.instance.ammo -= needAmmo;
                currentMag = magSize;
            }
            else
            {
                currentMag += InventoryControl.instance.ammo;
                InventoryControl.instance.ammo = 0;
            }

            if (InventoryControl.instance.ammo == 0)
            {
                InventoryControl.instance.RemoveItem(108);
            }
        }

        UIManager.instance.UpdateAmmoText(currentMag); // Test
    }

    public virtual void EnemyShoot(AIAgent agent) // 적 AI 발사 메서드
    {
        Fire(agent);
    }

    public virtual void EnemyReload() // 적 재장전 메서드
    {

    }


    private void Fire(AIAgent agent)
    {
        if (Time.time >= lastFireTime + agent.Nowgundata.timebetFire)
        {
            lastFireTime = Time.time;

            Shot(agent);
            agent.isShot = true;
        }
    }

    private void Shot(AIAgent agent)
    {

        Debug.Log("발사");

        GameObject b = BulletPooling.Instance.Bullets.Dequeue();
        b.gameObject.SetActive(true);
        b.transform.position = agent.CurrentGun_Gun.muzzleTransform.position;
        b.transform.rotation = agent.CurrentGun_Gun.muzzleTransform.rotation;
        b.GetComponent<BulletController>().Damage = agent.CurrentGun_Gun.damage;

        agent.FireEffect.transform.position = muzzleTransform.position;
        agent.FireEffect1.transform.position = transform.position;
        agent.FireEffect.Play();
        agent.FireEffect1.Play();
        agent.animator.SetTrigger("Fire");
        agent.enemyAudio.PlayShot();

        agent.magAmmo--;


        //GameObject b = MonoBehaviour.Instantiate(agent.Bullet, agent.SelectStartAim.position, agent.SelectStartAim.transform.rotation);


        /*        Vector3 direction = b.transform.position - agent.AimTarget.position;
                direction.Normalize();
                b.transform.forward = direction;
        
                 GameObject light = MonoBehaviour.Instantiate(agent.FireLight, agent.SelectStartAim.position, Quaternion.identity);
        MonoBehaviour.Destroy(light, 0.03f);*/
    }
}
