using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootTest : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform shootStartPoint;

    private CharacterMovement characterMovement;
    private bool canShoot = true;

    public float coolDown = 0.25f;

    private void Awake()
    {
        TryGetComponent(out characterMovement);
    }

    public void Shoot() // for Test
    {
        if (canShoot)
        {
            StartCoroutine(Shoot_co());
        }
    }

    private IEnumerator Shoot_co()
    {
        float timer = coolDown; // �߻� ��Ÿ��

        canShoot = false;

        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f)); // ȭ�� �߾� (ũ�ν���� ��ġ)�� Ray ���
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
        {
            GameObject currentBullet = Instantiate(bulletPrefab, shootStartPoint.position, Quaternion.identity);
            currentBullet.transform.forward = raycastHit.point - shootStartPoint.position;
            currentBullet.GetComponent<Bullet>().bulletDamage = 10.0f;
            currentBullet.GetComponent<Bullet>().hit = raycastHit;
        }

        //
        // ���� �Ѿ� ��� �ʿ�
        //

        // ��Ÿ�� ���
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        canShoot = true;
    }
}
