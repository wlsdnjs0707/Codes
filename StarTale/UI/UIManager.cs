using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using System;
using System.Globalization;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static EventHandler OnChangeCurrentCharacter;


    [Header("Tab")]
    [SerializeField] GameObject characterSelectTab;
    [SerializeField] GameObject tailTab;

    [Header("Character Select UI")]
    [SerializeField] GameObject characterSelectPopup; // 캐릭터 선택 팝업
    private int characterTabIndex = 0; // 캐릭터 탭(3개) 중 현재 탭 인덱스
    [SerializeField] GameObject[] characterButtons; // 캐릭터 버튼 9개
    [SerializeField] GameObject[] characterTabCounter; // 캐릭터 탭 3개
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text sightRangeText;
    [SerializeField] Sprite[] characterBodyImages; // 캐릭터 Body 이미지 배열
    [SerializeField] Sprite[] characterFaceImages; // 캐릭터 Face 이미지 배열
    private List<Character> characters;
    private List<Character> characterDatas;
    [SerializeField] GameObject currentCharacter; // 선택한 캐릭터

    [Header("Tail Select UI")]
    private int tailTabIndex = 0;
    [SerializeField] Image currentTailImage;
    [SerializeField] Image effectImage;
    [SerializeField] GameObject[] tailButtons; // 꼬리 버튼 9개
    [SerializeField] GameObject[] tailTabCounter; // 꼬리 탭 3개
    [SerializeField] Sprite[] tailImages; // 꼬리 이미지 배열

    [Header("Character Info UI")]
    [SerializeField] GameObject characterInfoPopup; // 캐릭터 정보 팝업
    private int index = 0;
    [SerializeField] TMP_Text characterName;
    [SerializeField] Image body;
    [SerializeField] Image face;
    [SerializeField] Image gauge;
    [SerializeField] Image starIcon;
    [SerializeField] Sprite star;
    [SerializeField] Sprite star_red;
    [SerializeField] TMP_Text gaugeText;
    [SerializeField] TMP_Text typeText;
    [SerializeField] TMP_Text healthText2;
    [SerializeField] TMP_Text sightText2;

    [Header("Option")]
    [SerializeField] GameObject OptionPopup;

    [Header("Friends UI")]
    [SerializeField] GameObject friendsPopup;

    [Header("Mail UI")]
    [SerializeField] GameObject mailPopup;

    [Header("Goods UI - Lobby")]
    [SerializeField] TMP_Text friendshipPointText;
    [SerializeField] TMP_Text rubyText;
    [SerializeField] TMP_Text goldText_1;

    [Header("Goods UI - StageSelect")]
    [SerializeField] TMP_Text activePointCountText;
    [SerializeField] TMP_Text activePointTimeText;
    [SerializeField] TMP_Text goldText_2;

    private Coroutine timerCoroutine = null;

    private void OnEnable()
    {
        UpdateGoods();

        DBManager.instance.CheckActivePoint();
        timerCoroutine = StartCoroutine(UpdateTimeLeftText_co());
    }

    private void OnDisable()
    {
        StopCoroutine(timerCoroutine);
    }

    public void UpdateGoods()
    {
        // Friendship Point
        if (DBManager.instance.user.goods["friendshipPoint"] >= 10000000)
        {
            friendshipPointText.text = String.Format("{0:0,0}M", DBManager.instance.user.goods["friendshipPoint"] / 1000000);
        }
        else if (DBManager.instance.user.goods["friendshipPoint"] >= 10000)
        {
            friendshipPointText.text = String.Format("{0:0,0}K", DBManager.instance.user.goods["friendshipPoint"] / 1000);
        }
        else
        {
            friendshipPointText.text = DBManager.instance.user.goods["friendshipPoint"].ToString();
        }

        // Ruby
        if (DBManager.instance.user.goods["ruby"] >= 10000000)
        {
            rubyText.text = String.Format("{0:0,0}M", DBManager.instance.user.goods["ruby"] / 1000000);
        }
        else if (DBManager.instance.user.goods["ruby"] >= 10000)
        {
            rubyText.text = String.Format("{0:0,0}K", DBManager.instance.user.goods["ruby"] / 1000);
        }
        else
        {
            rubyText.text = DBManager.instance.user.goods["ruby"].ToString();
        }

        // Gold
        if (DBManager.instance.user.goods["gold"] >= 10000000)
        {
            goldText_1.text = String.Format("{0:0,0}M", DBManager.instance.user.goods["gold"] / 1000000);
            goldText_2.text = String.Format("{0:0,0}M", DBManager.instance.user.goods["gold"] / 1000000);
        }
        else if (DBManager.instance.user.goods["gold"] >= 10000)
        {
            goldText_1.text = String.Format("{0:0,0}K", DBManager.instance.user.goods["gold"] / 1000);
            goldText_2.text = String.Format("{0:0,0}K", DBManager.instance.user.goods["gold"] / 1000);
        }
        else
        {
            goldText_1.text = DBManager.instance.user.goods["gold"].ToString();
            goldText_2.text = DBManager.instance.user.goods["gold"].ToString();
        }

        activePointCountText.text = $"{DBManager.instance.user.activePoint}";
    }

    public void NextTab()
    {
        if (characterTabIndex < 2)
        {
            characterTabIndex += 1;

            for (int i = 0; i < 3; i++)
            {
                if (i == characterTabIndex)
                {
                    Color color = characterTabCounter[i].GetComponent<Image>().color;
                    color.a = 1.0f;
                    characterTabCounter[i].GetComponent<Image>().color = color;
                }
                else
                {
                    Color color = characterTabCounter[i].GetComponent<Image>().color;
                    color.a = 0.2f;
                    characterTabCounter[i].GetComponent<Image>().color = color;
                }
            }
        }

        UpdateCharacterButton();
    }

    public void PrevTab()
    {
        if (characterTabIndex > 0)
        {
            characterTabIndex -= 1;
        }

        for (int i = 0; i < 3; i++)
        {
            if (i == characterTabIndex)
            {
                Color color = characterTabCounter[i].GetComponent<Image>().color;
                color.a = 1.0f;
                characterTabCounter[i].GetComponent<Image>().color = color;
            }
            else
            {
                Color color = characterTabCounter[i].GetComponent<Image>().color;
                color.a = 0.2f;
                characterTabCounter[i].GetComponent<Image>().color = color;
            }
        }

        UpdateCharacterButton();
    }

    public void NextTab_Tail()
    {
        if (tailTabIndex < 3)
        {
            tailTabIndex += 1;

            for (int i = 0; i < 4; i++)
            {
                if (i == tailTabIndex)
                {
                    Color color = tailTabCounter[i].GetComponent<Image>().color;
                    color.a = 1.0f;
                    tailTabCounter[i].GetComponent<Image>().color = color;
                }
                else
                {
                    Color color = tailTabCounter[i].GetComponent<Image>().color;
                    color.a = 0.2f;
                    tailTabCounter[i].GetComponent<Image>().color = color;
                }
            }
        }

        UpdateTailButton();
    }

    public void PrevTab_Tail()
    {
        if (tailTabIndex > 0)
        {
            tailTabIndex -= 1;
        }

        for (int i = 0; i < 4; i++)
        {
            if (i == tailTabIndex)
            {
                Color color = tailTabCounter[i].GetComponent<Image>().color;
                color.a = 1.0f;
                tailTabCounter[i].GetComponent<Image>().color = color;
            }
            else
            {
                Color color = tailTabCounter[i].GetComponent<Image>().color;
                color.a = 0.2f;
                tailTabCounter[i].GetComponent<Image>().color = color;
            }
        }

        UpdateTailButton();
    }

    public void UpdateTailButton()
    {
        if (tailTabIndex < 3)
        {
            // 모든 꼬리 버튼 비활성화
            for (int i = 0; i < 9; i++)
            {
                tailButtons[i].GetComponent<Button>().interactable = false;
                tailButtons[i].transform.GetChild(0).GetComponent<Image>().sprite = tailImages[i + 9 * tailTabIndex];

                Color color = tailButtons[i].GetComponent<Image>().color;
                color.a = 150.0f / 255.0f;
                tailButtons[i].GetComponent<Image>().color = color;
                color.a = 0.2f;
                tailButtons[i].transform.GetChild(0).GetComponent<Image>().color = color;
            }

            // 보유한 꼬리 버튼 활성화
            for (int i = 0; i < 30; i++)
            {
                if (DBManager.instance.user.tail[i] == 1)
                {
                    if (i >= 9 * tailTabIndex && i <= 8 + 9 * tailTabIndex)
                    {
                        int index = i - 9 * tailTabIndex;
                        tailButtons[index].GetComponent<Button>().interactable = true;
                        Color color = tailButtons[index].GetComponent<Image>().color;
                        color.a = 1.0f;
                        tailButtons[index].GetComponent<Image>().color = color;
                        tailButtons[index].transform.GetChild(0).GetComponent<Image>().color = color;
                    }
                }
            }
        }
        else
        {
            // 모든 꼬리 버튼 비활성화
            for (int i = 0; i < 9; i++)
            {
                if (i < 3)
                {
                    tailButtons[i].GetComponent<Button>().interactable = false;
                    tailButtons[i].transform.GetChild(0).GetComponent<Image>().sprite = tailImages[i + 9 * tailTabIndex];

                    Color color = tailButtons[i].GetComponent<Image>().color;
                    color.a = 150.0f / 255.0f;
                    tailButtons[i].GetComponent<Image>().color = color;
                    color.a = 0.2f;
                    tailButtons[i].transform.GetChild(0).GetComponent<Image>().color = color;
                }
                else
                {
                    Color color = tailButtons[i].GetComponent<Image>().color;
                    color.a = 0f;
                    tailButtons[i].GetComponent<Button>().interactable = false;
                    tailButtons[i].GetComponent<Image>().color = color;
                    tailButtons[i].transform.GetChild(0).GetComponent<Image>().color = color;
                }
            }

            // 보유한 꼬리 버튼 활성화
            for (int i = 0; i < 30; i++)
            {
                if (DBManager.instance.user.tail[i] == 1)
                {
                    if (i >= 9 * tailTabIndex && i <= 8 + 9 * tailTabIndex)
                    {
                        int index = i - 9 * tailTabIndex;
                        tailButtons[index].GetComponent<Button>().interactable = true;
                        Color color = tailButtons[index].GetComponent<Image>().color;
                        color.a = 1.0f;
                        tailButtons[index].GetComponent<Image>().color = color;
                        tailButtons[index].transform.GetChild(0).GetComponent<Image>().color = color;
                    }
                }
            }
        }
    }

    public void UpdateCharacterButton()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].imageIndex == DBManager.instance.user.currentCharacterIndex)
            {
                currentCharacter.transform.GetChild(0).GetComponent<Image>().sprite = characterBodyImages[characters[i].imageIndex];
                currentCharacter.transform.GetChild(1).GetComponent<Image>().sprite = characterFaceImages[characters[i].imageIndex / 4];
                currentCharacter.transform.GetChild(2).GetComponent<TMP_Text>().text = characters[i].name;

                healthText.text = $"체력 : {characters[i].maxHealth}";
                sightRangeText.text = $"시야 범위 : {characterDatas[i].maxSightRange}";
                break;
            }
        }

        if (characterTabIndex < 2)
        {
            // 모든 캐릭터 버튼 비활성화
            for (int i = 0; i < 9; i++)
            {
                characterButtons[i].GetComponent<Button>().interactable = false;
                characterButtons[i].transform.GetChild(0).GetComponent<Image>().sprite = characterBodyImages[characterDatas[i + 9 * characterTabIndex].imageIndex];
                characterButtons[i].transform.GetChild(1).GetComponent<Image>().sprite = characterFaceImages[characterDatas[i + 9 * characterTabIndex].imageIndex / 4];
                characterButtons[i].transform.GetChild(2).GetComponent<TMP_Text>().text = $"{characterDatas[i + 9 * characterTabIndex].name}";

                Color color = characterButtons[i].GetComponent<Image>().color;
                color.a = 150.0f / 255.0f;
                characterButtons[i].GetComponent<Image>().color = color;
                color.a = 0.2f;
                characterButtons[i].transform.GetChild(0).GetComponent<Image>().color = color;
                characterButtons[i].transform.GetChild(1).GetComponent<Image>().color = color;
                characterButtons[i].transform.GetChild(2).GetComponent<TMP_Text>().color = color;
            }

            // 보유한 캐릭터 버튼 활성화
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i].imageIndex >= 9 * characterTabIndex && characters[i].imageIndex <= 8 + 9 * characterTabIndex)
                {
                    int index = characters[i].imageIndex - 9 * characterTabIndex;
                    characterButtons[index].GetComponent<Button>().interactable = true;
                    Color color = characterButtons[index].GetComponent<Image>().color;
                    color.a = 1.0f;
                    characterButtons[index].GetComponent<Image>().color = color;
                    characterButtons[index].transform.GetChild(0).GetComponent<Image>().color = color;
                    characterButtons[index].transform.GetChild(1).GetComponent<Image>().color = color;
                    characterButtons[index].transform.GetChild(2).GetComponent<TMP_Text>().color = color;
                }
            }
        }
        else
        {
            // 모든 캐릭터 버튼 비활성화
            for (int i = 0; i < 9; i++)
            {
                if (i < 2)
                {
                    characterButtons[i].GetComponent<Button>().interactable = false;
                    characterButtons[i].transform.GetChild(0).GetComponent<Image>().sprite = characterBodyImages[characterDatas[i + 9 * characterTabIndex].imageIndex];
                    characterButtons[i].transform.GetChild(1).GetComponent<Image>().sprite = characterFaceImages[characterDatas[i + 9 * characterTabIndex].imageIndex / 4];
                    characterButtons[i].transform.GetChild(2).GetComponent<TMP_Text>().text = $"{characterDatas[i + 9 * characterTabIndex].name}";

                    Color color = characterButtons[i].GetComponent<Image>().color;
                    color.a = 150.0f / 255.0f;
                    characterButtons[i].GetComponent<Image>().color = color;
                    color.a = 0.2f;
                    characterButtons[i].transform.GetChild(0).GetComponent<Image>().color = color;
                    characterButtons[i].transform.GetChild(1).GetComponent<Image>().color = color;
                    characterButtons[i].transform.GetChild(2).GetComponent<TMP_Text>().color = color;
                }
                else
                {
                    Color color = characterButtons[i].GetComponent<Image>().color;
                    color.a = 0f;
                    characterButtons[i].GetComponent<Button>().interactable = false;
                    characterButtons[i].GetComponent<Image>().color = color;
                    characterButtons[i].transform.GetChild(0).GetComponent<Image>().color = color;
                    characterButtons[i].transform.GetChild(1).GetComponent<Image>().color = color;
                    characterButtons[i].transform.GetChild(2).GetComponent<TMP_Text>().color = color;
                }
            }

            // 보유한 캐릭터 버튼 활성화
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i].imageIndex >= 9 * characterTabIndex && characters[i].imageIndex <= 1 + 9 * characterTabIndex)
                {
                    int index = characters[i].imageIndex - 9 * characterTabIndex;
                    characterButtons[index].GetComponent<Button>().interactable = true;
                    Color color = characterButtons[index].GetComponent<Image>().color;
                    color.a = 1.0f;
                    characterButtons[index].GetComponent<Image>().color = color;
                    characterButtons[index].transform.GetChild(0).GetComponent<Image>().color = color;
                    characterButtons[index].transform.GetChild(1).GetComponent<Image>().color = color;
                    characterButtons[index].transform.GetChild(2).GetComponent<TMP_Text>().color = color;
                }
            }
        }
    }

    public void SelectTail(int b_index)
    {
        currentTailImage.sprite = tailImages[b_index + 9 * tailTabIndex];
        effectImage.sprite = tailImages[b_index + 9 * tailTabIndex];

        DBManager.instance.user.currentTailIndex = b_index + 9 * tailTabIndex;
    }

    public void SelectCharacter(int b_index)
    {
        index = b_index;

        characterInfoPopup.SetActive(true);

        Character chr = characterDatas[index + 9 * characterTabIndex];

        int matchIndex = -1;

        for (int i = 0; i < DBManager.instance.user.character.Count; i++)
        {
            if (DBManager.instance.CharacterIndexMatching(DBManager.instance.user.character[i].index) == index + 9 * characterTabIndex)
            {
                matchIndex = i;
            }
        }

        characterName.text = $"{chr.name}";
        body.sprite = characterBodyImages[index + 9 * characterTabIndex];
        face.sprite = characterFaceImages[(int)((index + 9 * characterTabIndex) / 4)];
        gauge.fillAmount = (float)DBManager.instance.user.character[matchIndex].count / 30.0f;
        gaugeText.text = $"{DBManager.instance.user.character[matchIndex].count} / {30}";
        typeText.text = chr.color;
        healthText2.text = $"{chr.maxHealth + chr.level * chr.healthIncreaseRate}"; // + 세트효과 구현 필요
        sightText2.text = $"{chr.minSightRange}"; // + 패시브, 세트효과 구현 필요

        if (chr.level >= 6)
        {
            starIcon.sprite = star_red;
        }
        else
        {
            starIcon.sprite = star;
        }
    }

    public void UseCharacter()
    {
        characterInfoPopup.SetActive(false);

        currentCharacter.transform.GetChild(0).GetComponent<Image>().sprite = characterButtons[index].transform.GetChild(0).GetComponent<Image>().sprite;
        currentCharacter.transform.GetChild(1).GetComponent<Image>().sprite = characterButtons[index].transform.GetChild(1).GetComponent<Image>().sprite;
        currentCharacter.transform.GetChild(2).GetComponent<TMP_Text>().text = characterButtons[index].transform.GetChild(2).GetComponent<TMP_Text>().text;

        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].imageIndex == index + 9 * characterTabIndex)
            {
                healthText.text = $"체력 : {characters[i].maxHealth}";
                sightRangeText.text = $"시야 범위 : {characterDatas[i].maxSightRange}";
                break;
            }
        }

        DBManager.instance.user.currentCharacterIndex = index + 9 * characterTabIndex;
        OnChangeCurrentCharacter?.Invoke(this, EventArgs.Empty);
    }

    public void OpenOptionPopup()
    {
        OptionPopup.SetActive(true);
    }

    public void OpenFriendsPopup()
    {
        friendsPopup.SetActive(true);
    }

    public void OpenMailPopup()
    {
        mailPopup.SetActive(true);
    }

    public void OpenSelectPopup()
    {
        characterSelectPopup.SetActive(true);

        characters = DBManager.instance.user.character;
        characterDatas = ChartManager.instance.characterDatas;

        UpdateCharacterButton();

        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].imageIndex == DBManager.instance.user.currentCharacterIndex)
            {
                healthText.text = $"체력 : {characters[i].maxHealth}";
                sightRangeText.text = $"시야 범위 : {characters[i].maxSightRange}";
            }
        }
    }

    public void CloseInfoPopup()
    {
        characterInfoPopup.SetActive(false);
    }

    public void OpenChracterSelectTab()
    {
        characterSelectTab.SetActive(true);
        tailTab.SetActive(false);
    }

    public void OpenTailTab()
    {
        characterSelectTab.SetActive(false);
        tailTab.SetActive(true);

        UpdateTailButton();
    }

    public void ExitGame()
    {
        DBManager.instance.SaveUserData();
        Application.Quit();
    }

    public DateTime SetAPPoint()
    {
        DateTime dt_apTime = DateTime.Now;

        for (int i = 0; i < DBManager.instance.user.activePointTime.Length; i++)
        {
            dt_apTime = DateTime.ParseExact(DBManager.instance.user.activePointTime[i], "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);

            if (DateTime.Compare(dt_apTime, DateTime.Now) == 1)
            {
                Debug.Log($"새 AP : {dt_apTime}");
                return dt_apTime;
            }
        }

        return dt_apTime;
    }

    public IEnumerator UpdateTimeLeftText_co()
    {
        TimeSpan timeDiff = TimeSpan.Zero;

        DateTime dt_apTime = SetAPPoint();

        WaitForSeconds wfs = new WaitForSeconds(1.0f);

        activePointTimeText.text = $"충전 완료!";

        while (true)
        {
            if (SceneManager.GetActiveScene().name.Equals("Chatting") && DBManager.instance.user.activePoint < 5)
            {
                if (DateTime.Compare(dt_apTime, DateTime.Now) == 1)
                {
                    timeDiff = dt_apTime - DateTime.Now;
                }

                if (timeDiff.Seconds < 10)
                {
                    activePointTimeText.text = $"{timeDiff.Minutes} : 0{timeDiff.Seconds}";
                }
                else
                {
                    activePointTimeText.text = $"{timeDiff.Minutes} : {timeDiff.Seconds}";
                }

                if (timeDiff.Minutes == 0 && timeDiff.Seconds == 0)
                {
                    DBManager.instance.user.activePoint += 1;
                    activePointTimeText.text = $"충전 완료!";
                    activePointCountText.text = $"{DBManager.instance.user.activePoint}";
                    dt_apTime = SetAPPoint();
                }
            }

            yield return wfs;
        }
    }
}
