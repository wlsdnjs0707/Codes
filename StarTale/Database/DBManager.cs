using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;
using System;
using System.Globalization;

public struct Friend
{
    public string id; // 아이디
    public string name; // 이름
    public string inDate; // 유저의 inDate
    public string createdAt; // 친구가 된 시각
    public string lastLogin; // 마지막 접속 시각
}

public struct Character
{
    public int index; // 인덱스
    public int imageIndex; // 이미지 인덱스
    public int lookImageIndex; // 표정 이미지 인덱스 배열의 시작 인덱스
    public string name; // 이름
    public string color; // 색상
    public int level; // 레벨 (강화단계)
    public int count; // 보유 개수
    public float healthIncreaseRate; // 레벨 당 최대 체력 증가량
    public float maxHealth; // 최대 체력
    public float maxSpeed; // 최대 속도
    public float minSpeed; // 최소 속도
    public float maxSightRange; // 최대 시야 범위
    public float minSightRange; // 최소 시야 범위
    public int activeSkill; // 보유한 액티브 스킬 인덱스
    public int passiveSkill; // 보유한 액티브 스킬 인덱스
}

public struct HousingObject // 하우징 오브젝트들의 고유한 정보
{
    public int index; // 인덱스(식별번호)
    public string name_e; // 이름 (영문)
    public string name_k; // 이름 (한글)
    public string type; // 종류 (배경, 전경, 상호작용..)
    public int layer; // 레이어 순서
    public string setType; // 세트효과 타입
    public int effect; // 효과 종류 (-1: none, 0: gold, 1: maxHP)
    public int maxLevel; // 최대 강화 수치
    public int level; // 강화 단계
    public float increaseRate; // 강화당 능력치 상승량
    public int imageIndex; // 오브젝트 이미지 인덱스
    public int interactType; // 상호작용 타입 (0: Touch, 1: Drag & Drop)
    public int price; // 판매가격
    public string text_e; // 강화 시 출력 텍스트 (영문)
    public string text_k; // 강화 시 출력 텍스트 (한글)
}

public struct MyHousingObject // 내가 설치한 (배치된) 하우징 오브젝트 정보
{
    public int index; // 인덱스
    public float x; // x 좌표
    public float y; // y 좌표
}

public struct Quest // 퀘스트 정보
{
    public int index; // 인덱스
    public string name; // 퀘스트 정보
    public int isDay; // 1 : 일일퀘스트, 0 : 주간퀘스트
}

public struct Mail
{
    public string content; // 우편 내용
    public string expirationDate; // 만료 날짜
    //public string receiverIndate; // 받은 유저의 inDate
    public Goods goods; // 보낸 재화 정보
    //public Dictionary<string, string> itemLocation // 해당 아이템이 위치해있던 테이블 정보
    //public string receiverNickname; // 받을 유저 닉네임
    //public string receivedDate; // 수령한 날짜 (수령한 경우에만 보임)
    //public string sender; // 보낸 유저의 uuid
    public string inDate; // 우편의 inDate
    //public string senderNickname; // 보낸 유저의 닉네임
    //public string senderIndate; // 보낸 유저의 inDate
    public string sentDate; // 보낸 날짜
    public string title; // 우편 제목
}

public struct Goods
{
    public int index;
    public int imageIndex;
    public string name;
    public int quantity;
}

public struct StageInfo
{
    public int index;
    public string name_e;
    public string name_k;
    public int condition_1; // 1별 획득 조건
    public int condition_2; // 2별 획득 조건
    public int condition_3; // 3별 획득 조건
    public int reward_1; // 골드 양
    public int reward_2; // 루비 양
    public int reward_3; // 토큰 인덱스 (하우징 오브젝트 인덱스)
    public int reward_4; // 특수 보상 인덱스 (하우징 오브젝트 인덱스)
    public int reward_repeat; // 반복 획득 골드 양
}

