using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;

public class FriendManager : MonoBehaviour
{
    [Header("Tab Button")]
    [SerializeField] private TMP_Text tabText;
    [SerializeField] private Button friendListTabButton;
    [SerializeField] private Button friendRequestTabButton;
    [SerializeField] private Button friendAddTabButton;

    [Header("Tab")]
    [SerializeField] private GameObject friendListTab;
    [SerializeField] private GameObject friendRequestTab;
    [SerializeField] private GameObject friendAddTab;

    [Header("Friend List")]
    [SerializeField] private Transform content_friendList;
    [SerializeField] private GameObject friendTabPrefab;

    [Header("Request List")]
    [SerializeField] private Transform content_requestList;
    [SerializeField] private GameObject requestTabPrefab;

    [Header("Add Friend")]
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private GameObject searchTab;
    [SerializeField] private GameObject errorText;
    [SerializeField] private Button sendRequestButton;
    private string searchUserName;

    [Header("Sprite")]
    [SerializeField] Sprite[] characterBodyImages; // 캐릭터 Body 이미지 배열
    [SerializeField] Sprite[] characterFaceImages; // 캐릭터 Face 이미지 배열

    private List<Friend> friends = new List<Friend>();
    private List<Friend> requests = new List<Friend>();

    private void OnEnable()
    {
        GetFriendsData();
        ShowFriendsList();
        ResetUI();
    }

    public void ResetUI()
    {
        // UI 초기화
        searchTab.SetActive(false);
        sendRequestButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "전송";
        sendRequestButton.interactable = true;
        errorText.SetActive(false);
        searchUserName = string.Empty;
    }

    public void GetFriendsData() // 친구 목록 조회
    {
        friends.Clear();

        // 전체 친구 리스트 조회
        var bro = Backend.Friend.GetFriendList();

        // limit, offset 사용하여 친구 리스트 조회
        //Backend.Friend.GetFriendList(5); // 5명 친구 조회(1-5)
        //Backend.Friend.GetFriendList(10, 5); // 처음 5명 이후의 10명 친구 조회(6-15)

        LitJson.JsonData json = bro.FlattenRows();

        //User user = DBManager.instance.user; // -> 나중에 user 클래스에 할당 필요

        for (int i = 0; i < json.Count; i++)
        {
            Friend currentFriend = new Friend();

            currentFriend.name = json[i]["nickname"].ToString();
            currentFriend.inDate = json[i]["inDate"].ToString();
            currentFriend.lastLogin = json[i]["lastLogin"].ToString();
            currentFriend.createdAt = json[i]["createdAt"].ToString();

            //user.friend.Add(currentFriend);
            friends.Add(currentFriend);
        }
    }

