using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using TMPro;

public class HighScoreTable : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI highScoreText; // Tham chiếu đến Text component trong UI
    [SerializeField] private SnakePlayerController player; // Tham chiếu đến Player
    [SerializeField] private SnakeAISpawner aiSnakeSpawner; // Tham chiếu đến Spawner AI

    private bool isInitialized = false; // Biến kiểm tra để kiểm tra xem đã khởi tạo chưa

    private void Awake()
    {
        // Kiểm tra và gán highScoreText nếu chưa được gán trong Inspector
        if (highScoreText == null)
        {
            highScoreText = GetComponent<TextMeshProUGUI>();
            if (highScoreText == null)
            {
                Debug.LogError("HighScoreTable: không thấy highScoreText");
                enabled = false; // Tắt script nếu không tìm thấy highScoreText
                return;
            }
        }
    }

    public void Initialize()
    {
        // Cập nhật lại tất cả tham chiếu
        if (player == null)
        {
            player = FindFirstObjectByType<SnakePlayerController>();
        }

        if (aiSnakeSpawner == null)
        {
            aiSnakeSpawner = FindFirstObjectByType<SnakeAISpawner>();
        }

        // Kiểm tra nếu player hoặc aiSnakeSpawner không tồn tại
        if (player == null || aiSnakeSpawner == null)
        {
            Debug.LogWarning("High Score Table: Player hoặc aiSnakeSpawner không tồn tại hoặc chưa được thêm");
        }

        isInitialized = true; // Đánh dấu đã khởi tạo
    }

    public void Start()
    {
        Initialize(); // Gọi Initialize khi khởi tạo lần đầu
    }

    private void OnEnable()
    {
        // Đảm bảo khởi tạo lại khi script được bật (sau restart)
        if (!isInitialized)
        {
            Initialize();
        }
    }

    private void Update()
    {
        if (isInitialized && highScoreText != null && player != null && aiSnakeSpawner != null)
        {
            UpdateHighScoresInRealTime();
        }
        else
        {
            Debug.LogWarning("Bảng full, ngừng update");

            Initialize(); // Thử khởi tạo lại cái bảng nếu nó bị lỗi
        }
    }

    private void UpdateHighScoresInRealTime()
    {
        // Kiểm tra null trước khi sử dụng các tham chiếu
        if (highScoreText == null || player == null || aiSnakeSpawner == null)
        {
            Debug.LogError("Mất tham chiếu đến bảng điểm");
            enabled = false; // Tắt script nếu tham chiếu bị mất hoàn toàn
            return;
        }

        // Tạo danh sách điểm số hiện tại từ Player và AI
        List<HighScoreEntry> currentScores = new List<HighScoreEntry>();

        // Thêm Player vào danh sách (nếu tồn tại và đang được active)
        if (player != null && player.gameObject.activeInHierarchy)
        {
            currentScores.Add(new HighScoreEntry(player.GetPlayerName(), player.score));
        }

        // Thêm các rắn AI vào danh sách (nếu Spawner tồn tại)
        if (aiSnakeSpawner != null)
        {
            foreach (Transform child in aiSnakeSpawner.transform)
            {
                SnakeAIController aiSnake = child.GetComponent<SnakeAIController>();
                if (aiSnake != null && aiSnake.gameObject.activeInHierarchy)
                {
                    currentScores.Add(new HighScoreEntry(aiSnake.GetAIName(), aiSnake.score));
                }
            }
        }

        // Nếu không có điểm số nào, hiển thị thông báo rỗng hoặc không cập nhật
        if (currentScores.Count == 0)
        {
            highScoreText.text = "HIGH SCORES\n----------------------------------------\nNo scores available yet.";
            return;
        }

        // Sắp xếp theo điểm số giảm dần (top 10)
        currentScores = currentScores.OrderByDescending(x => x.score).Take(10).ToList();

        // Tạo chuỗi hiển thị bảng xếp hạng
        string displayText = "HIGH SCORES\n";
        displayText += "----------------------------------------\n";

        // Tiêu đề cột, độ rộng cố định để đảm bảo thẳng hàng
        displayText += string.Format("{0,-6} | {1,-8} | {2,-8}\n", "POS", "SCORE", "NAME");
        displayText += "----------------------------------------\n";

        for (int i = 0; i < currentScores.Count; i++)
        {
            string rankString = GetRankString(i + 1);
            string scoreString = currentScores[i].score.ToString().PadLeft(7); // Căn phải, độ rộng 7 cho SCORE
            string nameString = currentScores[i].name.PadRight(7); // Căn trái, độ rộng 7 cho NAME (cắt bớt nếu dài hơn)

            // Giới hạn độ dài tên nếu cần (tối đa 7 ký tự)
            if (nameString.Length > 7) nameString = nameString.Substring(0, 7);

            // Sử dụng string.Format để căn chỉnh và giữ độ rộng cố định
            displayText += string.Format("{0,-6} | {1,7} | {2,-7}\n",
                rankString,
                scoreString,
                nameString);
        }

        // Cập nhật nội dung của Text
        highScoreText.text = displayText;
    }

    private string GetRankString(int rank)
    {
        switch (rank)
        {
            case 1: return "1ST";
            case 2: return "2ND";
            case 3: return "3RD";
            default: return $"{rank}TH";
        }
    }

    [Serializable]
    private class HighScoreEntry
    {
        public string name;
        public int score;

        public HighScoreEntry(string name, int score)
        {
            this.name = name;
            this.score = score;
        }
    }
}