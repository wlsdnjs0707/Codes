using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using BackEnd;
using System.Linq;

public class LoginManager : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] private GameObject popUp_Menu;

    [Header("LogIn")]
    [SerializeField] private GameObject popUp_LogIn;
    [SerializeField] private TMP_InputField inputFieldID;
    [SerializeField] private TMP_InputField inputFieldPW;
    [SerializeField] private Button btnLogin;
    [SerializeField] private Button btnGoToSignUp;
    [SerializeField] private TMP_Text logInErrorText;

    [Header("SignUp")]
    [SerializeField] private GameObject popUp_SignUp;
    [SerializeField] private TMP_InputField inputFieldSignUpID;
    [SerializeField] private TMP_InputField inputFieldSignUpPW_1;
    [SerializeField] private TMP_InputField inputFieldSignUpPW_2;
    [SerializeField] private Button btnSignUp;
    [SerializeField] private Button btnGoToLogIn;
    [SerializeField] private TMP_Text signUpErrorText;
    private string id;
    private string pw;

    [Header("UserName")]
    [SerializeField] private GameObject popUp_UserName;
    [SerializeField] private TMP_InputField inputFieldUserName;
    [SerializeField] private TMP_Text userNameErrorText;

    private void Start()
    {
        //inputFieldSignUpID.text = BackEndManager.Instance.googleHashKey;
        //signUpErrorText.text = BackEndManager.Instance.googleHashKey;
    }

    private void Update()
    {
        if (Backend.IsInitialized)
        {
            Backend.AsyncPoll();
        }
    }

    public void StartGoogleLogin()
    {
        TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin(GoogleLoginCallback);
    }

    private void GoogleLoginCallback(bool isSuccess, string errorMessage, string token)
    {
        if (isSuccess == false)
        {
            Debug.LogError(errorMessage);
            return;
        }

        Debug.Log("구글 토큰 : " + token);
        var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Google);
        Debug.Log("페데레이션 로그인 결과 : " + bro);
    }

    public void Login()
    {
        // 캐싱
        string idText = inputFieldID.text;
        string pwText = inputFieldPW.text;

        id = idText;
        pw = pwText;

        if (idText.Trim().Equals("") || pwText.Trim().Equals(""))
        {
            // InputField가 비워져 있을 때
            inputFieldID.text = "";
            inputFieldPW.text = "";
            logInErrorText.text = $"정보를 입력해주세요.";
            logInErrorText.gameObject.SetActive(true);
            btnLogin.interactable = true;
            return;
        }

        btnLogin.interactable = false;

        Backend.BMember.CustomLogin(idText, pwText, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log($"로그인 성공");

                Where where = new Where();
                where.Equal("owner_inDate", Backend.UserInDate); // 로그인 한 유저의 owner_inDate로 User DB 조회

                var bro = Backend.GameData.GetMyData("User", where);

                if (bro.IsSuccess() && (bro.GetReturnValuetoJSON()["rows"].Count == 0 || (bro.GetReturnValuetoJSON()["rows"].Count > 0) && bro.GetReturnValuetoJSON()["rows"][0]["UserName"][0].ToString().Equals(string.Empty)))
                {
                    Debug.Log("닉네임 재설정 필요");
                    btnLogin.interactable = true;
                    GoToSetUserName();
                }
                else
                {
                    Debug.Log("로그인 완료");
                    DBManager.instance.user.isLogin = true;
                    ChartManager.instance.GetChartData();
                    DBManager.instance.DB_Load(idText, pwText);
                    BackEndManager.Instance.GetMatchSystem().JoinMatchMaking();
                    BackEndManager.Instance.GetChatManager().GetChatStatus();
                }
            }
            else
            {
                inputFieldID.text = "";
                inputFieldPW.text = "";
                btnLogin.interactable = true;

                string message = string.Empty;

                switch (int.Parse(callback.GetStatusCode()))
                {
                    case 401:
                        message = callback.GetMessage().Contains("Id") ? "존재하지 않는 아이디입니다." : "잘못된 비밀번호입니다.";
                        break;
                    case 403:
                        message = callback.GetMessage().Contains("user") ? "차단당한 유저입니다." : "차단당한 디바이스입니다.";
                        break;
                    case 410:
                        message = "탈퇴가 진행중인 유저입니다.";
                        break;
                    default:
                        message = callback.GetMessage();
                        break;
                }

                logInErrorText.text = $"{message}";
                logInErrorText.gameObject.SetActive(true);
            }
        });
    }

    public void SignUp()
    {
        // 캐싱
        string idText = inputFieldSignUpID.text;
        string pwText_1 = inputFieldSignUpPW_1.text;
        string pwText_2 = inputFieldSignUpPW_2.text;

        if (idText.Trim().Equals("") || pwText_1.Trim().Equals("") || pwText_2.Trim().Equals(""))
        {
            // InputField가 비워져있을 때
            inputFieldSignUpID.text = "";
            inputFieldSignUpPW_1.text = "";
            inputFieldSignUpPW_2.text = "";
            signUpErrorText.text = "정보를 입력해주세요.";
            signUpErrorText.gameObject.SetActive(true);
            btnSignUp.interactable = true;
            return;
        }
        else if (pwText_1 != pwText_2)
        {
            // 패스워드가 일치하지 않을 때
            inputFieldSignUpID.text = "";
            inputFieldSignUpPW_1.text = "";
            inputFieldSignUpPW_2.text = "";
            signUpErrorText.text = "비밀번호가 일치하지 않습니다.";
            signUpErrorText.gameObject.SetActive(true);
            btnSignUp.interactable = true;
            return;
        }
        else if (idText.Any(x => char.IsWhiteSpace(x) == true) || pwText_1.Any(x => char.IsWhiteSpace(x) == true) || pwText_2.Any(x => char.IsWhiteSpace(x) == true))
        {
            // 공백이 포함되어 있을 때
            inputFieldSignUpID.text = "";
            inputFieldSignUpPW_1.text = "";
            inputFieldSignUpPW_2.text = "";
            signUpErrorText.text = "공백을 포함할 수 없습니다.";
            signUpErrorText.gameObject.SetActive(true);
            btnSignUp.interactable = true;
            return;
        }

        btnSignUp.interactable = false;

        Backend.BMember.CustomSignUp(idText, pwText_1, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log($"회원가입 성공");

                inputFieldSignUpID.text = "";
                inputFieldSignUpPW_1.text = "";
                inputFieldSignUpPW_2.text = "";
                btnSignUp.interactable = true;

                id = idText;
                pw = pwText_1;

                Backend.BMember.CustomLogin(idText, pwText_1);
                ChartManager.instance.GetChartData();
                DBManager.instance.DB_Add(id, pw, "");

                GoToSetUserName();
            }
            else
            {
                inputFieldSignUpID.text = "";
                inputFieldSignUpPW_1.text = "";
                inputFieldSignUpPW_2.text = "";
                btnSignUp.interactable = true;

                string message = string.Empty;

                switch (int.Parse(callback.GetStatusCode()))
                {
                    case 401:
                        message = "프로젝트 상태가 '점검'입니다.";
                        break;
                    case 403:
                        message = callback.GetMessage().Contains("blocked") ? "차단당한 디바이스입니다." : "출시 설정이 '테스트'인데 AU가 10을 초과하였습니다.";
                        break;
                    case 409:
                        message = "중복된 아이디입니다.";
                        break;
                    default:
                        message = callback.GetMessage();
                        break;
                }

                inputFieldSignUpID.text = "";
                inputFieldSignUpPW_1.text = "";
                inputFieldSignUpPW_2.text = "";
                signUpErrorText.text = $"{message}";
                signUpErrorText.gameObject.SetActive(true);
                btnSignUp.interactable = true;
            }
        });
    }

    public void SetUserName()
    {
        var bro = Backend.GameData.Get("User", new Where());

        string userNameText = inputFieldUserName.text;

        if (userNameText.Trim().Equals(""))
        {
            // InputField가 비워져있을 때
            inputFieldUserName.text = "";
            userNameErrorText.text = "정보를 입력해주세요.";
            userNameErrorText.gameObject.SetActive(true);
            return;
        }
        else if (userNameText.Any(x => char.IsWhiteSpace(x) == true))
        {
            // 공백이 포함되어 있을 때
            inputFieldUserName.text = "";
            userNameErrorText.text = "공백을 포함할 수 없습니다.";
            userNameErrorText.gameObject.SetActive(true);
            return;
        }

        if (bro.IsSuccess())
        {
            // 닉네임이 다른 유저의 닉네임과 중복될 때

            for (int i = 0; i < bro.GetReturnValuetoJSON()["rows"].Count; i++)
            {
                if (bro.GetReturnValuetoJSON()["rows"][i]["UserName"][0].ToString().Equals(userNameText))
                {
                    inputFieldUserName.text = "";
                    userNameErrorText.text = "사용중인 닉네임입니다.";
                    userNameErrorText.gameObject.SetActive(true);
                    return;
                }
            }
        }

        // 닉네임 항목에 입력한 유저 닉네임 할당
        Backend.BMember.CreateNickname(userNameText);

        Param param = new Param();
        param.Add("UserName", userNameText);

        // 해당 테이블 데이터 수정
        Backend.PlayerData.UpdateMyLatestData("User", param);

        GoToCustomLogIn();
    }

    public void GoToCustomLogIn()
    {
        popUp_Menu.SetActive(false);
        popUp_LogIn.SetActive(true);
        popUp_SignUp.SetActive(false);
        popUp_UserName.SetActive(false);
    }

    public void GoToSignUp()
    {
        popUp_Menu.SetActive(false);
        popUp_LogIn.SetActive(false);
        popUp_SignUp.SetActive(true);
        popUp_UserName.SetActive(false);
    }

    public void GoToMenu()
    {
        popUp_Menu.SetActive(true);
        popUp_LogIn.SetActive(false);
        popUp_SignUp.SetActive(false);
        popUp_UserName.SetActive(false);
    }

    public void GoToSetUserName()
    {
        popUp_Menu.SetActive(false);
        popUp_LogIn.SetActive(false);
        popUp_SignUp.SetActive(false);
        popUp_UserName.SetActive(true);
    }
}
