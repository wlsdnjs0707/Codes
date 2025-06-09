using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CarControl : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private CinemachineFreeLook carCamera; // 자동차 3인칭 카메라
    public Transform playerPosition; // 플레이어가 내릴 위치
    [SerializeField] private Transform frontHandle; // 차가 앞으로 갈 때 적용할 방향
    [SerializeField] private Transform backHandle; // 차가 뒤로 갈 때 적용할 방향
    [SerializeField] private GameObject leftWheel; // 왼쪽 바퀴
    [SerializeField] private GameObject rightWheel; // 오른쪽 바퀴
    [SerializeField] private GameObject headLight; // 라이트

    private float x; // 좌우 (각도)
    private float z; // 앞뒤 (속력)

    public int carSpeed; // 자동차 속력

    public bool isPlayerEntered = false; // 차량에 탑승했는지 확인

    private bool isIgnited = false; // 시동 걸었나

    private bool isLightOn = false; // 라이트 On, Off 여부

    [Header("Audio")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip doorClip;
    [SerializeField] private AudioClip ignitionClip;
    [SerializeField] private AudioClip drivingClip;

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out audioSource);

        StartCoroutine(CarAwake_co());
    }

    private void Update()
    {
        if (!isPlayerEntered)
        {
            return;
        }

        // 라이트 On, Off
        if (isPlayerEntered && Input.GetKeyDown(KeyCode.N))
        {
            ToggleLight();
        }

        if (z != 0)
        {
            carSpeed = Mathf.Abs((int)(z * 4.0f));
        }

        GetKeyboardInput();
        
    }

    private void FixedUpdate()
    {
        if (!isPlayerEntered)
        {
            return;
        }

        CarMove();
    }

    private void GetKeyboardInput()
    {
        // 앞뒤 입력
        if (Input.GetKey(KeyCode.W)) // 앞으로 갈 때
        {
            if (!isIgnited)
            {
                isIgnited = true;
                audioSource.PlayOneShot(ignitionClip);
                StartCoroutine(DrivingSound_co());
            }

            if (z < 25)
            {
                if (z < 0)
                {
                    z += Time.deltaTime * 20.0f;
                }
                else
                {
                    z += Time.deltaTime * 5.0f;
                }
            }

            if (z > 25) z = 25;
        }
        else if (Input.GetKey(KeyCode.S)) // 뒤로 갈 때
        {
            if (!isIgnited)
            {
                isIgnited = true;
                audioSource.PlayOneShot(ignitionClip);
                StartCoroutine(DrivingSound_co());
            }

            if (z > -10)
            {
                if (z > 0)
                {
                    z -= Time.deltaTime * 20.0f;
                }
                else
                {
                    z -= Time.deltaTime * 5.0f;
                }
            }

            if (z < -10) z = -10;
        }
        else // z축 입력 없을 때 0으로
        {
            if (z > 0)
            {
                z -= Time.deltaTime * 10.0f;
            }
            else if (z < 0)
            {
                z += Time.deltaTime * 10.0f;
            }

            if (Mathf.Abs(z) < 0.1f) // 0으로 보정
            {
                z = 0;
            }
        }

        float xLimit = 1.0f;

        // 좌우 입력
        if (Input.GetKey(KeyCode.D)) // 오른쪽으로 갈 때
        {
            if (x < 0)
            {
                x = 0;
            }

            if (x < xLimit)
            {
                x += Time.deltaTime * 30.0f;
            }

            if (x > xLimit) x = xLimit;
        }
        else if (Input.GetKey(KeyCode.A)) // 왼쪽으로 갈 때
        {
            if (x > 0)
            {
                x = 0;
            }

            if (x > -xLimit)
            {
                x -= Time.deltaTime * 20.0f;
            }

            if (x < -xLimit) x = -xLimit;
        }
        else // x축 입력 없을 때 0으로
        {
            if (x > 0)
            {
                x -= Time.deltaTime * 20.0f;
            }
            else if (x < 0)
            {
                x += Time.deltaTime * 20.0f;
            }

            if (Mathf.Abs(x) < 0.2f) // 0으로 보정
            {
                x = 0;
            }
        }

        frontHandle.localRotation = Quaternion.Euler(new Vector3(0, x, 0)); // 방향 각도 조절
        backHandle.localRotation = Quaternion.Euler(new Vector3(0, -x, 0)); // 방향 각도 조절

        leftWheel.transform.localRotation = Quaternion.Euler(new Vector3(0, x * 30.0f, 0)); // 왼쪽 바퀴 회전
        rightWheel.transform.localRotation = Quaternion.Euler(new Vector3(0, -180.0f + x * 30.0f, 0)); // 오른쪽 바퀴 회전
    }

    private void CarMove()
    {
        Vector3 frontDirection = frontHandle.forward * z; // 앞으로 갈 때 방향
        Vector3 backDirection = backHandle.forward * z; // 뒤로 갈 때 방향

        Vector3 targetDirection = frontDirection + backDirection;

        if (z > 0)
        {
            transform.forward = frontHandle.forward;
            //rb.MovePosition(rb.position + frontDirection * Time.deltaTime); // 차량 이동
            rb.velocity = new Vector3(targetDirection.x, rb.velocity.y, targetDirection.z);
        }
        else if (z < 0)
        {
            transform.forward = backHandle.forward;
            //rb.MovePosition(rb.position + backDirection * Time.deltaTime); // 차량 이동
            rb.velocity = new Vector3(targetDirection.x, rb.velocity.y, targetDirection.z);
        }
    }

    private IEnumerator CarAwake_co()
    {
        yield return new WaitForSeconds(2.0f);

        rb.isKinematic = true;
    }

    private void ToggleLight()
    {
        if (isLightOn)
        {
            isLightOn = false;

            for (int i = 0; i < headLight.transform.childCount; i++)
            {
                headLight.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            isLightOn = true;

            for (int i = 0; i < headLight.transform.childCount; i++)
            {
                headLight.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    public void EnterCar()
    {
        audioSource.PlayOneShot(doorClip);
        rb.velocity = Vector3.zero;
        isPlayerEntered = true;
        rb.isKinematic = false;
        carCamera.gameObject.SetActive(true);
    }

    public void ExitCar()
    {
        audioSource.Stop();
        isIgnited = false;
        audioSource.loop = false;
        audioSource.PlayOneShot(doorClip);
        x = 0;
        z = 0;
        rb.velocity = Vector3.zero;
        isPlayerEntered = false;
        rb.isKinematic = true;
        carCamera.gameObject.SetActive(false);
    }

    private IEnumerator DrivingSound_co()
    {
        yield return new WaitForSeconds(1.0f);
        audioSource.volume = 0.25f;
        audioSource.loop = true;
        audioSource.Play();
    }
}
