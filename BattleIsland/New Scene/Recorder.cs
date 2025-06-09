using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Recorder : MonoBehaviour
{
    public static Recorder instance;

    // Cameras
    [Header("Camera Object")]
    [SerializeField] private GameObject cameras;
    private CinemachineFreeLook bulletCamera;

    // Datas
    private Transform enemyTransform;
    private Vector3 bulletDirection;

    private GameObject enemy;
    private Vector3 bulletStartPosition;

    public GameObject bulletPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        bulletCamera = cameras.transform.Find("Bullet Camera").GetComponent<CinemachineFreeLook>();
    }

    public void UpdateData(GameObject enemy, Vector3 bulletStartPosition , Vector3 bulletDirection)
    {
        Debug.Log("UPDATE DATA");

        this.enemy = enemy;
        this.bulletStartPosition = bulletStartPosition;
        this.enemyTransform = enemy.transform;
        this.bulletDirection = bulletDirection;
    }

    public void Replay()
    {
        UIManager.instance.TurnOffUI();
        GetComponent<CharacterMovement>().canMove = false;

        // 적 위치 설정
        //Instantiate(enemy, enemyTransform.position, Quaternion.identity);

        // 총알 발사
        GameObject endBullet = Instantiate(bulletPrefab, bulletStartPosition, Quaternion.identity);
        endBullet.transform.forward = bulletDirection;

        // 카메라가 총알 추적
        bulletCamera.Follow = endBullet.transform;
        bulletCamera.LookAt = endBullet.transform;
        bulletCamera.gameObject.SetActive(true);
    }

    public void EndReplay()
    {
        GameManager.instance.isWin = true;
        //GetComponent<CharacterMovement>().canMove = true;
        bulletCamera.gameObject.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
