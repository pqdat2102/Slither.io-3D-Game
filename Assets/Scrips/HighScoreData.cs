using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HighScoreData
{
    public List<HighScoreEntry> highScores = new List<HighScoreEntry>();

    public HighScoreData()
    {
        // Constructor mặc định
    }
}

[Serializable]
public class HighScoreEntry
{
    public string name;
    public int score;

    public HighScoreEntry(string name, int score)
    {
        this.name = name;
        this.score = score;
    }
}

public static class HighScoreManager
{
    private static string SavePath => Application.persistentDataPath + "/highscores.json";

    public static void SaveHighScores(List<HighScoreEntry> scores)
    {
        HighScoreData data = new HighScoreData();
        data.highScores = scores;
        string json = JsonUtility.ToJson(data, true); // Pretty print JSON
        System.IO.File.WriteAllText(SavePath, json);
    }

    public static HighScoreData LoadHighScores()
    {
        if (System.IO.File.Exists(SavePath))
        {
            string json = System.IO.File.ReadAllText(SavePath);
            return JsonUtility.FromJson<HighScoreData>(json);
        }
        return new HighScoreData(); // Trả về dữ liệu mặc định nếu không tồn tại file
    }
}