public class User
{
    public bool isLogin { get; set; } // 현재 로그인 성공 했는가?
    public string UserID { get; set; } // 유저 아이디
    public string Password { get; set; } // 유저 비밀번호
    public string UserName { get; set; } // 유저 이름
    public List<Character> character { get; set; } // 보유한 캐릭터 리스트
    public int currentCharacterIndex { get; set; } // 현재 사용중인 캐릭터 인덱스 (0부터)
    public int[] tail { get; set; } // 보유한 꼬리 배열 (0:미보유, 1:보유)
    public int currentTailIndex { get; set; } // 현재 사용중인 꼬리 인덱스
    public Dictionary<string, int> goods { get; set; } // 보유한 재화의 종류와 수량
    public Dictionary<string, int> housingObject { get; set; } // 보유한 하우징 오브젝트 (Key : 이름, Value : 보유수량)
    public List<MyHousingObject> myHousingObject { get; set; } // 설치한 하우징 오브젝트 리스트
    public int[] tokens { get; set; } // 보유한 토큰 개수 배열
    public List<Friend> friend { get; set; } // 친구 리스트
    public List<int> guestBook { get; set; } // 방명록 리스트
    public List<Mail> mail { get; set; } // 우편 리스트
    public int[,] clearInfo { get; set; } // 최초 보상 획득 정보
    public int[] stageTotalInfo { get; set; } // 스테이지 통합 별 개수에 따른 보상 획득 여부
    public int[] tokenInfo { get; set; } // 해당 스테이지에서 토큰을 먹었는지 여부
    public int[] dayQuestInfo { get; set; } // 일일 퀘스트 수행 여부 (0: 진행중, 1: 수행 완료)
    public int questRewardCount { get; set; } // 퀘스트 완료 횟수
    public int[] questRewardInfo { get; set; } // 퀘스트 완료 보상 획득 여부
    public string lastCheckTime { get; set; } // 최종 접속 시간 (접속중이라면 최종 상점 체크 시간)
    public int activePoint { get; set; } // 스테이지 진입 시 필요한 재화 (별)
    public string[] activePointTime { get; set; } // 다음 스타 획득 가능 시간
    public int[,] dailyShopInfo { get; set; } // 일일 상점 3개 품목 정보 ([인덱스, 살수있는 개수])
    public int[,] tokenShopInfo { get; set; } // 토큰 상점 6개 품목 정보 ([인덱스, 살수있는 개수])
    public string timeLeftText { get; set; } // 갱신까지 남은 시간 문자열

    public User() // 생성자에서 초기화
    {
        isLogin = false;
        UserID = "";
        Password = "";
        UserName = "";
        character = new List<Character>();
        tail = new int[30];
        goods = new Dictionary<string, int> { { "friendshipPoint", 0 }, { "ruby", 0 }, { "gold", 0 } };
        housingObject = new Dictionary<string, int>();
        myHousingObject = new List<MyHousingObject>();
        tokens = new int[10];
        friend = new List<Friend>();
        guestBook = new List<int>();
        mail = new List<Mail>();
        clearInfo = new int[10, 5];
        stageTotalInfo = new int[3];
        tokenInfo = new int[10];
        dayQuestInfo = new int[13];
        questRewardCount = 0;
        questRewardInfo = new int[5];
        lastCheckTime = "";
        activePoint = 0;
        activePointTime = new string[5];
        dailyShopInfo = new int[3, 2];
        tokenShopInfo = new int[6, 2];
        timeLeftText = "";
    }
}

public class DBManager : MonoBehaviour
{
    public static DBManager instance;

    public User user = new User();

    private Coroutine timerCoroutine = null;

    public event EventHandler TimerEvent;
    public event EventHandler OnResetDay;

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

    private void OnEnable()
    {
        timerCoroutine = StartCoroutine(UpdateTimeLeftText_co());
    }

    private void OnDisable()
    {
        StopCoroutine(timerCoroutine);
    }

    private void OnApplicationQuit()
    {
        // 게임 종료 시 user class값과 DB값 동기화
        SaveUserData();
    }

