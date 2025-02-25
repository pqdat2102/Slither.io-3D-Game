using UnityEngine;

public class GameController : MonoBehaviour
{
    public SnakePlayerController playerController;
    public SnakeAISpawner aiSnakeSpawner;
    public FoodSpawner foodSpawner;
    public Canvas gameOverCanvas;

    private void Start()
    {
        gameOverCanvas.enabled = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        gameOverCanvas.enabled = false;

        // Reset player
        if (playerController != null)
        {
            playerController.RestartPlayer();
        }

        // Reset AI Snakes - xóa hoàn toàn và tạo lại
        if (aiSnakeSpawner != null)
        {
            aiSnakeSpawner.ResetAI();
        }

        // Reset food - xóa hoàn toàn và tạo lại
        if (foodSpawner != null)
        {
            foodSpawner.ResetFood();
        }

        // Đảm bảo HighScoreTable được reset hoặc cập nhật lại tham chiếu
        HighScoreTable highScoreTable = FindFirstObjectByType<HighScoreTable>();
        if (highScoreTable != null)
        {
            highScoreTable.Start(); // Gọi lại Start để tìm lại các tham chiếu
        }
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        gameOverCanvas.enabled = true;
    }
}