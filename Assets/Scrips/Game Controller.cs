using UnityEngine;

public class GameController : MonoBehaviour
{
    public PlayerController playerController;  // Tham chiếu đến PlayerController
    public AISnakeController aiSnakeController;    // Tham chiếu đến EnemyController
    public FoodSpawner foodSpawner;            // Tham chiếu đến FoodSpawner

    public Canvas gameOverCanvas;              // Canvas Game Over

    private void Start()
    {
        // Đảm bảo canvas Game Over ban đầu bị ẩn
        gameOverCanvas.enabled = false;
    }

    // Hàm Restart để reset game
    public void RestartGame()
    {
        // Đặt lại thời gian
        Time.timeScale = 1f;

        // Tắt canvas Game Over
        gameOverCanvas.enabled = false;

        // Reset player
        playerController.RestartPlayer();

        // Reset enemies
        aiSnakeController.ResetAI();

        // Reset food
        foodSpawner.ResetFood();
    }

    // Hàm gọi khi người chơi chết
    public void GameOver()
    {
        // Dừng thời gian (pause game)
        Time.timeScale = 0f;

        // Hiển thị canvas Game Over
        gameOverCanvas.enabled = true;
    }
}