    public void DB_Load(string idText, string pwText) // 로그인 시 DB 초기설정
    {
        Where where = new Where();
        where.Equal("owner_inDate", Backend.UserInDate); // 로그인 한 유저의 owner_inDate로 User DB 조회

        var bro = Backend.GameData.GetMyData("User", where);

        user.UserID = idText;
        user.Password = pwText;
        user.UserName = Backend.UserNickName;

        // 저장된 데이터를 불러와 user 클래스에 할당
        JsonData json = bro.FlattenRows(); // 캐싱

        // [보유한 재화]
        var goods_keys = json[0]["Goods"].Keys; // JsonData를 딕셔너리 키로 변환하는 과정

        string[] goodsArray = new string[goods_keys.Count];
        goods_keys.CopyTo(goodsArray, 0);

        for (var i = 0; i < goods_keys.Count; i++)
        {
            var key = goodsArray[i];
            user.goods[key] = int.Parse(json[0]["Goods"][key].ToString());
        }

        // [보유한 토큰]
        for (int i = 0; i < 10; i++) // (총 토큰 개수 = 10)
        {
            user.tokens[i] = int.Parse(bro.FlattenRows()[0]["Tokens"][i].ToString());
        }

        // [보상 획득 정보 (2차원 배열)]
        for (int i = 0; i < 10; i++) // (총 스테이지 개수 = 10)
        {
            for (int j = 0; j < 5; j++) // (리워드 개수 = 5)
            {
                user.clearInfo[i, j] = int.Parse(bro.FlattenRows()[0]["ClearInfo"][i][j].ToString());
            }
        }

        // [하우징 오브젝트 보유 정보]
        var housing_keys = json[0]["HousingObject"].Keys; // JsonData를 딕셔너리 키로 변환하는 과정

        string[] housingArray = new string[housing_keys.Count];
        housing_keys.CopyTo(housingArray, 0);

        for (var i = 0; i < housing_keys.Count; i++)
        {
            var key = housingArray[i];
            user.housingObject[key] = int.Parse(json[0]["HousingObject"][key].ToString());
        }

        // [총 별 개수에 따른 보상 획득 정보]
        for (int i = 0; i < 3; i++)
        {
            user.stageTotalInfo[i] = int.Parse(bro.FlattenRows()[0]["StageTotalInfo"][i].ToString());
        }

        // [토큰 획득 정보]
        for (int i = 0; i < 10; i++) // (총 스테이지 개수 = 10)
        {
            user.tokenInfo[i] = int.Parse(bro.FlattenRows()[0]["TokenInfo"][i].ToString());
        }

        // [캐릭터] (Character 테이블에서 불러오기)
        var c_bro = Backend.GameData.GetMyData("Character", where);

        if (c_bro.GetReturnValuetoJSON()["rows"].Count > 0)
        {
            for (int i = 0; i < c_bro.FlattenRows().Count; i++)
            {
                Character currentCharacter = new Character();

                JsonData c_json = c_bro.GetReturnValuetoJSON()["rows"];

                currentCharacter.index = int.Parse(c_json[i]["Index"][0].ToString());
                currentCharacter.imageIndex = int.Parse(c_json[i]["ImageIndex"][0].ToString());
                currentCharacter.lookImageIndex = int.Parse(c_json[i]["LookImageIndex"][0].ToString());
                currentCharacter.name = c_json[i]["Name"][0].ToString();
                currentCharacter.color = c_json[i]["Color"][0].ToString();
                currentCharacter.level = int.Parse(c_json[i]["Level"][0].ToString());
                currentCharacter.count = int.Parse(c_json[i]["Count"][0].ToString());
                currentCharacter.healthIncreaseRate = float.Parse(c_json[i]["HealthIncreaseRate"][0].ToString());
                currentCharacter.maxHealth = float.Parse(c_json[i]["MaxHealth"][0].ToString());
                currentCharacter.maxSpeed = float.Parse(c_json[i]["MaxSpeed"][0].ToString());
                currentCharacter.minSpeed = float.Parse(c_json[i]["MinSpeed"][0].ToString());
                currentCharacter.maxSightRange = float.Parse(c_json[i]["MaxSightRange"][0].ToString());
                currentCharacter.minSightRange = float.Parse(c_json[i]["MinSightRange"][0].ToString());
                currentCharacter.activeSkill = int.Parse(c_json[i]["ActiveSkill"][0].ToString());
                currentCharacter.passiveSkill = int.Parse(c_json[i]["PassiveSkill"][0].ToString());

                user.character.Add(currentCharacter);
            }
        }

        // [배치한 하우징] (Housing 테이블에서 불러오기)
        var h_bro = Backend.GameData.GetMyData("Housing", where);

        if (h_bro.GetReturnValuetoJSON()["rows"].Count > 0)
        {
            for (int i = 0; i < h_bro.FlattenRows().Count; i++)
            {
                MyHousingObject currentHousing = new MyHousingObject();

                JsonData h_json = h_bro.GetReturnValuetoJSON()["rows"];

                currentHousing.index = int.Parse(h_json[i]["Index"][0].ToString());
                currentHousing.x = float.Parse(h_json[i]["X"][0].ToString());
                currentHousing.y = float.Parse(h_json[i]["Y"][0].ToString());

                user.myHousingObject.Add(currentHousing);
            }
        }

        // [꼬리]
        for (int i = 0; i < 30; i++) // (총 꼬리 개수 = 30)
        {
            user.tail[i] = int.Parse(bro.FlattenRows()[0]["Tail"][i].ToString());
        }

        // [사용중인 캐릭터 인덱스]
        user.currentCharacterIndex = int.Parse(bro.GetReturnValuetoJSON()["rows"][0]["CurrentCharacterIndex"][0].ToString());

        // [사용중인 꼬리 인덱스]
        user.currentTailIndex = int.Parse(bro.GetReturnValuetoJSON()["rows"][0]["CurrentTailIndex"][0].ToString());

        // [일일 퀘스트 진행 정보]
        for (int i = 0; i < 13; i++) // (총 스테이지 개수 = 13)
        {
            user.dayQuestInfo[i] = int.Parse(bro.FlattenRows()[0]["DayQuestInfo"][i].ToString());
        }

        // [퀘스트 완료 횟수]
        user.questRewardCount = int.Parse(bro.GetReturnValuetoJSON()["rows"][0]["QuestRewardCount"][0].ToString());

        // [퀘스트 완료 보상 획득 정보]
        for (int i = 0; i < 5; i++)
        {
            user.questRewardInfo[i] = int.Parse(bro.FlattenRows()[0]["QuestRewardInfo"][i].ToString());
        }

        // [최종 접속 시간]
        user.lastCheckTime = bro.GetReturnValuetoJSON()["rows"][0]["LastCheckTime"][0].ToString();

        // [스타 재화 (활동 포인트)]
        user.activePoint = int.Parse(bro.GetReturnValuetoJSON()["rows"][0]["ActivePoint"][0].ToString());

        // [다음 스타 재화 획득 시간]
        for (int i = 0; i < bro.GetReturnValuetoJSON()["rows"][0]["ActivePointTime"][0].Count; i++)
        {
            user.activePointTime[i] = bro.FlattenRows()[0]["ActivePointTime"][i].ToString();
        }

        // [일일 상점 정보 (2차원 배열)]
        for (int i = 0; i < 3; i++) // (총 품목 개수 = 3)
        {
            for (int j = 0; j < 2; j++) // (인덱스, 살수있는 개수)
            {
                user.dailyShopInfo[i, j] = int.Parse(bro.FlattenRows()[0]["DailyShopInfo"][i][j].ToString());
            }
        }

        // [토큰 상점 정보 (2차원 배열)]
        for (int i = 0; i < 6; i++) // (총 품목 개수 = 6)
        {
            for (int j = 0; j < 2; j++) // (인덱스, 살수있는 개수)
            {
                user.tokenShopInfo[i, j] = int.Parse(bro.FlattenRows()[0]["TokenShopInfo"][i][j].ToString());
            }
        }

        CheckActivePoint();

        Debug.Log("기존 유저 데이터 불러오기 완료");
    }

