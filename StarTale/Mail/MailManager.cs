using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using System;

public class MailManager : MonoBehaviour
{
    [SerializeField] private GameObject mailPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private TMP_Text noPresentText;
    [SerializeField] private Sprite[] goodsImages;
    [SerializeField] private TMP_Text mailCountText;

    private List<Mail> mails = new List<Mail>();

    Coroutine timerCoroutine = null;

    private void OnEnable()
    {
        GetMailData();
        ShowMailData();
        timerCoroutine = StartCoroutine(UpdateExpDate_co());
    }

    private void OnDestroy()
    {
        StopCoroutine(timerCoroutine);
    }

    public void GetMailData()
    {
        BackendReturnObject bro = Backend.UPost.GetPostList(PostType.Admin, 100);
        LitJson.JsonData json = bro.GetReturnValuetoJSON()["postList"];

        for (int i = 0; i < json.Count; i++)
        {
            Mail currentMail = new Mail();

            currentMail.title = json[i]["title"].ToString();
            currentMail.content = json[i]["content"].ToString();
            currentMail.inDate = json[i]["inDate"].ToString();
            currentMail.sentDate = json[i]["sentDate"].ToString();
            currentMail.expirationDate = json[i]["expirationDate"].ToString();

            if (json[i]["items"].Count != 0)
            {
                currentMail.goods.index = int.Parse(json[i]["items"][0]["item"]["Index"].ToString());
                currentMail.goods.imageIndex = int.Parse(json[i]["items"][0]["item"]["ImageIndex"].ToString());
                currentMail.goods.name = json[i]["items"][0]["item"]["Name"].ToString();
                currentMail.goods.quantity = int.Parse(json[i]["items"][0]["itemCount"].ToString());
            }

            mails.Add(currentMail);
        }
    }

    public void ShowMailData()
    {
        for (int i = 0; i < mails.Count; i++)
        {
            GameObject mail = Instantiate(mailPrefab);
            mail.transform.SetParent(content);

            mail.transform.GetChild(1).GetComponent<Image>().sprite = goodsImages[mails[i].goods.imageIndex];
            mail.transform.GetChild(2).GetComponent<TMP_Text>().text = $"{mails[i].goods.quantity}";
            mail.transform.GetChild(3).GetComponent<TMP_Text>().text = mails[i].title;
            mail.transform.GetChild(4).GetComponent<TMP_Text>().text = $"남은 시간 : nn 시간"; // 남은시간 계산 필요

            int temp = i;
            mail.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(delegate { ReceiveMail(temp); });
        }

        if (mails.Count == 0)
        {
            noPresentText.gameObject.SetActive(true);
        }
        else
        {
            noPresentText.gameObject.SetActive(false);
        }

        mailCountText.text = $"{mails.Count} / 100";
    }

    public IEnumerator UpdateExpDate_co()
    {
        TimeSpan dateDiff;

        WaitForSeconds wfs = new WaitForSeconds(30.0f);

        while (true)
        {
            for (int i = 0; i < content.childCount; i++)
            {
                DateTime ExpDate = Convert.ToDateTime($"{mails[i].expirationDate.Substring(0, 10)} {mails[i].expirationDate.Substring(11, 8)}");
                dateDiff = ExpDate - DateTime.Now;

                content.GetChild(i).transform.GetChild(4).GetComponent<TMP_Text>().text = $"남은 시간 : {dateDiff.Days}일 {dateDiff.Hours}시간 {dateDiff.Minutes}분";
            }

            yield return wfs;
        }
    }

    public void ReceiveMail(int index)
    {
        // inDate 사용
        var bro = Backend.UPost.ReceivePostItem(PostType.Admin, mails[index].inDate);

        if (bro.IsSuccess())
        {
            DBManager.instance.user.goods[mails[index].goods.name] += mails[index].goods.quantity;
            Debug.Log($"{mails[index].goods.name}를 {mails[index].goods.quantity}만큼 획득");

            Destroy(content.GetChild(index).gameObject);
        }
    }

    public void CloseMailPopup()
    {
        gameObject.SetActive(false);
    }
}
