using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab; // Prefab của thức ăn
    public int foodCount = 5; // Số lượng thức ăn tối đa xuất hiện cùng lúc
    public Vector3 spawnAreaMin = new Vector3(-10, 0, -10); // Góc dưới trái khu vực spawn
    public Vector3 spawnAreaMax = new Vector3(10, 0, 10); // Góc trên phải khu vực spawn

    private List<GameObject> foodPool = new List<GameObject>(); // Danh sách chứa thức ăn tái sử dụng

    private void Start()
    {
        for (int i = 0; i < foodCount; i++)
        {
            SpawnNewFood();
        }
    }

    // Hàm spawn thức ăn mới
    private void SpawnNewFood()
    {
        GameObject food = Instantiate(foodPrefab);
        food.transform.position = GetRandomPosition();
        food.SetActive(true);
        foodPool.Add(food);
    }

    // Hàm làm thức ăn xuất hiện lại
    public void RespawnFood(GameObject food)
    {
        food.SetActive(false); // Tắt thức ăn cũ
        food.transform.position = GetRandomPosition(); // Di chuyển đến vị trí mới
        food.SetActive(true); // Bật lại thức ăn
    }

    // Hàm tìm vị trí ngẫu nhiên cho thức ăn
    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float z = Random.Range(spawnAreaMin.z, spawnAreaMax.z);
        return new Vector3(x, 0.25f, z);
    }

    /// <summary>
    /// Tạo thức ăn dọc theo vị trí của các phần thân
    /// </summary>
    public void CreateFoodFromBodyParts(List<GameObject> bodyParts)
    {
        // Kiểm tra nếu danh sách không rỗng
        if (bodyParts.Count == 0) return;

        // Tạo thức ăn dọc theo chiều dài của cơ thể (50% chiều dài)
        int foodCount = Mathf.CeilToInt(bodyParts.Count * 0.5f); // Tạo thức ăn bằng 50% chiều dài cơ thể
        
        // Tạo thức ăn tại các vị trí trong danh sách phần thân
        for (int i = 0; i < foodCount; i++)
        {
            // Lấy vị trí ngẫu nhiên trong các phần thân
            int index = Random.Range(0, bodyParts.Count);
            Debug.Log(index);
            Vector3 foodPosition = bodyParts[index].transform.position;
            Debug.Log(foodPosition);
            // Tạo thức ăn tại vị trí đó
            GameObject food = Instantiate(foodPrefab, foodPosition, Quaternion.identity);
            food.tag = "Food"; // Gán tag cho thức ăn
        }
    }
}

