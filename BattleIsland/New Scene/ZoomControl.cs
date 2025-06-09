using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ZoomControl : MonoBehaviour
{
    // Components
    private Animator animator;

    // Player
    private GameObject player;

    // Cameras
    [Header("Camera Object")]
    [SerializeField] private GameObject cameras;
    public CinemachineFreeLook normalCamera;
    public CinemachineVirtualCamera firstPersonCamera;
    public CinemachineFreeLook thirdPersonCamera;

    // Gun
    private GameObject gun;

    private void Awake()
    {
        // [애니메이터 할당]
        TryGetComponent(out animator);

        // [플레이어 오브젝트 할당]
        player = GameObject.FindGameObjectWithTag("Player");

        // [카메라 컴포넌트 할당]
        normalCamera = cameras.transform.Find("Normal Camera").GetComponent<CinemachineFreeLook>();
        firstPersonCamera = cameras.transform.Find("First Person Camera").GetComponent<CinemachineVirtualCamera>();
        thirdPersonCamera = cameras.transform.Find("Third Person Camera").GetComponent<CinemachineFreeLook>();

        // [할당 초기화된 경우 재 할당]
        if (normalCamera.Follow == null || normalCamera.LookAt == null)
        {
            normalCamera.Follow = player.transform.Find("Normal LookAt");
            normalCamera.LookAt = player.transform.Find("Normal LookAt");
        }
        if (firstPersonCamera.Follow == null)
        {
            firstPersonCamera.Follow = player.transform.Find("Normal LookAt");
        }
        if (thirdPersonCamera.Follow == null || thirdPersonCamera.LookAt == null)
        {
            thirdPersonCamera.Follow = player.transform.Find("Third Person LookAt");
            thirdPersonCamera.LookAt = player.transform.Find("Third Person LookAt");
        }
    }

    private void Update()
    {
        if (normalCamera.m_YAxis.Value <= 0.01f)
        {
            normalCamera.m_YAxis.Value = 0.01f;
        }
    }

    public void First_ZoomIn(GameObject currentGun)
    {
        // [총 모델만 비활성화]
        gun = currentGun;
        gun.transform.SetParent(transform);
        gun.transform.Find("Model").gameObject.SetActive(false);
        gun.transform.localPosition = Vector3.zero;
        gun.transform.localRotation = Quaternion.Euler(Vector3.zero);

        // [플레이어 모델 비활성화]
        player.transform.Find("Model").gameObject.SetActive(false);

        // [카메라 회전값 동기화]
        firstPersonCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value = normalCamera.m_XAxis.Value;
        firstPersonCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value = 100.0f * normalCamera.m_YAxis.Value - 50.0f;
        firstPersonCamera.gameObject.SetActive(true);

        StartCoroutine(SetCameraVerticalMaxSpeed_co());

        // [1인칭 UI 출력]
        if (GetComponent<CombatControl>().currentGun.GetComponent<Gun>().gunType == GunType.Rifle1 || GetComponent<CombatControl>().currentGun.GetComponent<Gun>().gunType == GunType.Rifle2)
        {
            UIManager.instance.FirstPersonRifleCrosshair(true);
        }
        else if (GetComponent<CombatControl>().currentGun.GetComponent<Gun>().gunType == GunType.Sniper1)
        {
            UIManager.instance.FirstPersonSniperCrosshair(true);
        }
    }

    public void First_ZoomOut()
    {
        // [총 모델 활성화]
        gun.transform.SetParent(GetComponent<CombatControl>().holdGunPivot);
        gun.transform.Find("Model").gameObject.SetActive(true);
        gun.transform.localPosition = Vector3.zero;
        gun.transform.localRotation = Quaternion.Euler(Vector3.zero);

        // [카메라 회전값 동기화]
        normalCamera.m_XAxis.Value = firstPersonCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value;
        normalCamera.m_YAxis.Value = 0.01f * firstPersonCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value + 0.5f;
        firstPersonCamera.gameObject.SetActive(false);

        StartCoroutine(SetActivePlayerModel());

        // [1인칭 UI 출력]
        if (GetComponent<CombatControl>().currentGun.GetComponent<Gun>().gunType == GunType.Rifle1 || GetComponent<CombatControl>().currentGun.GetComponent<Gun>().gunType == GunType.Rifle2)
        {
            UIManager.instance.FirstPersonRifleCrosshair(false);
        }
        else if (GetComponent<CombatControl>().currentGun.GetComponent<Gun>().gunType == GunType.Sniper1)
        {
            UIManager.instance.FirstPersonSniperCrosshair(false);
        }
    }

    public void Third_ZoomIn()
    {
        // [카메라 회전값 동기화]
        thirdPersonCamera.m_XAxis.Value = normalCamera.m_XAxis.Value;
        thirdPersonCamera.m_YAxis.Value = normalCamera.m_YAxis.Value;
        thirdPersonCamera.gameObject.SetActive(true);

        StartCoroutine(SetCameraVerticalMaxSpeed_co());

        if (GetComponent<CombatControl>().currentWeapon == Weapon.Gun)
        {
            // [1인칭 UI 출력]
            UIManager.instance.ThirdPersonCrosshair(true);

            // [애니메이션]
            animator.SetTrigger("Aim");
        }
    }

    public void Third_ZoomOut()
    {
        // [카메라 회전값 동기화]
        normalCamera.m_XAxis.Value = thirdPersonCamera.m_XAxis.Value;
        normalCamera.m_YAxis.Value = thirdPersonCamera.m_YAxis.Value;
        thirdPersonCamera.gameObject.SetActive(false);

        if (GetComponent<CombatControl>().currentWeapon == Weapon.Gun)
        {
            // [1인칭 UI 출력]
            UIManager.instance.ThirdPersonCrosshair(false);

            // [애니메이션]
            animator.SetTrigger("UnAim");
        }
    }

    private IEnumerator SetCameraVerticalMaxSpeed_co()
    {
        if (GetComponent<CombatControl>().isFirstPerson)
        {
            firstPersonCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = 0.0f;
            firstPersonCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = 0.0f;

            yield return new WaitForSeconds(0.25f);

            firstPersonCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = 250.0f;
            firstPersonCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = 250.0f;
        }
        else if (GetComponent<CombatControl>().isThirdPerson)
        {
            thirdPersonCamera.m_YAxis.m_MaxSpeed = 0.0f;
            thirdPersonCamera.m_XAxis.m_MaxSpeed = 0.0f;

            yield return new WaitForSeconds(0.25f);

            thirdPersonCamera.m_YAxis.m_MaxSpeed = 2.5f;
            thirdPersonCamera.m_XAxis.m_MaxSpeed = 300.0f;
        }
    }

    public void StartLookAround()
    {
        player.GetComponent<CombatControl>().normalCamX = normalCamera.m_XAxis.Value;
        player.GetComponent<CombatControl>().normalCamY = normalCamera.m_YAxis.Value;
    }

    public void EndLookAround()
    {
        normalCamera.m_XAxis.Value = player.GetComponent<CombatControl>().normalCamX;
        normalCamera.m_YAxis.Value = player.GetComponent<CombatControl>().normalCamY;
    }

    private IEnumerator SetActivePlayerModel()
    {
        yield return new WaitForSeconds(0.05f);

        // [플레이어 모델 활성화]
        player.transform.Find("Model").gameObject.SetActive(true);
    }
}