    public void DB_Add(string idText, string pwText, string userName) // 회원 가입 시 데이터 초기값 삽입
    {
        user.UserID = idText;
        user.Password = pwText;
        user.UserName = userName;

        Param param = new Param(); // DB에 저장할 데이터들

        param.Add("UserID", user.UserID);
        param.Add("Password", user.Password);
        param.Add("UserName", user.UserName);
        param.Add("Goods", user.goods);
        param.Add("ClearInfo", user.clearInfo);
        param.Add("StageTotalInfo", user.stageTotalInfo);
        param.Add("TokenInfo", user.tokenInfo);
        param.Add("Tokens", user.tokens);
        param.Add("HousingObject", user.housingObject);

        AddCharacter(101);

        param.Add("Tail", user.tail);
        param.Add("DayQuestInfo", user.dayQuestInfo);
        param.Add("QuestRewardCount", user.questRewardCount);
        param.Add("QuestRewardInfo", user.questRewardInfo);
        user.lastCheckTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        param.Add("LastCheckTime", user.lastCheckTime);
        user.activePoint = 5;
        param.Add("ActivePoint", user.activePoint);
        for (int i = 0; i < 3; i++)
        {
            int rand = UnityEngine.Random.Range(0, user.tokens.Length);
            user.dailyShopInfo[i, 0] = rand;
            user.dailyShopInfo[i, 1] = 1;
        }
        param.Add("DailyShopInfo", user.dailyShopInfo);

        for (int i = 0; i < 6; i++)
        {
            int rand = UnityEngine.Random.Range(0, user.tokens.Length);
            user.tokenShopInfo[i, 0] = rand;
            user.tokenShopInfo[i, 1] = 3;
        }
        param.Add("TokenShopInfo", user.tokenShopInfo);


        for (int i = 0; i < 5; i++)
        {
            user.activePointTime[i] = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        }

        param.Add("ActivePointTime", user.activePointTime);

        // Prototype - 30개 모두 해금
        for (int i = 0; i < 30; i++)
        {
            AddTail(i);
        }

        param.Add("CurrentCharacterIndex", 0);
        param.Add("CurrentTailIndex", 0);

        Backend.GameData.Insert("User", param); // User 테이블에 데이터 삽입

        Debug.Log("새로운 유저 데이터 초기값 설정 완료");
    }

