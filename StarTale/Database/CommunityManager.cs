using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

public class CommunityManager : MonoBehaviour
{
    public static CommunityManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region 친구 시스템

    public void GetFriendsList() // 보유한 친구 목록 조회 후 user에 할당
    {
        // 전체 친구 리스트 조회
        var bro = Backend.Friend.GetFriendList();

        // limit, offset 사용하여 친구 리스트 조회
        //Backend.Friend.GetFriendList(5); // 5명 친구 조회(1-5)
        //Backend.Friend.GetFriendList(10, 5); // 처음 5명 이후의 10명 친구 조회(6-15)

        LitJson.JsonData json = bro.FlattenRows();

        User user = DBManager.instance.user;

        for (int i = 0; i < json.Count; i++)
        {
            Friend currentFriend = new Friend();

            currentFriend.name = json[i]["nickname"].ToString();
            currentFriend.inDate = json[i]["inDate"].ToString();
            //currentFriend.lastLogin = json[i]["lastLogin"].ToString();
            currentFriend.createdAt = json[i]["createdAt"].ToString();

            user.friend.Add(currentFriend);
        }
    }

    public void SendFriendsRequest(string nickname) // 닉네임으로 친구 요청 보내기
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
                    message = "뒤끝 콘솔 소셜관리 메뉴의 친구 최대보유수 설정값이 0입니다.";
                    break;
                case 409:
                    message = "이미 요청한 유저입니다.";
                    break;
                case 412:
                    message = bro.GetMessage().Contains("Send") ? "보낸 유저의 요청이 가득 찼습니다." : "받는 유저의 요청이 가득 찼습니다.";
                    break;
                default:
                    message = bro.GetMessage();
                    break;
            }
            Debug.Log(message);
        }
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
            currentFriend.lastLogin = json[i]["lastLogin"].ToString();
            currentFriend.createdAt = json[i]["createdAt"].ToString();

            list.Add(currentFriend);
        }

        return list;
    }

    public void AcceptFriendRequest(string nickname)
    {
        // 닉네임으로 gamer inDate 가져오기
        var n_bro = Backend.Social.GetUserInfoByNickName(nickname);
        string n_inDate = n_bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();

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
    } // 해당 닉네임 친구요청 수락

    public void RejectFriendRequest(string nickname)
    {
        // 닉네임으로 gamer inDate 가져오기
        var n_bro = Backend.Social.GetUserInfoByNickName(nickname);
        string n_inDate = n_bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();

        // gamer inDate 로 친구 요청 수락
        var bro = Backend.Friend.RejectFriend(n_inDate);
    } // 해당 닉네임 친구요청 거절

    public void DeleteFriend(string nickname)
    {
        // 닉네임으로 gamer inDate 가져오기
        var n_bro = Backend.Social.GetUserInfoByNickName(nickname);
        string n_inDate = n_bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();

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
    } // 해당 닉네임 친구 삭제

    #endregion

    #region 우편 시스템

    // 프로토타입 완성 이후에 재개할게요~~

    public void GetPostList()
    {
        // PostType : Admin, Rank, Coupon, User

        BackendReturnObject bro = Backend.UPost.GetPostList(PostType.User, 30);
        LitJson.JsonData json = bro.GetReturnValuetoJSON()["postList"];

        for (int i = 0; i < json.Count; i++)
        {
            // json[i]["content"].ToString();
            // json[i]["expirationDate"].ToString();
            // json[i]["receiverInDate"].ToString();
            // json[i]["item"].ToString(); -> 딕셔너리
            // json[i]["itemLocation"].ToString(); -> 딕셔너리
            // json[i]["receiverNickname"].ToString();
            // json[i]["receivedDate"].ToString();
            // json[i]["sender"].ToString();
            // json[i]["inDate"].ToString();
            // json[i]["senderNickname"].ToString();
            // json[i]["senderInDate"].ToString();
            // json[i]["sentDate"].ToString();
            // json[i]["title"].ToString();
        }
    }

    #endregion
}
