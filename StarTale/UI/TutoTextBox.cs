using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutoTextBox : MonoBehaviour
{
    [SerializeField] private GameObject textBox;
    [SerializeField] private TMP_Text text;

    private int textIndex = 0;

    public void ShowText()
    {
        TurnOn();

        switch (textIndex)
        {
            case 0:
                text.text = "좌, 우로 슬라이드하여 움직여보세요!";
                break;

            case 1:
                text.text = "장애물에 충돌하면 체력이 감소해요!";
                break;

            case 2:
                text.text = "하트를 획득하여 체력을 회복하세요!";
                break;

            case 3:
                text.text = "별을 모아 별자리를 완성하세요!";
                break;

            case 4:
                text.text = "숨겨진 공간에는 코인과 별이 있어요!";
                break;

            case 5:
                text.text = "모든 스테이지에는 숨겨진 공간이 존재해요!";
                break;

            default:
                break;
        }
    }

    public void TurnOn()
    {
        textBox.SetActive(true);
    }

    public void TurnOff()
    {
        textBox.SetActive(false);
    }
}
