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
    [SerializeField] private Transform mainCamera; // ����ī�޶�

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
    public bool hasGun = false; // �� �ڿ� ���� �����ߴ°�?
    private bool isReloading = false; // ������ ���ΰ�?

    // Grenade
    [Header("Grenade")]
    public Transform grenadePivot; // ����ź �ǹ�
    [SerializeField] private GameObject testGrenadePrefab; // ����ź ������
    [SerializeField] private GameObject grenadeModel; // ����ź ��
    public float throwPower = 7.5f; // ������ ��
    public Vector3 throwDirection; // ���� ����
    private bool canThrow = true; // ������ ���� ����

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

        // [���� �տ� ����] - TEST
        if (hasGun && (currentWeapon != Weapon.Gun) && !isChanging && Input.GetKeyDown(KeyCode.Keypad1))
        {
            isChanging = true;

            StartCoroutine(EquipGun_co());
        }

        // [���� �տ��� �� �ڷ� �ֱ�]
        if (hasGun && (currentWeapon == Weapon.Gun) && !isChanging && Input.GetKeyDown(KeyCode.Keypad1))
        {
            isChanging = true;

            currentWeapon = Weapon.None;

            StartCoroutine(UnEquipGun_co());
        }

        // [������]
        if (currentWeapon == Weapon.Gun && !isReloading && Input.GetKeyDown(KeyCode.R))
        {
            if (currentGun != null && currentGun.GetComponent<Gun>().currentMag != currentGun.GetComponent<Gun>().magSize && InventoryControl.instance.CheckInventory(108))
            {
                isReloading = true;
                StartCoroutine(Reload_co());
            }
        }

        // [����ź ����]
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

        // [���콺 �Է�]
        // 1. ��Ŭ�� -> Ÿ�̸� ����
        // 2. ��Ŭ�� ���� �� Ÿ�̸ӿ� ����
        // ��¦ �������� 1��Ī ����
        // ���� 3��Ī ���¿����� 3��Ī ����
        // ���� 1��Ī ���¿����� 1��Ī ����
        // 3. ��Ŭ�� �� ä�� ���� ������ 3��Ī ����

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
        throwDirection = transform.up * upPower + transform.forward; // ���콺 ȸ���� ���� ����ź ��ô ���� ����

        // [�� & ��Ŭ��]
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
                    // 3��Ī ����
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
                    // 1��Ī ����
                    isFirstPerson = false;
                    cm.currentSpeed = cm.walkSpeed;
                    zoomControl.First_ZoomOut();

                    rig.transform.Find("Aim").GetComponent<MultiAimConstraint>().weight = 0f;
                    rig.transform.Find("Body").GetComponent<MultiAimConstraint>().weight = 0f;

                    return;
                }

                if (isThirdPerson)
                {
                    // 3��Ī ����
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
                    // 1��Ī ����
                    isFirstPerson = true;
                    cm.currentSpeed = cm.firstPersonSpeed;
                    zoomControl.First_ZoomIn(currentGun);
                    return;
                }
            }
        }

        // [�� & ��Ŭ��]
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

        // [����ź & ��Ŭ��]
        if (currentWeapon == Weapon.Grenade && cm.isGround && !isReloading)
        {
            if (Input.GetMouseButtonDown(1))
            {
                isThirdPerson = true;
                zoomControl.Third_ZoomIn();
                // ���� On
                GetComponent<DrawProjection>().drawProjection = true;
            }

            if (Input.GetMouseButtonUp(1))
            {
                isThirdPerson = false;
                zoomControl.Third_ZoomOut();
                // ���� Off
                GetComponent<DrawProjection>().drawProjection = false;
            }
        }

        // [����ź & ��Ŭ��]
        if (currentWeapon == Weapon.Grenade && Input.GetMouseButtonDown(0))
        {
            if (isThirdPerson && canThrow)
            {
                canThrow = false;
                InventoryControl.instance.RemoveItem(107);
                StartCoroutine(ThrowGrenade());
                // ���� Off
                GetComponent<DrawProjection>().drawProjection = false;
            }
        }

        // [�ѷ�����]
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

        // [Aim ����]
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f)); // ȭ�� �߾� (ũ�ν���� ��ġ)�� Ray ���
        
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
        // [���� �� �ڿ� ����] - TEST
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
                pov.m_HorizontalAxis.Value += Random.Range(-crouchRecoilX, crouchRecoilX); // x�� �ݵ�;
                pov.m_VerticalAxis.Value -= Random.Range(0, crouchRecoilY); // y�� �ݵ�
            }
            else if (isThirdPerson)
            {
                zoomControl.thirdPersonCamera.m_XAxis.Value += Random.Range(-crouchRecoilX * 0.01f, crouchRecoilX * 0.01f); // x�� �ݵ�;
                zoomControl.thirdPersonCamera.m_YAxis.Value -= Random.Range(0, crouchRecoilY * 0.01f); // y�� �ݵ�;
            }
        }
        else
        {
            if (isFirstPerson)
            {
                CinemachinePOV pov = zoomControl.firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
                pov.m_HorizontalAxis.Value += Random.Range(-recoilX, recoilX); // x�� �ݵ�;
                pov.m_VerticalAxis.Value -= Random.Range(0, recoilY); // y�� �ݵ�
            }
            else if (isThirdPerson)
            {
                zoomControl.thirdPersonCamera.m_XAxis.Value += Random.Range(-recoilX * 0.01f, recoilX * 0.01f); // x�� �ݵ�;
                zoomControl.thirdPersonCamera.m_YAxis.Value -= Random.Range(0, recoilY * 0.01f); // y�� �ݵ�;
            }
        }
    }

    private IEnumerator ThrowGrenade() // ����ź ������
    {
        Vector3 direction = throwDirection * throwPower; // ���� �̸� ����

        animator.SetTrigger("ThrowGrenade"); // �ִϸ��̼� ���

        yield return new WaitForSeconds(0.55f);

        grenadeModel.SetActive(false);

        GameObject currentGrenade = Instantiate(testGrenadePrefab, grenadePivot.position, Quaternion.identity); // ����ź ����
        currentGrenade.GetComponent<Rigidbody>().velocity = direction; // �������� ������
        currentGrenade.GetComponent<Grenade>().StartTimer(); // ����ź Ÿ�̸� ����

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

        // ���� ���� ���� �Լ� ȣ��
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
        //Debug.Log("Heal �Ϸ�");
    }
}
