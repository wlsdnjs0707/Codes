using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect: MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(DisableEffect());
    }

    private IEnumerator DisableEffect()
    {
        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false);
        ObjectPoolControl.instance.hitEffectQueue.Enqueue(gameObject);

        yield break;
    }
}
