using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionManager : MonoBehaviour
{
    [Header("Volume UI")]
    [SerializeField] private GameObject backgroundVolumeIcon;
    [SerializeField] private GameObject effectVolumeIcon;
    [SerializeField] private Sprite volumeSprite;
    [SerializeField] private Sprite muteSprite;
    [SerializeField] private Scrollbar backgroundScrollbar;
    [SerializeField] private Scrollbar effectScrollbar;
    private bool isBackgroundMuted = false;
    private bool isEffectMuted = false;

    [Header("Alarm UI")]
    [SerializeField] private GameObject[] alarmButtons;
    private bool[] bools = { true, true, true };

    private void Start()
    {
        // DB에 옵션 설정값 저장 후 불러오기 필요
        // 스크롤바 밸류 소리크기 연동 필요
        // 알람 on off 연동 필요
        backgroundScrollbar.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
        effectScrollbar.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
        backgroundScrollbar.value = AudioManager.Instance.bgmValue;
        effectScrollbar.value = AudioManager.Instance.sfxValue;
    }


    public void ToggleBackgroundVolume()
    {
        if (isBackgroundMuted)
        {
            backgroundVolumeIcon.GetComponent<Image>().sprite = volumeSprite;
            backgroundScrollbar.value = 0.6f;
        }
        else
        {
            backgroundVolumeIcon.GetComponent<Image>().sprite = muteSprite;
            backgroundScrollbar.value = 0;
        }

        isBackgroundMuted = !isBackgroundMuted;
    }
    
    public void ToggleEffectVolume()
    {
        if (isEffectMuted)
        {
            effectVolumeIcon.GetComponent<Image>().sprite = volumeSprite;
            effectScrollbar.value = 0.6f;
        }
        else
        {
            effectVolumeIcon.GetComponent<Image>().sprite = muteSprite;
            effectScrollbar.value = 0;
        }

        isEffectMuted = !isEffectMuted;
    }

    public void UpdateBackgroundVolume()
    {
        Debug.Log($"Background Volume : {backgroundScrollbar.value}");
        AudioManager.Instance.bgmValue = backgroundScrollbar.value;
    }

    public void UpdateEffectVolume()
    {
        Debug.Log($"Effect Volume : {effectScrollbar.value}");
        AudioManager.Instance.sfxValue = effectScrollbar.value;
    }

    public void ToggleAlarm(int index)
    {
        switch (index)
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                break;
            default:
                return;
        }

        if (bools[index])
        {
            alarmButtons[index].transform.GetChild(0).GetComponent<TMP_Text>().text = "꺼짐";
        }
        else
        {
            alarmButtons[index].transform.GetChild(0).GetComponent<TMP_Text>().text = "켜짐";
        }

        bools[index] = !bools[index];

    }

    public void CloseOptionPopup()
    {
        gameObject.SetActive(false);
    }
}
