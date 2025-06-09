using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject model;

    public float timeDelay = 5.0f; // 폭발까지 걸리는 시간
    private float explodeRange = 5.0f;

    public void StartTimer()
    {
        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        float timeLeft = timeDelay;

        while (timeLeft > 0) // 시간이 다 흐르면
        {
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        Explode(); // 폭발
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explodeRange); // 폭발 범위 콜라이더 검출

        foreach (Collider c in colliders)
        {
            if (c.CompareTag("Enemy"))
            {
                float damage = 0f;

                // [ 거리에 따른 데미지 계산]
                if (explodeRange * 0.75f >= Vector3.Magnitude(c.transform.position - transform.position))
                {
                    damage = 200.0f;
                }
                else if (explodeRange * 0.75f < Vector3.Magnitude(c.transform.position - transform.position))
                {
                    damage = Vector3.Magnitude(c.transform.position - transform.position) * (-200.0f / 3.0f) + (1000.0f / 3.0f);
                }

                c.GetComponent<EnemyHealth>().TakeDamage((int)damage, c.transform.position - transform.position);

                //Debug.Log($"수류탄 : {c.name}에게 {(int)damage}의 데미지");
            }

            if (c.CompareTag("Player"))
            {
                float damage = 0f;

                // [ 거리에 따른 데미지 계산]
                if (explodeRange * 0.75f >= Vector3.Magnitude(c.transform.position - transform.position))
                {
                    damage = 200.0f;
                }
                else if (explodeRange * 0.75f < Vector3.Magnitude(c.transform.position - transform.position))
                {
                    damage = Vector3.Magnitude(c.transform.position - transform.position) * (-200.0f / 3.0f) + (1000.0f / 3.0f);
                }

                c.GetComponent<CombatControl>().TakeDamage((int)damage);

                //Debug.Log($"수류탄 : {c.name}에게 {(int)damage}의 데미지");
                //Debug.Log($"player health : {c.GetComponent<CombatControl>().playerHealth}");
            }
        }

        model.SetActive(false);

        Instantiate(explosionPrefab, transform.position, Quaternion.identity); // 폭발 이펙트 생성
        Destroy(gameObject);
    }
}