    public void AddCharacter(int index) // 최초 캐릭터 획득 시 호출
    {
        List<Character> characters = ChartManager.instance.characterDatas; // 캐싱

        for (int i = 0; i < characters.Count; i++)
        {
            Debug.Log($"index : {index}");

            if (characters[i].index == index)
            {
                Character chr = new Character();
                chr.index = characters[i].index;
                chr.imageIndex = characters[i].imageIndex;
                chr.lookImageIndex = characters[i].lookImageIndex;
                chr.name = characters[i].name;
                chr.color = characters[i].color;
                chr.level = characters[i].level;
                chr.count = characters[i].count;
                chr.healthIncreaseRate = characters[i].healthIncreaseRate;
                chr.maxHealth = characters[i].maxHealth;
                chr.maxSpeed = characters[i].maxSpeed;
                chr.maxSightRange = characters[i].maxSightRange;
                chr.minSightRange = characters[i].minSightRange;
                chr.activeSkill = characters[i].activeSkill;
                chr.passiveSkill = characters[i].passiveSkill;
                user.character.Add(chr);
                break;
            }
        }
        Debug.Log("캐릭터 추가 완료");
    }

    public void AddMyHousingObject(int index, float x, float y) // 하우징 오브젝트를 아예 새로 배치할 때
    {
        MyHousingObject current = new MyHousingObject();
        current.index = index;
        current.x = x;
        current.y = y;

        // user 클래스의 리스트에 값 저장
        user.myHousingObject.Add(current);
    }