    public void ShowFriendsList()
    {
        for (int i = 0; i < content_friendList.childCount; i++)
        {
            Destroy(content_friendList.GetChild(0).gameObject);
        }

        for (int i = 0; i < friends.Count; i++)
        {
            GameObject friendTab = Instantiate(friendTabPrefab);
            friendTab.transform.SetParent(content_friendList);

            var bro = Backend.PlayerData.GetOtherData("User", friends[i].inDate);

            int index = int.Parse(bro.GetReturnValuetoJSON()["rows"][0]["CurrentCharacterIndex"][0].ToString());

            friendTab.transform.GetChild(1).GetComponent<Image>().sprite = characterBodyImages[index];
            friendTab.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = characterFaceImages[index / 4];
            friendTab.transform.GetChild(2).GetComponent<TMP_Text>().text = $"{friends[i].name}";

            int temp = i;
            friendTab.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { OpenWhisperTab(temp, friends[temp].inDate); });
            friendTab.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(delegate { SendStar(temp, friends[temp].inDate); });
            friendTab.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(delegate { DeleteFriend(temp, friends[temp].inDate); });
        }
    }

    public void GetRequestData() // 받은 친구 요청 목록 조회
    {
        requests = GetReceivedRequestList();
    }

    public void ShowRequestsList()
    {
        requests = GetReceivedRequestList();

        for (int i = 0; i < requests.Count; i++)
        {
            GameObject requestTab = Instantiate(requestTabPrefab);
            requestTab.transform.SetParent(content_requestList);

            var bro = Backend.PlayerData.GetOtherData("User", requests[i].inDate);

            int index = int.Parse(bro.GetReturnValuetoJSON()["rows"][0]["CurrentCharacterIndex"][0].ToString());

            requestTab.transform.GetChild(1).GetComponent<Image>().sprite = characterBodyImages[index]; // 해당 친구가 사용중인 캐릭터로 매칭 필요
            requestTab.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = characterFaceImages[index / 4]; // 해당 친구가 사용중인 캐릭터로 매칭 필요
            requestTab.transform.GetChild(2).GetComponent<TMP_Text>().text = $"{requests[i].name}";

            int temp = i;
            requestTab.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { RejectRequest(temp, requests[temp].inDate); });
            requestTab.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(delegate { AcceptRequest(temp, requests[temp].inDate); });
        }
    }

    public void SendFriendsRequest(string nickname) // 친구 요청 전송
    {
        // 닉네임으로 gamer inDate 가져오기
        var n_bro = Backend.Social.GetUserInfoByNickName(nickname);
        string n_inDate = n_bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();

        // gamer inDate 로 친구 요청 보내기
        var bro = Backend.Friend.RequestFriend(n_inDate);

        if (!bro.IsSuccess())
        {
            string message = string.Empty;

            switch (int.Parse(bro.GetStatusCode()))
            {
                case 403:
                    message = "뒤끝 콘솔 소셜관리 메뉴의 친구 최대보유수 설정값이 0입니다";
                    break;
                case 409:
                    message = "이미 요청을 보낸 유저입니다";
                    break;
                case 412:
                    message = bro.GetMessage().Contains("Send") ? "보낸 유저의 요청이 가득 찼습니다" : "받는 유저의 요청이 가득 찼습니다";
                    break;
                default:
                    message = bro.GetMessage();
                    break;
            }
            errorText.GetComponent<TMP_Text>().text = message;
        }
        else
        {
            errorText.GetComponent<TMP_Text>().text = $"친구 요청 전송 완료";
        }

        errorText.SetActive(true);
    }

    public List<Friend> GetSentFriendRequestList() // 보낸 친구요청 목록 조회 (수락, 거절 시 리스트에서 제거됨)
    {
        List<Friend> list = new List<Friend>();

        // 친구 요청을 보낸 리스트 전체 조회
        var bro = Backend.Friend.GetSentRequestList();
        // limit, offset 사용하여 친구 요청을 보낸 리스트 조회
        //Backend.Friend.GetSentRequestList(5); // 친구 요청을 보낸 5명 리스트 조회(1-5)
        //Backend.Friend.GetSentRequestList(5, 5); // 친구 요청을 보낸 처음 5명 이후의 5명 리스트 조회(6-10)

        LitJson.JsonData json = bro.FlattenRows();

        for (int i = 0; i < json.Count; i++)
        {
            Friend currentFriend = new Friend();

            currentFriend.name = json[i]["nickname"].ToString();
            currentFriend.inDate = json[i]["inDate"].ToString();
            currentFriend.lastLogin = json[i]["lastLogin"].ToString();
            currentFriend.createdAt = json[i]["createdAt"].ToString();

            list.Add(currentFriend);
        }

        return list;
    }

    public List<Friend> GetReceivedRequestList() // 받은 친구요청 목록 조회 (수락, 거절 시 리스트에서 제거됨)
    {
        List<Friend> list = new List<Friend>();

        // 친구 요청을 받은 리스트 전체 조회
        var bro = Backend.Friend.GetReceivedRequestList();
        // limit, offset 사용하여 친구 요청을 받은 리스트 조회
        //Backend.Friend.GetReceivedRequestList(5); // 친구 요청을 받은 5명 리스트 조회(1-5)
        //Backend.Friend.GetReceivedRequestList(5, 5); // 친구 요청을 받은 처음 5명 이후의 5명 리스트 조회(6-10)

        LitJson.JsonData json = bro.FlattenRows();

        for (int i = 0; i < json.Count; i++)
        {
            Friend currentFriend = new Friend();

            currentFriend.name = json[i]["nickname"].ToString();
            currentFriend.inDate = json[i]["inDate"].ToString();
            //currentFriend.lastLogin = json[i]["lastLogin"].ToString();
            currentFriend.createdAt = json[i]["createdAt"].ToString();

            list.Add(currentFriend);
        }

        return list;
    }

    public void AcceptFriendRequest(string n_inDate) // 친구 요청 수락
    {
        // 닉네임으로 gamer inDate 가져오기
        //var n_bro = Backend.Social.GetUserInfoByNickName(nickname);
        //string n_inDate = n_bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();

        // gamer inDate 로 친구 요청 수락
        var bro = Backend.Friend.AcceptFriend(n_inDate);

        if (!bro.IsSuccess())
        {
            string message = string.Empty;

            switch (int.Parse(bro.GetStatusCode()))
            {
                case 412:
                    message = bro.GetMessage().Contains("Requested") ? "요청한 유저의 친구 목록이 가득 찼습니다." : "친구 목록이 가득 찼습니다.";
                    break;
                default:
                    message = bro.GetMessage();
                    break;
            }
            Debug.Log(message);
        }
    }

    public void RejectFriendRequest(string n_inDate) // 친구 요청 거절
    {
        // 닉네임으로 gamer inDate 가져오기
        //var n_bro = Backend.Social.GetUserInfoByNickName(nickname);
        //string n_inDate = n_bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();

        // gamer inDate 로 친구 요청 거절
        var bro = Backend.Friend.RejectFriend(n_inDate);
    }

    public void DeleteFriend(string n_inDate) // 친구 삭제
    {
        // 닉네임으로 gamer inDate 가져오기
        //var n_bro = Backend.Social.GetUserInfoByNickName(nickname);
        //string n_inDate = n_bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();

        // gamer inDate 로 친구 삭제
        var bro = Backend.Friend.BreakFriend(n_inDate);

        if (!bro.IsSuccess())
        {
            string message = string.Empty;

            switch (int.Parse(bro.GetStatusCode()))
            {
                case 404:
                    message = "gamerIndate가 올바르지 않거나 해당 유저와 친구가 아닙니다.";
                    break;
                default:
                    message = bro.GetMessage();
                    break;
            }
            Debug.Log(message);
        }
    }

    public void OpenWhisperTab(int index, string inDate)
    {
        // 해당 유저와 귓속말 탭 구현 필요
    }

    public void SendStar(int index, string inDate)
    {
        // 해당 유저에게 별 전송하기 구현 필요

        content_friendList.GetChild(index).GetChild(4).GetChild(1).GetComponent<TMP_Text>().text = $"전송 아직 미구현";
    }

    public void DeleteFriend(int index, string inDate) // 해당 친구 삭제
    {
        friends.RemoveAt(index);
        Destroy(content_friendList.GetChild(index).gameObject);
        DeleteFriend(inDate);
    }

    public void RejectRequest(int index, string inDate) // 친구 요청 거절
    {
        requests.RemoveAt(index);
        Destroy(content_requestList.GetChild(index).gameObject);
        RejectFriendRequest(inDate);
    }

    public void AcceptRequest(int index, string inDate) // 친구 요청 수락
    {
        requests.RemoveAt(index);
        Destroy(content_requestList.GetChild(index).gameObject);
        AcceptFriendRequest(inDate);
    }

    public void SearchUser()
    {
        var n_bro = Backend.Social.GetUserInfoByNickName(searchInputField.text);

        // 해당 닉네임을 사용중인 유저가 존재 시에만 표시
        if (!n_bro.IsSuccess())
        {
            searchTab.SetActive(false);
            errorText.SetActive(true);
            errorText.GetComponent<TMP_Text>().text = $"검색에 실패했습니다";
            return;
        }
        else
        {
            searchTab.SetActive(true);
            errorText.SetActive(false);
        }

        string n_inDate = n_bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();
        var bro = Backend.PlayerData.GetOtherData("User", n_inDate);

        //int index = DBManager.instance.CharacterIndexMatching(int.Parse(bro.GetReturnValuetoJSON()["rows"][0]["CurrentCharacterIndex"][0].ToString()));
        int index = int.Parse(bro.GetReturnValuetoJSON()["rows"][0]["CurrentCharacterIndex"][0].ToString());

        // Debug.Log($"Original Index : {int.Parse(bro.GetReturnValuetoJSON()["rows"][0]["CurrentCharacterIndex"][0].ToString())} / Return Index : {index}");

        // 닉네임 & InDate로 유저 조회 후 표시 구현 필요
        searchTab.transform.GetChild(1).GetComponent<Image>().sprite = characterBodyImages[index]; // 해당 친구가 사용중인 캐릭터로 매칭 필요
        searchTab.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = characterFaceImages[index / 4]; // 해당 친구가 사용중인 캐릭터로 매칭 필요
        searchTab.transform.GetChild(2).GetComponent<TMP_Text>().text = $"{bro.GetReturnValuetoJSON()["rows"][0]["UserName"][0]}";

        searchUserName = searchInputField.text;
    }

    public void SendAddRequest()
    {
        if (Backend.UserNickName == searchUserName)
        {
            errorText.GetComponent<TMP_Text>().text = $"자신에게는 보낼 수 없습니다";
            errorText.SetActive(true);
            return;
        }

        sendRequestButton.interactable = false;
        sendRequestButton.transform.GetChild(0).GetComponent<TMP_Text>().text = $"전송 완료";

        SendFriendsRequest(searchUserName); // 또는 searchTab.transform.GetChild(2).GetComponent<TMP_Text>().text;
    }

    public void OpenFriendListTab()
    {
        ResetUI();

        tabText.text = $"친구 목록";
        friendListTabButton.interactable = false;
        friendRequestTabButton.interactable = true;
        friendAddTabButton.interactable = true;

        friendListTab.SetActive(true);
        friendRequestTab.SetActive(false);
        friendAddTab.SetActive(false);

        GetFriendsData();
        ShowFriendsList();
    }

    public void OpenFriendRequestTab()
    {
        ResetUI();

        tabText.text = $"받은 요청";
        friendListTabButton.interactable = true;
        friendRequestTabButton.interactable = false;
        friendAddTabButton.interactable = true;

        friendListTab.SetActive(false);
        friendRequestTab.SetActive(true);
        friendAddTab.SetActive(false);

        GetRequestData();
        ShowRequestsList();
    }

    public void OpenFriendAddTab()
    {
        ResetUI();

        tabText.text = $"친구 추가";
        friendListTabButton.interactable = true;
        friendRequestTabButton.interactable = true;
        friendAddTabButton.interactable = false;

        friendListTab.SetActive(false);
        friendRequestTab.SetActive(false);
        friendAddTab.SetActive(true);
    }

    public void CloseFriendPopup()
    {
        OpenFriendListTab();
        ResetUI();
        gameObject.SetActive(false);
    }
}
