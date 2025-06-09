using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairControl : MonoBehaviour
{
    [SerializeField] private GameObject up;
    [SerializeField] private GameObject down;
    [SerializeField] private GameObject left;
    [SerializeField] private GameObject right;
    [SerializeField] private GameObject dot;

    private float originalDistance;

    private float target;

    public float expandAmount = 20.0f;
    public float expandSpeed = 35.0f;

    private bool isRed = false;

    private void Awake()
    {
        originalDistance = up.GetComponent<RectTransform>().anchoredPosition.y;
    }

    private void Update()
    {
        if (target > originalDistance)
        {
            target -= 25.0f* Time.deltaTime;
        }
        
        if (target < originalDistance)
        {
            target = originalDistance;
        }

        float upValue;
        float downValue;
        float leftValue;
        float rightValue;

        upValue = Mathf.Lerp(up.GetComponent<RectTransform>().anchoredPosition.y, target, expandSpeed * Time.deltaTime);
        downValue = Mathf.Lerp(down.GetComponent<RectTransform>().anchoredPosition.y, -target, expandSpeed * Time.deltaTime);
        leftValue = Mathf.Lerp(left.GetComponent<RectTransform>().anchoredPosition.x, -target, expandSpeed * Time.deltaTime);
        rightValue = Mathf.Lerp(right.GetComponent<RectTransform>().anchoredPosition.x, target, expandSpeed * Time.deltaTime);

        up.GetComponent<RectTransform>().anchoredPosition = new Vector2(up.GetComponent<RectTransform>().anchoredPosition.x, upValue);
        down.GetComponent<RectTransform>().anchoredPosition = new Vector2(down.GetComponent<RectTransform>().anchoredPosition.x, downValue);
        left.GetComponent<RectTransform>().anchoredPosition = new Vector2(leftValue, left.GetComponent<RectTransform>().anchoredPosition.y);
        right.GetComponent<RectTransform>().anchoredPosition = new Vector2(rightValue, right.GetComponent<RectTransform>().anchoredPosition.y);
    }

    public void Expand()
    {
        target = originalDistance + expandAmount;
    }

    public void TurnRed()
    {
        if (isRed)
        {
            return;
        }

        isRed = true;

        up.GetComponent<Image>().color = Color.red;
        down.GetComponent<Image>().color = Color.red;
        left.GetComponent<Image>().color = Color.red;
        right.GetComponent<Image>().color = Color.red;
        dot.GetComponent<Image>().color = Color.red;

        StartCoroutine(TurnWhite());
    }

    private IEnumerator TurnWhite()
    {
        yield return new WaitForSeconds(1.0f);

        up.GetComponent<Image>().color = Color.white;
        down.GetComponent<Image>().color = Color.white;
        left.GetComponent<Image>().color = Color.white;
        right.GetComponent<Image>().color = Color.white;
        dot.GetComponent<Image>().color = Color.white;

        isRed = false;
    }
}
