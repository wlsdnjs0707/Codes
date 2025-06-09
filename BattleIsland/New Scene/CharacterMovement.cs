using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;

public class CharacterMovement : MonoBehaviour
{
    // Components
    private Rigidbody rb;
    private Animator animator;
    private ZoomControl zoomControl;
    private CombatControl combatControl;

    // Movement
    [Header("Movement")]
    public bool canMove = true;
    private float x;
    private float z;
    public float currentSpeed;
    public float walkSpeed = 2.5f;
    public float runSpeed = 6.5f;
    public float forwardSpeed = 0f;
    public float firstPersonSpeed = 0.5f;
    public float thirdPersonSpeed = 1.0f;
    public bool isCrouch = false;
    private float crouchTimer = 0f;

    // Jump
    private float jumpForce = 5.0f;

    // Ground Check
    [Header("Ground Check")]
    public LayerMask playerLayer;
    public bool isGround = true;

    // Movement Bool
    private bool isRun = false;

    // Car
    [Header("Car")]
    private GameObject nearCar; // �ֺ� ��
    private bool isCarEntered = false; // �� ž�� ����

    // Model, Pivot
    [Header("Model")]
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject holdGunPivot;
    [SerializeField] private GameObject backGunPivot;

    // Night Vision
    [Header("Night Vision")]
    [SerializeField] private GameObject nightVision;
    private bool isFading = false;

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out animator);
        TryGetComponent(out zoomControl);
        TryGetComponent(out combatControl);

        // [���콺 Ŀ�� ����]
        Cursor.lockState = CursorLockMode.Locked;

        // [�̵� �ӵ� �ʱ�ȭ]
        forwardSpeed = walkSpeed;
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        if (combatControl.isDead)
        {
            return;
        }

        if (!isCarEntered)
        {
            if (canMove)
            {
                GetInput();
                GroundCheck();
            }

            if (isCrouch)
            {
                crouchTimer += Time.deltaTime;
            }

            if (!isRun && currentSpeed > walkSpeed) // �ӵ� õõ�� �پ���
            {
                currentSpeed -= Time.deltaTime * 10.0f;
                forwardSpeed = currentSpeed;
            }

            if (Input.GetKeyDown(KeyCode.Insert)) // TEST
            {
                InventoryControl.instance.ShowInventory();
            }

            if (Input.GetKeyDown(KeyCode.Home)) // Test
            {
                animator.SetTrigger("Dance");
            }

            if (Input.GetKeyDown(KeyCode.G)) // ����
            {
                EnterCar();
            }

            if (Input.GetKeyDown(KeyCode.N)) // �߰����ð� On, Off
            {
                ToggleNightVision();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.G)) // ����
            {
                ExitCar();
            }
        }
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            Move();
        }
    }

    private void GetInput()
    {
        #region Ű���� �Է�
        // [���� �Է�]
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        // [�ִϸ����� ����]
        animator.SetFloat("MoveSpeedX", x);
        animator.SetFloat("MoveSpeedZ", z * currentSpeed);

        // [�ӵ� ����] - �ȱ�, �޸���
        if (Input.GetKey(KeyCode.LeftShift) && z > 0 && isGround)
        {
            if (!combatControl.isFirstPerson && !combatControl.isThirdPerson)
            {
                if (isCrouch && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                {
                    isCrouch = false;
                    crouchTimer = 0;
                    animator.SetTrigger("UnCrouch");
                    animator.SetBool("isCrouch", isCrouch);
                }

                isRun = true;
                combatControl.isHealing = false;

                if (forwardSpeed < runSpeed)
                {
                    forwardSpeed += Time.deltaTime * 7.5f;
                }

                currentSpeed = forwardSpeed;
            }
        }
        
        // [�ɱ�]
        if (isGround && combatControl.currentWeapon == Weapon.Gun && Input.GetKeyDown(KeyCode.C))
        {
            if (combatControl.isFirstPerson || combatControl.isThirdPerson)
            {
                return;
            }

            if (isCrouch)
            {
                isCrouch = false;
                crouchTimer = 0;
                animator.SetTrigger("UnCrouch");
                animator.SetBool("isCrouch", isCrouch);
            }
            else
            {
                isCrouch = true;
                animator.SetTrigger("Crouch");
                animator.SetBool("isCrouch", isCrouch);
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isRun = false;
        }

        // [����]
        if (isGround && Input.GetKeyDown(KeyCode.Space))
        {
            combatControl.clickTimer = 0;

            if (combatControl.isFirstPerson)
            {
                combatControl.isFirstPerson = false;
                currentSpeed = walkSpeed;
                zoomControl.First_ZoomOut();
            }
            else if (combatControl.isThirdPerson)
            {
                combatControl.isThirdPerson = false;
                currentSpeed = walkSpeed;
                zoomControl.Third_ZoomOut();
            }

            isCrouch = false;

            animator.SetTrigger("Jump");

            forwardSpeed = walkSpeed;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        #endregion

        // [�÷��̾� ȸ��]
        if (!combatControl.lookAround)
        {
            transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
        }
    }

    private void Move()
    {
        // [�ӵ� ����] - Ⱦ�̵� �� �ӵ�
        float strafeSpeed;
        if (currentSpeed > walkSpeed)
        {
            strafeSpeed = walkSpeed;
        }
        else
        {
            strafeSpeed = currentSpeed;
        }

        // [�ӵ� ����] - ���� �� �ӵ�
        if (!isGround)
        {
            currentSpeed = walkSpeed;
        }

        // [�ӵ� ����] - ġ�� �� �ӵ�
        if (combatControl.isHealing)
        {
            currentSpeed = walkSpeed * 0.5f;
        }

        // [�̵�]
        Vector3 targetDirection = transform.forward * z * currentSpeed + transform.right * x * strafeSpeed;

        // [�ɱ� ����]
        if (Vector3.Magnitude(targetDirection) > 0)
        {
            if (isCrouch && crouchTimer > 0.95f)
            {
                if (combatControl.isThirdPerson)
                {
                    combatControl.isThirdPerson = false;
                    currentSpeed = walkSpeed;
                    zoomControl.Third_ZoomOut();
                }

                isCrouch = false;
                crouchTimer = 0;
                animator.SetTrigger("UnCrouch");
                animator.SetBool("isCrouch", isCrouch);
            }
            
            if (!isCrouch)
            {
                rb.velocity = new Vector3(targetDirection.x, rb.velocity.y, targetDirection.z);
            }
            
        }
        
    }

    private void GroundCheck()
    {
        // [�÷��̾� ���̾�� ����]
        int layerMask = ~playerLayer.value;

        // [�÷��̾ ���� ����ִ��� üũ]
        isGround = Physics.OverlapSphere(transform.position, 0.2f, layerMask).Length > 0;
    }

    private void CheckCar() // �÷��̾� �ֺ� ���� Ȯ��
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 3.0f);

        foreach (Collider c in colliders)
        {
            if (c.CompareTag("Car"))
            {
                nearCar = c.gameObject; // �ֺ� �� �Ҵ�
                return;
            }
        }
    }

    private void EnterCar() // ����
    {
        CheckCar(); // �ֺ� �� Ȯ��

        if (nearCar == null)
        {
            return;
        }

        animator.speed = 0;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        nightVision.SetActive(false);

        isCarEntered = true;
        isRun = false;
        currentSpeed = walkSpeed;

        playerModel.SetActive(false); // �÷��̾� �𵨸� Off

        // �� �𵨸� Off
        if (holdGunPivot.transform.childCount != 0)
        {
            holdGunPivot.transform.GetChild(0).gameObject.SetActive(false); 
        }
        else if (backGunPivot.transform.childCount != 0)
        {
            backGunPivot.transform.GetChild(0).gameObject.SetActive(false);
        }

        nearCar.GetComponent<CarControl>().EnterCar();
    }

    private void ExitCar() // ����
    {
        if (nearCar == null)
        {
            return;
        }

        isCarEntered = false;

        nearCar.GetComponent<CarControl>().ExitCar();

        animator.speed = 1.0f;
        rb.isKinematic = false;

        playerModel.SetActive(true); // �÷��̾� �𵨸� On

        // �� �𵨸� On
        if (holdGunPivot.transform.childCount != 0)
        {
            holdGunPivot.transform.GetChild(0).gameObject.SetActive(true); 
        }
        else if (backGunPivot.transform.childCount != 0)
        {
            backGunPivot.transform.GetChild(0).gameObject.SetActive(true);
        }

        transform.position = nearCar.GetComponent<CarControl>().playerPosition.position;
        transform.forward = nearCar.GetComponent<CarControl>().playerPosition.forward;
    }

    private void ToggleNightVision()
    {
        if (isFading)
        {
            return;
        }

        if (nightVision.activeSelf)
        {
            StartCoroutine(TurnOffNightVision());
        }
        else
        {
            StartCoroutine(TurnOnNightVision());
        }
    }

    private IEnumerator TurnOnNightVision()
    {
        isFading = true;

        nightVision.SetActive(true);

        while (nightVision.GetComponent<Volume>().weight < 1)
        {
            nightVision.GetComponent<Volume>().weight += Time.deltaTime * 10.0f;
            yield return null;
        }

        nightVision.transform.GetChild(0).gameObject.SetActive(true);
        nightVision.GetComponent<Volume>().weight = 1.0f;

        isFading = false;
    }

    private IEnumerator TurnOffNightVision()
    {
        isFading = true;

        nightVision.transform.GetChild(0).gameObject.SetActive(false);

        while (nightVision.GetComponent<Volume>().weight > 0)
        {
            nightVision.GetComponent<Volume>().weight -= Time.deltaTime * 5.0f;
            yield return null;
        }

        nightVision.GetComponent<Volume>().weight = 0f;

        nightVision.SetActive(false);

        isFading = false;
    }
}
