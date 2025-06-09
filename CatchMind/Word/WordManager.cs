using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class Data
{
    public List<string> words = new List<string>();
}

public class WordManager : MonoBehaviour
{
    [Header("CSV")]
    [SerializeField] private TextAsset[] csvFiles = null;

    [Header("Words")]
    private Data words_data = new Data();

    private Dictionary<string, int> words_dict = new Dictionary<string, int>();

    private string path;

    private void Awake()
    {
        path = Path.Combine(Application.dataPath + "/Resources/Word/", "words.json");

        if (!File.Exists(path)) // json������ ���� ��� json���� ����
        {
            SetDictionary();
            SaveJsonFile();
        }

        LoadJsonFile();
    }

    private void SetDictionary()
    {
        for (int j = 0; j < csvFiles.Length; j++)
        {
            string csvText = csvFiles[j].text.Substring(0, csvFiles[j].text.Length - 1); // �� �� ����ִ� �� �� ����
            string[] rows = csvText.Split('\n'); // �ٹٲ� ���ڸ� �������� csv������ �ɰ� rows�� ����

            // ----------
            // currentWord[0] : �ܾ��̸�
            // currentWord[2] : ���� (�ܾ�)
            // currentWord[10] : ���� ��� (�ʱ�)
            // ----------

            for (int i = 1; i < rows.Length; i++) // ��� ���� ��ȸ (0��° �� ����)
            {
                if (rows[i].Length == 0)
                {
                    continue;
                }

                char first = rows[i][0];

                if ((0xAC00 <= first && first <= 0xD7A3) || (0x3131 <= first && first <= 0x318E))
                {
                    string[] currentWord = rows[i].Split(',');

                    if (rows.Length != 1)
                    {
                        if (!currentWord[0].Contains(" ") && currentWord[2].Equals("�ܾ�") && currentWord[3].Equals("���") && currentWord[10].Equals("�ʱ�"))
                        {
                            if (!words_dict.ContainsKey(currentWord[0].ToString().Trim()))
                            {
                                words_dict.Add(currentWord[0].ToString().Trim(), 1);
                            }
                        }
                    }
                }
            }
        }
    }

    private void SaveJsonFile()
    {
        Data saveData = new Data();

        foreach (KeyValuePair<string, int> kv in words_dict)
        {
            saveData.words.Add(kv.Key);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(path, json);
    }

    private void LoadJsonFile()
    {
        string loadJson = File.ReadAllText(path);
        words_data = JsonUtility.FromJson<Data>(loadJson);
    }

    public string GetRandomWord()
    {
        int index = Random.Range(0, words_data.words.Count - 1);

        return words_data.words[index];
    }
}
