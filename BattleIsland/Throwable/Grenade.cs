using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject model;

    public float timeDelay = 5.0f; // ���߱��� �ɸ��� �ð�
    private float explodeRange = 5.0f;

    public void StartTimer()
    {
        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        float timeLeft = timeDelay;

        while (timeLeft > 0) // �ð��� �� �帣��
        {
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        Explode(); // ����
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explodeRange); // ���� ���� �ݶ��̴� ����

        foreach (Collider c in colliders)
        {
            if (c.CompareTag("Enemy"))
            {
                float damage = 0f;

                // [ �Ÿ��� ���� ������ ���]
                if (explodeRange * 0.75f >= Vector3.Magnitude(c.transform.position - transform.position))
                {
                    damage = 200.0f;
                }
                else if (explodeRange * 0.75f < Vector3.Magnitude(c.transform.position - transform.position))
                {
                    damage = Vector3.Magnitude(c.transform.position - transform.position) * (-200.0f / 3.0f) + (1000.0f / 3.0f);
                }

                c.GetComponent<EnemyHealth>().TakeDamage((int)damage, c.transform.position - transform.position);

                //Debug.Log($"����ź : {c.name}���� {(int)damage}�� ������");
            }

            if (c.CompareTag("Player"))
            {
                float damage = 0f;

                // [ �Ÿ��� ���� ������ ���]
                if (explodeRange * 0.75f >= Vector3.Magnitude(c.transform.position - transform.position))
                {
                    damage = 200.0f;
                }
                else if (explodeRange * 0.75f < Vector3.Magnitude(c.transform.position - transform.position))
                {
                    damage = Vector3.Magnitude(c.transform.position - transform.position) * (-200.0f / 3.0f) + (1000.0f / 3.0f);
                }

                c.GetComponent<CombatControl>().TakeDamage((int)damage);

                //Debug.Log($"����ź : {c.name}���� {(int)damage}�� ������");
                //Debug.Log($"player health : {c.GetComponent<CombatControl>().playerHealth}");
            }
        }

        model.SetActive(false);

        Instantiate(explosionPrefab, transform.position, Quaternion.identity); // ���� ����Ʈ ����
        Destroy(gameObject);
    }
}