    public void MoveMyHousingObject(int index, float original_x, float original_y, float new_x, float new_y) // 배치된 하우징 오브젝트를 이동시킬 때
    {
        //Debug.Log($"index = {index}, original_x,y = {original_x}, {original_y}");

        for (int i = 0; i < user.myHousingObject.Count; i++)
        {
            //Debug.Log($"[비교] index = {user.myHousingObject[i].index}, original_x,y = {user.myHousingObject[i].x}, {user.myHousingObject[i].y}");

            if (user.myHousingObject[i].index == index && user.myHousingObject[i].x == original_x && user.myHousingObject[i].y == original_y)
            {
                MyHousingObject temp = user.myHousingObject[i];
                temp.x = new_x;
                temp.y = new_y;
                user.myHousingObject[i] = temp;

                Debug.Log($"새로운 x:{user.myHousingObject[i].x}, 새로운 y:{user.myHousingObject[i].y} 에 오브젝트 이동!");

                return;
            }
        }
    }

    public void RemoveMyHousingObject(int index, float x, float y) // 배치된 하우징 오브젝트를 넣을 때
    {
        for (int i = 0; i < user.myHousingObject.Count; i++)
        {
            if (user.myHousingObject[i].index == index && user.myHousingObject[i].x == x && user.myHousingObject[i].y == y)
            {
                user.myHousingObject.RemoveAt(i);
                return;
            }
        }
    }

    public void AddTail(int index)
    {
        user.tail[index] = 1;
    }

    public int CharacterIndexMatching(int index)
    {
        int returnIndex = -1;

        switch (index)
        {
            case 101:
                returnIndex = 0;
                break;

            case 102:
                returnIndex = 1;
                break;

            case 103:
                returnIndex = 2;
                break;

            case 104:
                returnIndex = 3;
                break;

            case 201:
                returnIndex = 4;
                break;

            case 202:
                returnIndex = 5;
                break;

            case 203:
                returnIndex = 6;
                break;

            case 204:
                returnIndex = 7;
                break;

            case 301:
                returnIndex = 8;
                break;

            case 302:
                returnIndex = 9;
                break;

            case 303:
                returnIndex = 10;
                break;

            case 304:
                returnIndex = 11;
                break;

            case 401:
                returnIndex = 12;
                break;

            case 402:
                returnIndex = 13;
                break;

            case 403:
                returnIndex = 14;
                break;

            case 404:
                returnIndex = 15;
                break;

            case 501:
                returnIndex = 16;
                break;

            case 502:
                returnIndex = 17;
                break;

            case 503:
                returnIndex = 18;
                break;

            case 504:
                returnIndex = 19;
                break;

            default:
                break;
        }

        return returnIndex;
    }

