using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndicatorControl : MonoBehaviour
{
    // UI
    [Header("UI")]
    [SerializeField] private GameObject indicator;

    // Target
    private bool hasTarget = true;
    public GameObject target;
    public float detectRange = 5.0f;


    private void Update()
    {
        if (target != null && hasTarget) // ǥ���� Ÿ��(����ź)�� �ִ� ���
        {
            CheckDistance(); // ����ź�� �÷��̾� �Ÿ� üũ

            // 2���� UI�� rotation���� 3���������� rotation�� ���
            Vector3 direction = target.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            rotation.z = -rotation.y;
            rotation.x = 0;
            rotation.y = 0;

            indicator.transform.localRotation = rotation * Quaternion.Euler(0, 0, transform.eulerAngles.y);
        }
        else if (hasTarget)
        {
            hasTarget = false;
            ToggleIndicator(false);
        }
    }

    public void ToggleIndicator(bool on)
    {
        if (!on && indicator.activeSelf)
        {
            indicator.SetActive(false);
        }
        else if (on && !indicator.activeSelf)
        {
            indicator.SetActive(true);
            hasTarget = true;
        }
    }

    private void CheckDistance()
    {
        if (Vector3.Magnitude(transform.position - target.transform.position) < detectRange)
        {
            indicator.transform.Find("Triangle").GetComponent<Image>().color = Color.red;
            indicator.transform.Find("Grenade").GetComponent<Image>().color = Color.red;
        }
        else
        {
            indicator.transform.Find("Triangle").GetComponent<Image>().color = Color.white;
            indicator.transform.Find("Grenade").GetComponent<Image>().color = Color.white;
        }
    }
}
