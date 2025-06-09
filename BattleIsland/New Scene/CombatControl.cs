using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cinemachine;

public enum Weapon
{
    None,
    Gun,
    Grenade
}

public class CombatControl : MonoBehaviour
{
    // Camera
    [Header("Camera")]
    [SerializeField] private Transform mainCamera; // 메인카메라

    // Weapon Stat
    public Weapon currentWeapon = Weapon.None;
    private bool isChanging = false;

    // Health Stat
    public bool isDead = false;
    public float playerHealth = 300.0f;
    public bool isArmor = false;

    // Gun
    [Header("Gun")]
    public GameObject currentGun;
    [SerializeField] private GameObject testGunPrefab;
    [SerializeField] private GameObject riflePrefab;
    [SerializeField] private GameObject sniperPrefab;
    public bool hasGun = false; // 등 뒤에 총을 장착했는가?
    private bool isReloading = false; // 재장전 중인가?

    // Grenade
    [Header("Grenade")]
    public Transform grenadePivot; // 수류탄 피벗
    [SerializeField] private GameObject testGrenadePrefab; // 수류탄 프리팹
    [SerializeField] private GameObject grenadeModel; // 수류탄 모델
    public float throwPower = 7.5f; // 던지는 힘
    public Vector3 throwDirection; // 던질 방향
    private bool canThrow = true; // 던지기 가능 여부

    [Header("Recoil")]
    public float recoilX;
    public float recoilY;
    public float crouchRecoilX;
    public float crouchRecoilY;

    // Components
    private Animator animator;
    private CharacterMovement cm;
    private ZoomControl zoomControl;
    private AudioSource audioSource;

    // Mouse Input
    [Header("Mouse Input")]
    public float clickTimer = 0f;

    // Zoom
    [Header("Zoom Status")]
    public bool isFirstPerson = false;
    public bool isThirdPerson = false;
    private float thirdPersonEnterTime = 0.25f;

    // Aim
    [Header("Aim")]
    [SerializeField] private GameObject aimTarget;
    [SerializeField] private GameObject rig;
    [SerializeField] private Transform backGunPivot;
    public Transform holdGunPivot;
    [SerializeField] private Transform shootGunPivot;
    public bool lookAround = false;
    public float normalCamX;
    public float normalCamY;
    [SerializeField] private GameObject crosshairControl;

    // Heal
    [Header("Heal")]
    public bool isHealing = false;

    // AudioClip
    [Header("AudioClip")]
    [SerializeField] private AudioClip reloadClip;

