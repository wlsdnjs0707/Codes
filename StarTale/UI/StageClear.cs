using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageClear : MonoBehaviour
{
    [SerializeField] private TMP_Text stageInfoText;
    [SerializeField] private GameObject character;

    [Header("Popup")]
    [SerializeField] GameObject tutorialRewardPopup; // 건물 획득 알림 팝업

    [Header("Sprite")]
    [SerializeField] Sprite[] characterBodyImages; // 캐릭터 Body 이미지 배열
    [SerializeField] Sprite[] characterFaceImages; // 캐릭터 Face 이미지 배열
    [SerializeField] Sprite[] tokenImages; // 토큰 이미지 배열

    [Header("Reward")]
    [SerializeField] private GameObject[] starIcons;
    [SerializeField] private TMP_Text rewardText_1;
    [SerializeField] private TMP_Text rewardText_2;
    [SerializeField] private TMP_Text rewardText_3;
    [SerializeField] private GameObject rewardTab_1;
    [SerializeField] private GameObject rewardTab_2;
    [SerializeField] private GameObject rewardTab_3;
    [SerializeField] private TMP_Text conditionText_2;
    [SerializeField] private TMP_Text conditionText_3;
    [SerializeField] private TMP_Text rewardText_gold;
    [SerializeField] private TMP_Text starCountText;
    [SerializeField] private GameObject tokenRewardIcon;

    [Header("Player")]
    private GameObject player;

    [Header("Stage")]
    public int stageIndex = 0; // 현재 클리어한 스테이지의 인덱스, 스테이지마다 연동 필요

    private void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void ShowClearUI()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        DBManager.instance.UseActivePoint();

        stageIndex = Utils.Instance.stageIndex;

        int chapter = 1;
        int stageLevel = ChartManager.instance.stageInfos[stageIndex].index + 1;
        string stageName = ChartManager.instance.stageInfos[stageIndex].name_k;
        stageInfoText.text = $"{chapter} - {stageLevel} {stageName}";

        if (stageLevel == 1)
        {
            tutorialRewardPopup.SetActive(true);
        }
        else
        {
            tutorialRewardPopup.SetActive(false);
        }

        int characterIndex = DBManager.instance.user.currentCharacterIndex;
        character.transform.GetChild(0).GetComponent<Image>().sprite = characterBodyImages[characterIndex];
        character.transform.GetChild(1).GetComponent<Image>().sprite = characterFaceImages[characterIndex / 4];

        int reward_1 = ChartManager.instance.stageInfos[stageIndex].reward_1;
        int reward_2 = ChartManager.instance.stageInfos[stageIndex].reward_2;
        int reward_gold = ChartManager.instance.stageInfos[stageIndex].reward_repeat;

        starCountText.text = $"획득한 별 개수 : {player.GetComponent<PlayerProperty>().stars.Count}";

        if (player.GetComponent<PlayerProperty>().stars.Count >= ChartManager.instance.stageInfos[stageIndex].condition_2)
        {
            starIcons[1].SetActive(true);
        }

        if (player.GetComponent<PlayerProperty>().stars.Count >= ChartManager.instance.stageInfos[stageIndex].condition_3)
        {
            starIcons[2].SetActive(true);
        }

        conditionText_2.text = $"별 X {ChartManager.instance.stageInfos[stageIndex].condition_2}";
        conditionText_3.text = $"별 X {ChartManager.instance.stageInfos[stageIndex].condition_3}";
        rewardText_1.text = $"{reward_1}";
        rewardText_2.text = $"{reward_2}";
        rewardText_3.text = $"1";
        tokenRewardIcon.GetComponent<Image>().sprite = tokenImages[stageIndex];
        rewardText_gold.text = $"{reward_gold}";

        CheckReward(player.GetComponent<PlayerProperty>().stars.Count);
    }

    public void CheckReward(int starCount)
    {
        // 반복보상 추가
        DBManager.instance.user.goods["gold"] += ChartManager.instance.stageInfos[stageIndex].reward_repeat;

        if (DBManager.instance.user.tokenInfo[stageIndex] == 0)
        {
            DBManager.instance.user.tokenInfo[stageIndex] = 1;
            DBManager.instance.user.tokens[stageIndex] += 1;
        }

        if (DBManager.instance.user.clearInfo[stageIndex, 3] == 0 && ChartManager.instance.stageInfos[stageIndex].reward_4 != -1)
        {
            DBManager.instance.user.clearInfo[stageIndex, 3] = 1;

            // 건물 해금
            if (DBManager.instance.user.housingObject.ContainsKey(ChartManager.instance.GetHousingObjectName(ChartManager.instance.stageInfos[stageIndex].reward_4)))
            {
                DBManager.instance.user.housingObject[ChartManager.instance.GetHousingObjectName(ChartManager.instance.stageInfos[stageIndex].reward_4)] += 1;
            }
            else
            {
                // 없을 때 Add
                DBManager.instance.user.housingObject.Add(ChartManager.instance.GetHousingObjectName(ChartManager.instance.stageInfos[stageIndex].reward_4), 1);
            }

        }

        if (stageIndex == 4)
        {
            rewardTab_1.SetActive(true);
            rewardTab_2.SetActive(true);
            rewardTab_3.SetActive(true);

            if (DBManager.instance.user.clearInfo[stageIndex, 2] == 0)
            {
                DBManager.instance.user.clearInfo[stageIndex, 2] = 1;
                DBManager.instance.user.tokens[stageIndex] += 1;
            }
            else
            {
                rewardText_3.text = $"획득 완료";
            }

            if (DBManager.instance.user.clearInfo[stageIndex, 1] == 0)
            {
                DBManager.instance.user.clearInfo[stageIndex, 1] = 1;
                DBManager.instance.user.goods["ruby"] += ChartManager.instance.stageInfos[stageIndex].reward_2;
            }
            else
            {
                rewardText_2.text = $"획득 완료";
            }

            if (DBManager.instance.user.clearInfo[stageIndex, 0] == 0)
            {
                DBManager.instance.user.clearInfo[stageIndex, 0] = 1;
                DBManager.instance.user.goods["gold"] += ChartManager.instance.stageInfos[stageIndex].reward_1;
            }
            else
            {
                rewardText_1.text = $"획득 완료";
            }

            return;
        }

        if (starCount >= ChartManager.instance.stageInfos[stageIndex].condition_3) // 3별 획득 조건 - 토큰
        {
            rewardTab_1.SetActive(true);
            rewardTab_2.SetActive(true);
            rewardTab_3.SetActive(true);

            if (DBManager.instance.user.clearInfo[stageIndex, 2] == 0)
            {
                DBManager.instance.user.clearInfo[stageIndex, 2] = 1;
                DBManager.instance.user.tokens[stageIndex] += 1;
            }
            else
            {
                rewardText_3.text = $"획득 완료";
            }

            if (DBManager.instance.user.clearInfo[stageIndex, 1] == 0)
            {
                DBManager.instance.user.clearInfo[stageIndex, 1] = 1;
                DBManager.instance.user.goods["ruby"] += ChartManager.instance.stageInfos[stageIndex].reward_2;
            }
            else
            {
                rewardText_2.text = $"획득 완료";
            }

            if (DBManager.instance.user.clearInfo[stageIndex, 0] == 0)
            {
                DBManager.instance.user.clearInfo[stageIndex, 0] = 1;
                DBManager.instance.user.goods["gold"] += ChartManager.instance.stageInfos[stageIndex].reward_1;
            }
            else
            {
                rewardText_1.text = $"획득 완료";
            }
        }
        else if (starCount >= ChartManager.instance.stageInfos[stageIndex].condition_2) // 2별 획득 조건 - 루비
        {
            rewardTab_1.SetActive(true);
            rewardTab_2.SetActive(true);
            rewardTab_3.SetActive(false);

            if (DBManager.instance.user.clearInfo[stageIndex, 1] == 0)
            {
                DBManager.instance.user.clearInfo[stageIndex, 1] = 1;
                DBManager.instance.user.goods["ruby"] += ChartManager.instance.stageInfos[stageIndex].reward_2;
            }
            else
            {
                rewardText_2.text = $"획득 완료";
            }

            if (DBManager.instance.user.clearInfo[stageIndex, 0] == 0)
            {
                DBManager.instance.user.clearInfo[stageIndex, 0] = 1;
                DBManager.instance.user.goods["gold"] += ChartManager.instance.stageInfos[stageIndex].reward_1;
            }
            else
            {
                rewardText_1.text = $"획득 완료";
            }
        }
        else if (starCount >= ChartManager.instance.stageInfos[stageIndex].condition_1) // 1별 획득 조건 - 골드
        {
            rewardTab_1.SetActive(true);
            rewardTab_2.SetActive(false);
            rewardTab_3.SetActive(false);

            if (DBManager.instance.user.clearInfo[stageIndex, 0] == 0)
            {
                DBManager.instance.user.clearInfo[stageIndex, 0] = 1;
                DBManager.instance.user.goods["gold"] += ChartManager.instance.stageInfos[stageIndex].reward_1;
            }
            else
            {
                rewardText_1.text = $"획득 완료";
            }
        }
    }
}
