using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private GameObject thirdPersonCrosshair;

    public Vector3 startPostion;

    public float gravity = 9.8f;
    public float bulletSpeed = 1.0f;
    public float bulletDamage = 0;

    public RaycastHit hit;

    private Rigidbody rb;

    private void Awake()
    {
        TryGetComponent(out rb);
    }

    private void OnEnable()
    {
        rb.velocity = transform.forward * bulletSpeed; // Bullet ������ �̵�
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration); // �߷� �ۿ� (ź����)
    }

    private void OnCollisionEnter(Collision collision) // �浹 ó��
    {
        if (!collision.collider.transform.root.CompareTag("Player") && !collision.collider.transform.root.CompareTag("Weapon") && !collision.collider.transform.root.CompareTag("EnemyBullet")) // ����
        {
            Vector3 hitDirection = (collision.contacts[0].point - transform.position).normalized; // �浹 ����

            if (collision.collider.transform.root.CompareTag("Wall")) // �Ѿ� ����Ʈ ������ ������
            {
                // ������Ʈ Ǯ���� ������ ���
                if (ObjectPoolControl.instance.hitEffectQueue.Count > 0)
                {
                    GameObject currentHitEffect = ObjectPoolControl.instance.hitEffectQueue.Dequeue();
                    currentHitEffect.transform.position = collision.contacts[0].point;
                    currentHitEffect.transform.rotation = Quaternion.Euler(hitDirection);
                    currentHitEffect.SetActive(true);
                }

                if (hit.point != null)
                {
                    GameObject bulletHole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));
                    Destroy(bulletHole, 10.0f);
                }
            }

            if (collision.collider.CompareTag("Helicopter"))
            {
                // ������Ʈ Ǯ���� ������ ���
                if (ObjectPoolControl.instance.hitEffectQueue.Count > 0)
                {
                    GameObject currentHitEffect = ObjectPoolControl.instance.hitEffectQueue.Dequeue();
                    currentHitEffect.transform.position = collision.contacts[0].point;
                    currentHitEffect.transform.rotation = Quaternion.Euler(hitDirection);
                    currentHitEffect.SetActive(true);
                }

                if (hit.point != null)
                {
                    GameObject bulletHole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));
                    Destroy(bulletHole, 10.0f);
                }

                collision.collider.transform.root.GetComponent<HelicopterHealth>().TakeDamage(bulletDamage);
                //Recorder.instance.UpdateData(collision.collider.gameObject, startPostion, transform.forward);
            }

            if (collision.collider.transform.root.CompareTag("Enemy"))
            {
                /*if (thirdPersonCrosshair == null)
                {
                    thirdPersonCrosshair = FindObjectOfType<CrosshairControl>().gameObject;
                }

                thirdPersonCrosshair.GetComponent<CrosshairControl>().TurnRed();*/

                collision.collider.transform.root.GetComponent<EnemyHealth>().TakeDamage(bulletDamage, hitDirection);
                Recorder.instance.UpdateData(collision.collider.transform.root.gameObject, startPostion ,transform.forward);
            }

            if (collision.collider.CompareTag("Head"))
            {
                collision.collider.transform.root.GetComponent<EnemyHealth>().TakeDamage(bulletDamage * 4.0f, hitDirection);
                Recorder.instance.UpdateData(collision.collider.transform.root.gameObject, startPostion, transform.forward);
            }

            // ������Ʈ Ǯ�� �ֱ�
            gameObject.SetActive(false);
            ObjectPoolControl.instance.bulletQueue.Enqueue(gameObject);
        }
    }
}