    private void Awake()
    {
        TryGetComponent(out animator);
        TryGetComponent(out cm);
        TryGetComponent(out zoomControl);
        TryGetComponent(out audioSource);
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.PageUp)) // Damage Test
        {
            TakeDamage(20.0f);
        }

        if (!isHealing && Input.GetKeyDown(KeyCode.Keypad4)) // Heal
        {
            if (InventoryControl.instance.CheckInventory(109))
            {
                isHealing = true;
                StartCoroutine(Heal_Co());
            }
        }

        // [총을 손에 장착] - TEST
        if (hasGun && (currentWeapon != Weapon.Gun) && !isChanging && Input.GetKeyDown(KeyCode.Keypad1))
        {
            isChanging = true;

            StartCoroutine(EquipGun_co());
        }

        // [총을 손에서 등 뒤로 넣기]
        if (hasGun && (currentWeapon == Weapon.Gun) && !isChanging && Input.GetKeyDown(KeyCode.Keypad1))
        {
            isChanging = true;

            currentWeapon = Weapon.None;

            StartCoroutine(UnEquipGun_co());
        }

        // [재장전]
        if (currentWeapon == Weapon.Gun && !isReloading && Input.GetKeyDown(KeyCode.R))
        {
            if (currentGun != null && currentGun.GetComponent<Gun>().currentMag != currentGun.GetComponent<Gun>().magSize && InventoryControl.instance.CheckInventory(108))
            {
                isReloading = true;
                StartCoroutine(Reload_co());
            }
        }

        // [수류탄 장착]
        if (currentWeapon != Weapon.Grenade && !isChanging && InventoryControl.instance.CheckInventory(107) && Input.GetKeyDown(KeyCode.Keypad2))
        {
            if (isFirstPerson || isThirdPerson)
            {
                return;
            }

            isChanging = true;

            currentWeapon = Weapon.Grenade;

            StartCoroutine(UnEquipGun_co());

            grenadeModel.SetActive(true);
        }

        // [마우스 입력]
        // 1. 우클릭 -> 타이머 시작
        // 2. 우클릭 해제 시 타이머에 따라
        // 살짝 눌렀으면 1인칭 진입
        // 원래 3인칭 상태였으면 3인칭 해제
        // 원래 1인칭 상태였으면 1인칭 해제
        // 3. 우클릭 한 채로 오래 있으면 3인칭 진입

        float camAngle;
        if (mainCamera.eulerAngles.x > 300)
        {
            camAngle = mainCamera.eulerAngles.x - 360.0f;
        }
        else
        {
            camAngle = mainCamera.eulerAngles.x;
        }

        float upPower = 1.0f - camAngle / 30.0f;
        throwDirection = transform.up * upPower + transform.forward; // 마우스 회전에 따라 수류탄 투척 방향 결정

        // [총 & 우클릭]
        if (currentWeapon == Weapon.Gun && cm.isGround && !isReloading)
        {
            if (Input.GetMouseButton(1))
            {
                clickTimer += Time.deltaTime;
            }

            if (clickTimer >= thirdPersonEnterTime)
            {
                clickTimer = 0;

                if (!isFirstPerson && !isThirdPerson)
                {
                    // 3인칭 진입
                    isThirdPerson = true;
                    cm.currentSpeed = cm.thirdPersonSpeed;
                    zoomControl.Third_ZoomIn();

                    currentGun.transform.SetParent(shootGunPivot);
                    currentGun.transform.localPosition = Vector3.zero;
                    currentGun.transform.localRotation = Quaternion.Euler(Vector3.zero);

                    rig.transform.Find("Aim").GetComponent<MultiAimConstraint>().weight = 1.0f;
                    rig.transform.Find("Body").GetComponent<MultiAimConstraint>().weight = 1.0f;
                }
            }

            if (Input.GetMouseButtonUp(1))
            {
                clickTimer = 0;

                if (isFirstPerson)
                {
                    // 1인칭 해제
                    isFirstPerson = false;
                    cm.currentSpeed = cm.walkSpeed;
                    zoomControl.First_ZoomOut();

                    rig.transform.Find("Aim").GetComponent<MultiAimConstraint>().weight = 0f;
                    rig.transform.Find("Body").GetComponent<MultiAimConstraint>().weight = 0f;

                    return;
                }

                if (isThirdPerson)
                {
                    // 3인칭 해제
                    isThirdPerson = false;
                    cm.currentSpeed = cm.walkSpeed;
                    zoomControl.Third_ZoomOut();

                    rig.transform.Find("Aim").GetComponent<MultiAimConstraint>().weight = 0f;
                    rig.transform.Find("LeftHand").GetComponent<TwoBoneIKConstraint>().weight = 1.0f;

                    currentGun.transform.SetParent(holdGunPivot);
                    currentGun.transform.localPosition = Vector3.zero;
                    currentGun.transform.localRotation = Quaternion.Euler(Vector3.zero);

                    return;
                }

                if (!isFirstPerson && !isThirdPerson && clickTimer < thirdPersonEnterTime)
                {
                    // 1인칭 진입
                    isFirstPerson = true;
                    cm.currentSpeed = cm.firstPersonSpeed;
                    zoomControl.First_ZoomIn(currentGun);
                    return;
                }
            }
        }

        // [총 & 좌클릭]
        if (currentWeapon == Weapon.Gun && Input.GetMouseButton(0))
        {
            if (isFirstPerson || isThirdPerson)
            {
                if (currentGun != null && currentGun.GetComponent<Gun>().currentMag > 0)
                {
                    if (isThirdPerson)
                    {
                        crosshairControl.GetComponent<CrosshairControl>().Expand();
                    }

                    currentGun.GetComponent<Gun>().PlayerShoot();
                    GunRecoil();
                }
            }
        }

        // [수류탄 & 우클릭]
        if (currentWeapon == Weapon.Grenade && cm.isGround && !isReloading)
        {
            if (Input.GetMouseButtonDown(1))
            {
                isThirdPerson = true;
                zoomControl.Third_ZoomIn();
                // 궤적 On
                GetComponent<DrawProjection>().drawProjection = true;
            }

            if (Input.GetMouseButtonUp(1))
            {
                isThirdPerson = false;
                zoomControl.Third_ZoomOut();
                // 궤적 Off
                GetComponent<DrawProjection>().drawProjection = false;
            }
        }

        // [수류탄 & 좌클릭]
        if (currentWeapon == Weapon.Grenade && Input.GetMouseButtonDown(0))
        {
            if (isThirdPerson && canThrow)
            {
                canThrow = false;
                InventoryControl.instance.RemoveItem(107);
                StartCoroutine(ThrowGrenade());
                // 궤적 Off
                GetComponent<DrawProjection>().drawProjection = false;
            }
        }

        // [둘러보기]
        if (!isFirstPerson && !isThirdPerson && !lookAround && Input.GetKeyDown(KeyCode.LeftAlt))
        {
            zoomControl.StartLookAround();
            lookAround = true;
        }
        if (lookAround && Input.GetKeyUp(KeyCode.LeftAlt))
        {
            zoomControl.EndLookAround();
            lookAround = false;
        }

        // [Aim 설정]
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f)); // 화면 중앙 (크로스헤어 위치)에 Ray 쏘기
        
        /*if (Physics.Raycast(ray, out RaycastHit raycastHit, 100f, ~(1 << LayerMask.NameToLayer("Player"))))
        {
            aimTarget.transform.position = raycastHit.point;
        }*/

        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, 100f, ~(1 << LayerMask.NameToLayer("Player")));

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if ((Vector3.Distance(Camera.main.transform.position, hit.point) > Vector3.Distance(Camera.main.transform.position, transform.position)) && Vector3.Distance(transform.position, hit.point) > 2.5f)
            {
                aimTarget.transform.position = hit.point;
                break;
            }
        }

        if (!isFirstPerson && !isThirdPerson)
        {
            rig.transform.Find("Body").GetComponent<MultiAimConstraint>().weight = 0.0f;
        }
    }

    public void EquipGun(GunType gunType)
    {
        // [총을 등 뒤에 장착] - TEST
        if (!hasGun)
        {
            if (gunType == GunType.Sniper1)
            {
                hasGun = true;

                currentGun = Instantiate(sniperPrefab, transform.position, Quaternion.identity);
                currentGun.transform.SetParent(backGunPivot);
                currentGun.transform.localPosition = Vector3.zero;
                currentGun.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            else
            {
                hasGun = true;

                currentGun = Instantiate(riflePrefab, transform.position, Quaternion.identity);
                currentGun.transform.SetParent(backGunPivot);
                currentGun.transform.localPosition = Vector3.zero;
                currentGun.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }
    }

    private IEnumerator Reload_co()
    {
        if (isFirstPerson)
        {
            isFirstPerson = false;
            cm.currentSpeed = cm.walkSpeed;
            zoomControl.First_ZoomOut();

            rig.transform.Find("Aim").GetComponent<MultiAimConstraint>().weight = 0f;
            rig.transform.Find("Body").GetComponent<MultiAimConstraint>().weight = 0f;
        }
        else if (isThirdPerson)
        {
            isThirdPerson = false;
            cm.currentSpeed = cm.walkSpeed;
            zoomControl.Third_ZoomOut();

            rig.transform.Find("Aim").GetComponent<MultiAimConstraint>().weight = 0f;
            rig.transform.Find("LeftHand").GetComponent<TwoBoneIKConstraint>().weight = 1.0f;

            currentGun.transform.SetParent(holdGunPivot);
            currentGun.transform.localPosition = Vector3.zero;
            currentGun.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        rig.GetComponent<Rig>().weight = 0f;

        animator.SetTrigger("Reload");

        yield return new WaitForSeconds(0.25f);

        audioSource.PlayOneShot(reloadClip);
        currentGun.GetComponent<Gun>().PlayerReload();

        yield return new WaitForSeconds(1.5f);
        rig.GetComponent<Rig>().weight = 1.0f;

        isReloading = false;
    }

    public void EquipGun(GameObject gun)
    {
        currentGun = gun;
    }

    private IEnumerator UnEquipGun_co()
    {
        animator.SetBool("EquipGun", false);
        animator.SetTrigger("UnEquip");
        rig.GetComponent<Rig>().weight = 0f;

        yield return new WaitForSeconds(0.7f);

        if (currentGun != null)
        {
            currentGun.transform.SetParent(backGunPivot);
            currentGun.transform.localPosition = Vector3.zero;
            currentGun.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        yield return new WaitForSeconds(0.8f);

        isChanging = false;
    }

    private IEnumerator EquipGun_co()
    {
        GetComponent<DrawProjection>().drawProjection = false;

        if (isThirdPerson)
        {
            isThirdPerson = false;
            zoomControl.Third_ZoomOut();
        }

        currentWeapon = Weapon.Gun;
        rig.GetComponent<Rig>().weight = 1.0f;
        animator.SetBool("EquipGun", true);
        animator.SetTrigger("Equip");

        currentGun.transform.SetParent(holdGunPivot);
        currentGun.transform.localPosition = Vector3.zero;
        currentGun.transform.localRotation = Quaternion.Euler(Vector3.zero);

        grenadeModel.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        isChanging = false;
    }

    public void GunRecoil()
    {
        if (GetComponent<CharacterMovement>().isCrouch)
        {
            if (isFirstPerson)
            {
                CinemachinePOV pov = zoomControl.firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
                pov.m_HorizontalAxis.Value += Random.Range(-crouchRecoilX, crouchRecoilX); // x축 반동;
                pov.m_VerticalAxis.Value -= Random.Range(0, crouchRecoilY); // y축 반동
            }
            else if (isThirdPerson)
            {
                zoomControl.thirdPersonCamera.m_XAxis.Value += Random.Range(-crouchRecoilX * 0.01f, crouchRecoilX * 0.01f); // x축 반동;
                zoomControl.thirdPersonCamera.m_YAxis.Value -= Random.Range(0, crouchRecoilY * 0.01f); // y축 반동;
            }
        }
        else
        {
            if (isFirstPerson)
            {
                CinemachinePOV pov = zoomControl.firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
                pov.m_HorizontalAxis.Value += Random.Range(-recoilX, recoilX); // x축 반동;
                pov.m_VerticalAxis.Value -= Random.Range(0, recoilY); // y축 반동
            }
            else if (isThirdPerson)
            {
                zoomControl.thirdPersonCamera.m_XAxis.Value += Random.Range(-recoilX * 0.01f, recoilX * 0.01f); // x축 반동;
                zoomControl.thirdPersonCamera.m_YAxis.Value -= Random.Range(0, recoilY * 0.01f); // y축 반동;
            }
        }
    }

    private IEnumerator ThrowGrenade() // 수류탄 던지기
    {
        Vector3 direction = throwDirection * throwPower; // 방향 미리 지정

        animator.SetTrigger("ThrowGrenade"); // 애니메이션 재생

        yield return new WaitForSeconds(0.55f);

        grenadeModel.SetActive(false);

        GameObject currentGrenade = Instantiate(testGrenadePrefab, grenadePivot.position, Quaternion.identity); // 수류탄 생성
        currentGrenade.GetComponent<Rigidbody>().velocity = direction; // 방향으로 던지기
        currentGrenade.GetComponent<Grenade>().StartTimer(); // 수류탄 타이머 시작

        GetComponent<IndicatorControl>().target = currentGrenade;
        GetComponent<IndicatorControl>().ToggleIndicator(true);

        yield return new WaitForSeconds(1.0f);

        if (isThirdPerson)
        {
            isThirdPerson = false;
            zoomControl.Third_ZoomOut();
        }

        canThrow = true;

        currentWeapon = Weapon.None;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }

        UIManager.instance.DamageIndicator(damage);

        playerHealth -= damage;

        if (playerHealth <= 0)
        {
            playerHealth = 0;
            PlayerDead();
        }
    }

    private void PlayerDead()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (isFirstPerson)
        {
            isFirstPerson = false;
            zoomControl.First_ZoomOut();
        }
        else if (isThirdPerson)
        {
            isThirdPerson = false;
            zoomControl.Third_ZoomOut();
        }

        isDead = true;
        GameManager.instance.isPlayerDead = true;
        GameManager.instance.isGameOver = true;
        rig.GetComponent<Rig>().weight = 0f;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        //GetComponent<Rigidbody>().isKinematic = true;
        animator.SetTrigger("Dead");

        // 죽은 다음 동작 함수 호출
    }

    private IEnumerator Heal_Co()
    {
        float timer = 3.0f;

        animator.SetTrigger("Heal");

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            if (!isHealing)
            {
                animator.SetTrigger("Cancel");
                yield break;
            }

            yield return null;
        }

        if (playerHealth < 300)
        {
            if (playerHealth >= 270)
            {
                playerHealth = 300;
            }
            else
            {
                playerHealth += 150;
            }
        }

        InventoryControl.instance.RemoveItem(109);
        //Debug.Log("Heal 완료");
    }
}
