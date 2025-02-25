using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SnakeAISpawner : MonoBehaviour
{
    public GameObject aiSnakePrefab;
    public int initialPoolSize = 3;
    public Vector3 spawnAreaMin = new Vector3(-150, 0, -150);
    public Vector3 spawnAreaMax = new Vector3(150, 0, 150);
    public SnakePlayerController player;
    private List<GameObject> aiSnakePool = new List<GameObject>();
    private const float MIN_DISTANCE_FROM_PLAYER = 15f;
    private const float MAX_DISTANCE_FROM_PLAYER = 30f;

    void Start()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            SpawnAISnake();
        }
    }

    public void SpawnAISnake()
    {
        GameObject aiSnake = Instantiate(aiSnakePrefab);
        aiSnake.SetActive(false);
        aiSnake.tag = "Enemy";

        SnakeAIController controller = aiSnake.GetComponent<SnakeAIController>();// thêm Snake AI Controller cho rắn mới tạo

        controller.snakeId = aiSnakePool.Count + 1;// gắn ID cho rắn

        aiSnakePool.Add(aiSnake);// thêm vào pool Snake AI

        aiSnake.transform.position = GetSafeSpawnPosition(); // Đặt transform rắn AI ở vùng an toàn ( tránh đặt ra cái chết luôn )
        aiSnake.SetActive(true);
        aiSnake.transform.parent = transform;

        controller.InitializeSnake(); // làm những thứ liên quan đến rắn AI chết và spawn lại
    }

    public void ResetAISnake(SnakeAIController aiSnakeController) // reset khi rắn AI chết và cần tạo lại trong scene
    {
        GameObject aiSnake = aiSnakeController.gameObject;
        aiSnake.transform.position = GetSafeSpawnPosition();
        aiSnake.transform.rotation = Quaternion.identity;
        aiSnakeController.InitializeSnake(); // Đã bao gồm reset scale và gap
    }

    private Vector3 GetSafeSpawnPosition()
    {
        Vector3 position; // giá trị trả về
        int attempts = 0; // biến đếm số lần thử tìm vị trí
        const int maxAttempts = 20; // số lần thử tối đa

        do
        {
            position = new Vector3(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                0.5f,
                Random.Range(spawnAreaMin.z, spawnAreaMax.z)
            );

            float distance = Random.Range(MIN_DISTANCE_FROM_PLAYER, MAX_DISTANCE_FROM_PLAYER);
            Vector3 direction = Random.onUnitSphere;
            direction.y = 0;
            position = player.transform.position + direction.normalized * distance;

            position.x = Mathf.Clamp(position.x, spawnAreaMin.x, spawnAreaMax.x);
            position.z = Mathf.Clamp(position.z, spawnAreaMin.z, spawnAreaMax.z);

            attempts++;
            if (attempts > maxAttempts)
            {
                Debug.LogWarning("Không tìm thấy vùng an toàn");
                return position;
            }
        } while (Vector3.Distance(position, player.transform.position) < MIN_DISTANCE_FROM_PLAYER ||
                 Vector3.Distance(position, player.transform.position) > MAX_DISTANCE_FROM_PLAYER);

        return position;
    }

    public void ResetAI() // gọi khi reset game
    {
        foreach (var aiSnake in aiSnakePool.ToList())
        {
            if (aiSnake != null)
            {
                SnakeAIController controller = aiSnake.GetComponent<SnakeAIController>();
                if (controller != null)
                {
                    foreach (GameObject body in controller.bodyParts)
                    {
                        if (body != null)
                        {
                            Destroy(body);
                        }
                    }
                    controller.bodyParts.Clear();
                }
                Destroy(aiSnake);
            }
        }
        aiSnakePool.Clear();

        for (int i = 0; i < initialPoolSize; i++)
        {
            SpawnAISnake();
        }
    }
}