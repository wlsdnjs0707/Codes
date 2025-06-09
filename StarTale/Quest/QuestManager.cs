using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class QuestManager : MonoBehaviour
{
    [Header("Time Left Text")]
    [SerializeField] private TMP_Text timeLeftText;

    [Header("Day Week Tab")]
    [SerializeField] private Button dayTabButton;
    [SerializeField] private Button weekTabButton;

    [Header("Reward Tab")]
    [SerializeField] private GameObject[] checkIcons;
    private int[] rewardsQuantity = { 1, 3, 5, 7, 10 };
    private int[] rewards = { 500, 700, 800, 1000, 1 };

    [Header("Quest Tab")]
    [SerializeField] private GameObject questPrefab;
    [SerializeField] private Transform content;

    private void OnEnable()
    {
        DBManager.instance.TimerEvent += UpdateTimeText;
    }

    private void OnDisable()
    {
        DBManager.instance.TimerEvent -= UpdateTimeText;
    }

    public void OpenDayTab()
    {
        dayTabButton.interactable = false;
        weekTabButton.interactable = true;

        DBManager.instance.AllResetCheck();

        ShowDayQuestList();
    }

    public void OpenWeekTab()
    {
        dayTabButton.interactable = true;
        weekTabButton.interactable = false;
    }

    public void ShowDayQuestList()
    {
        for (int i = 0; i < 5; i++)
        {
            if (DBManager.instance.user.questRewardCount >= rewardsQuantity[i])
            {
                checkIcons[i].SetActive(true);

                if (i < 4)
                {
                    if (DBManager.instance.user.questRewardInfo[i] == 0)
                    {
                        DBManager.instance.user.questRewardInfo[i] = 1;
                        DBManager.instance.user.goods["gold"] += rewards[i];
                    }
                }
                else
                {
                    // 랜덤 토큰 획득 메서드 구현 필요
                }
            }
            else
            {
                checkIcons[i].SetActive(false);
            }
        }

        for (int i = 0; i < ChartManager.instance.quests.Count; i++)
        {
            GameObject questTab = Instantiate(questPrefab);
            questTab.transform.SetParent(content);

            questTab.transform.GetChild(1).GetComponent<TMP_Text>().text = ChartManager.instance.quests[i].name;

            if (DBManager.instance.user.dayQuestInfo[i] == 0)
            {
                questTab.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = $"진행중";
            }
            else if (DBManager.instance.user.dayQuestInfo[i] == 1)
            {
                questTab.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = $"완료";
            }
        }
    }

    public void CheckDayQuest()
    {
        for (int i = 0; i < ChartManager.instance.quests.Count; i++)
        {
            switch (i)
            {
                case 0:
                    // 게임 접속
                    if (DBManager.instance.user.dayQuestInfo[i] == 0)
                    {
                        DBManager.instance.user.dayQuestInfo[i] = 1;
                        DBManager.instance.user.questRewardCount += 1;
                    }
                    break;
            }
        }
    }

    public void UpdateTimeText(object sender, EventArgs e)
    {
        timeLeftText.text = DBManager.instance.user.timeLeftText;
    }

    public void OpenQuestPopup()
    {
        gameObject.SetActive(true);
    }

    public void CloseQuestPopup()
    {
        gameObject.SetActive(false);
    }
}