    public bool DailyReset() // 6시 기준으로 지났는가 안지났는가
    {
        DateTime dt_last = DateTime.ParseExact(user.lastCheckTime, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);

        // # 매일 6시마다 리셋
        // 기준점(내일 6시)이 될 DateTime 선언
        DateTime point_Day = DateTime.Now;

        // 6시 이후면 기준점을 다음날로 설정
        if (point_Day.Hour >= 6)
        {
            point_Day = point_Day.AddDays(1);
        }

        // Point를 6시로 설정
        DateTime next6AM = new DateTime(point_Day.Year, point_Day.Month, point_Day.Day, 6, 0, 0);

        // 최종 접속 시간이 6시 이전이고, 현재 시간이 6시 이후인 경우 True 반환
        if (DateTime.Compare(dt_last, next6AM) == -1 && DateTime.Compare(DateTime.Now, next6AM) >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool WeeklyReset() // 월요일 6시 기준으로 지났는가 안지났는가
    {
        DateTime dt_last = DateTime.ParseExact(user.lastCheckTime, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);

        // # 매주 월요일 6시마다 리셋
        // 기준점(최근 월요일 6시)이 될 DateTime 선언
        DateTime point_Week = DateTime.Now;

        // 현재 요일이 월요일이 아닌 경우, 가장 최근 월요일까지 날짜를 이동
        while (point_Week.DayOfWeek != DayOfWeek.Monday)
        {
            point_Week = point_Week.AddDays(-1);
        }

        // Point를 가장 최근 월요일의 6시로 설정
        DateTime nextMonday6AM = new DateTime(point_Week.Year, point_Week.Month, point_Week.Day, 6, 0, 0);

        // 최종 접속 시간이 월요일 6시 이전이고, 현재 시간이 월요일 6시 이후인 경우 True 반환
        if (DateTime.Compare(dt_last, nextMonday6AM) == -1 && DateTime.Compare(DateTime.Now, nextMonday6AM) >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AllResetCheck()
    {
        // 퀘스트 리셋
        QuestResetCheck();

        // 상점 리셋
        ShopResetCheck();

        // 최종 체크 시간 갱신
        user.lastCheckTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
    }

    public void ShopResetCheck()
    {
        // # 매일 6시마다 리셋
        if (DailyReset())
        {
            for (int i = 0; i < 3; i++)
            {
                int rand = UnityEngine.Random.Range(0, user.tokens.Length);
                user.dailyShopInfo[i, 0] = rand;
                user.dailyShopInfo[i, 1] = 1;
            }
            for (int i = 0; i < 6; i++)
            {
                int rand = UnityEngine.Random.Range(0, user.tokens.Length);
                user.tokenShopInfo[i, 0] = rand;
                user.tokenShopInfo[i, 1] = 3;
            }
            OnResetDay?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ShopResetCheck_Token()
    {
        //즉시 갱신
        for (int i = 0; i < 6; i++)
        {
            int rand = UnityEngine.Random.Range(0, user.tokens.Length);
            user.tokenShopInfo[i, 0] = rand;
            user.tokenShopInfo[i, 1] = 3;
        }
    }

    public void QuestResetCheck()
    {
        // # 매일 6시마다 리셋
        if (DailyReset())
        {
            for (int i = 0; i < user.dayQuestInfo.Length; i++)
            {
                user.dayQuestInfo[i] = 0;
            }
        }

        // # 매주 월요일 6시마다 리셋
        if (WeeklyReset())
        {
            // 퀘스트 완료 횟수 초기화
            user.questRewardCount = 0;

            // 퀘스트 보상 획득 여부 초기화
            for (int i = 0; i < user.questRewardInfo.Length; i++)
            {
                user.questRewardInfo[i] = 0;
            }
        }
    }

    public void UseActivePoint()
    {
        user.activePoint -= 1;

        for (int i = 0; i < 5; i++)
        {
            user.activePointTime[i] = DateTime.Now.AddSeconds(300 * (i + 1)).ToString("yyyy-MM-dd-HH-mm-ss");
        }
    }

    public void CheckActivePoint()
    {
        for (int i = 0; i < user.activePointTime.Length; i++)
        {
            if (user.activePointTime[i] == null)
            {
                return;
            }
        }

        DateTime dt_apTime;

        for (int i = 0; i < 5; i++)
        {
            if (user.activePoint >= 5)
            {
                return;
            }

            dt_apTime = DateTime.ParseExact(user.activePointTime[i], "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);

            if (DateTime.Compare(dt_apTime, DateTime.Now) == -1)
            {
                user.activePoint += 1;
            }
        }
    }

    public IEnumerator UpdateTimeLeftText_co()
    {
        TimeSpan dateDiff;

        WaitForSeconds wfs = new WaitForSeconds(1.0f);

        // 기준점(내일 월요일 6시)이 될 DateTime 선언
        DateTime point = DateTime.Now;
        point = point.AddDays(1);

        // Point를 내일 오전 6시로 설정
        DateTime next6AM = new DateTime(point.Year, point.Month, point.Day, 6, 0, 0);

        while (true)
        {
            if (user.isLogin)
            {
                dateDiff = next6AM - DateTime.Now;
                user.timeLeftText = $"갱신까지 {dateDiff.Hours}시간 {dateDiff.Minutes}분";

                // 이벤트 호출
                TimerEvent?.Invoke(this, EventArgs.Empty);

                AllResetCheck();
            }

            yield return wfs;
        }
    }

    public void SaveUserData()
    {
        // User 정보 갱신
        Param param = new Param();
        param.Add("CurrentCharacterIndex", user.currentCharacterIndex);
        param.Add("CurrentTailIndex", user.currentTailIndex);
        param.Add("Goods", user.goods);
        param.Add("ClearInfo", user.clearInfo);
        param.Add("StageTotalInfo", user.stageTotalInfo);
        param.Add("TokenInfo", user.tokenInfo);
        param.Add("Tokens", user.tokens);
        param.Add("HousingObject", user.housingObject);
        param.Add("Tail", user.tail);
        param.Add("DayQuestInfo", user.dayQuestInfo);
        param.Add("QuestRewardCount", user.questRewardCount);
        param.Add("QuestRewardInfo", user.questRewardInfo);
        param.Add("LastCheckTime", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        param.Add("ActivePoint", user.activePoint);
        param.Add("ActivePointTime", user.activePointTime);
        param.Add("DailyShopInfo", user.dailyShopInfo);
        param.Add("TokenShopInfo", user.tokenShopInfo);

        Backend.PlayerData.UpdateMyLatestData("User", param);

        // Housing 정보 갱신
        Where where = new Where();
        where.Equal("owner_inDate", Backend.UserInDate);
        var h_bro = Backend.GameData.GetMyData("Housing", where);
        if (h_bro.GetReturnValuetoJSON()["rows"].Count > 0)
        {
            for (int i = 0; i < h_bro.FlattenRows().Count; i++)
            {
                Backend.PlayerData.DeleteMyLatestData("Housing");
            }
        }
        for (int i = 0; i < user.myHousingObject.Count; i++)
        {
            Param h_Param = new Param(); // 하우징 오브젝트 정보
            Debug.Log($"X:{user.myHousingObject[i].x}, Y:{user.myHousingObject[i].y} 에 Index:{user.myHousingObject[i].index}의 하우징 오브젝트 저장!");
            h_Param.Add("Index", user.myHousingObject[i].index);
            h_Param.Add("X", user.myHousingObject[i].x);
            h_Param.Add("Y", user.myHousingObject[i].y);
            Backend.GameData.Insert("Housing", h_Param); // Housing 테이블에 데이터 삽입
        }

        // Character 정보 갱신
        var c_bro = Backend.GameData.GetMyData("Character", where);
        if (c_bro.GetReturnValuetoJSON()["rows"].Count > 0)
        {
            for (int i = 0; i < c_bro.FlattenRows().Count; i++)
            {
                Backend.PlayerData.DeleteMyLatestData("Character");
            }
        }
        for (int i = 0; i < user.character.Count; i++)
        {
            Param characterParam = new Param(); // Character 정보
            characterParam.Add("Index", user.character[i].index);
            characterParam.Add("ImageIndex", user.character[i].imageIndex);
            characterParam.Add("LookImageIndex", user.character[i].lookImageIndex);
            characterParam.Add("Name", user.character[i].name);
            characterParam.Add("Color", user.character[i].color);
            characterParam.Add("Level", user.character[i].level);
            characterParam.Add("Count", user.character[i].count);
            characterParam.Add("HealthIncreaseRate", user.character[i].healthIncreaseRate);
            characterParam.Add("MaxHealth", user.character[i].maxHealth);
            characterParam.Add("MaxSpeed", user.character[i].maxSpeed);
            characterParam.Add("MinSpeed", user.character[i].minSpeed);
            characterParam.Add("MaxSightRange", user.character[i].maxSightRange);
            characterParam.Add("MinSightRange", user.character[i].minSightRange);
            characterParam.Add("ActiveSkill", user.character[i].activeSkill);
            characterParam.Add("PassiveSkill", user.character[i].passiveSkill);

            Backend.GameData.Insert("Character", characterParam); // Character 테이블에 데이터 삽입
        }

        Debug.Log($"게임 종료 시 데이터 DB에 저장");
    }
}
