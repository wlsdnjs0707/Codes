using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndBullet : MonoBehaviour
{
    public float gravity = 9.8f;
    public float bulletSpeed = 1.0f;

    private Rigidbody rb;

    private void Awake()
    {
        TryGetComponent(out rb);
    }

    private void Start()
    {
        rb.velocity = transform.forward * bulletSpeed; // Bullet 앞으로 이동
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    private void OnCollisionEnter(Collision collision) // 충돌 처리
    {
        if (!collision.collider.CompareTag("Player")) // 무시
        {
            Vector3 hitDirection = (collision.contacts[0].point - transform.position).normalized; // 충돌 방향
            if (collision.collider.transform.root.CompareTag("Enemy") || collision.collider.transform.CompareTag("Head"))
            {
                collision.collider.transform.root.GetComponent<EnemyHealth>().TakeDamage_End(1000f, hitDirection);
                Debug.Log("GAME OVER");

                Recorder.instance.EndReplay();
            }

            Destroy(gameObject);
        }
    }
}
