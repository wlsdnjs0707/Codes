using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Round Setting")]
    public float timer;
    public int roundCount = 5;
    public float roundTime = 30.0f;

    [Header("Status")]
    public bool isTimerOn = false;
    public int currentRound = 0;
    private bool isCorrect = false; // ������ ������ ����°�

    [Header("Word Manager")]
    [SerializeField] private GameObject wordManager;
    public string currentWord;

    public event Action OnRoundChanged;

    private void Start()
    {
        StartRound();
    }

    private void StartRound()
    {
        if (isCorrect == true) // isCorrect ���� �ʱ�ȭ
        {
            isCorrect = false;
        }

        currentRound += 1;

        // �׸��� �׸� ��� ����, ���� �ο�

        // ���þ� �̱�
        currentWord = wordManager.GetComponent<WordManager>().GetRandomWord();

        // �׸��� �׸� ���(������ �ִ»��)���Ը� ���þ� �����ֱ�

        OnRoundChanged?.Invoke();

        StartTimer();
    }

    private IEnumerator EndRound_co()
    {
        // �ش� ������ ���� ��ο��� ����

        yield return new WaitForSeconds(3.0f);

        if (currentRound == roundCount - 1) // ������ ���� ���� ���
        {
            // ���� ����, ������ ���
        }
        else
        {
            // ���� ���� ����
        }

        yield break;
    }

    private void StartTimer()
    {
        if (timer == 0)
        {
            isTimerOn = true;
            timer = roundTime;
            StartCoroutine(Timer_co());
        }
    }

    private IEnumerator Timer_co()
    {
        while (timer > 0)
        {
            if (isCorrect == true)
            {
                timer = 0;
                isTimerOn = false;
                StartCoroutine(EndRound_co());
                yield break;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        timer = 0;
        isTimerOn = false;
        StartCoroutine(EndRound_co());
        yield break;
    }
}
