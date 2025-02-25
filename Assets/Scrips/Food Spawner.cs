using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;      // Prefab của Food
    public int initialPoolSize = 20;    // Số lượng food ban đầu
    public Vector3 spawnAreaMin = new Vector3(-50, 0, -50);  // Vùng spawn min
    public Vector3 spawnAreaMax = new Vector3(50, 0, 50);   // Vùng spawn max

    private List<GameObject> foodPool = new List<GameObject>();  // Pool chứa thông tin của các food được tạo

    void Start()
    {
        // Tạo và thêm các food vào pool khi game bắt đầu
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject food = Instantiate(foodPrefab);
            food.SetActive(false);  // Tắt food ngay khi tạo
            foodPool.Add(food);
        }

        // Spawn food ngay khi game bắt đầu
        for (int i = 0; i < initialPoolSize; i++)
        {
            SpawnFood();
        }
    }

    // Hàm spawn food
    public void SpawnFood()
    {
        // Lấy một food không hoạt động từ pool
        GameObject food = GetInactiveFood();
        if (food != null)
        {
            food.transform.position = GetRandomPosition();  // Đặt vị trí ngẫu nhiên cho food
            food.SetActive(true);  // Kích hoạt food
        }
    }

    // Hàm lấy food không hoạt động trong pool
    private GameObject GetInactiveFood()
    {
        foreach (var food in foodPool)
        {
            if (!food.activeInHierarchy)  // Nếu food không hoạt động thì trả về
            {
                return food;
            }
        }

        // Nếu không có food không hoạt động, tạo mới một food và thêm vào pool
        GameObject newFood = Instantiate(foodPrefab);
        newFood.SetActive(false);  // Tắt food mới và thêm vào pool
        foodPool.Add(newFood);
        return newFood;
    }

    // Hàm reset lại tất cả các food trong pool
    public void ResetFood()
    {
        Debug.Log(foodPool.Count);
        // Tắt active tất cả các food hiện có trong scene
        GameObject[] allFoods = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject food in allFoods)
        {
            if (food != null)
            {
                food.SetActive(false);
            }
        }

        // Nếu số lượng food trong pool lớn hơn initialPoolSize, xóa bớt
        while (foodPool.Count > initialPoolSize)
        {
            GameObject foodToRemove = foodPool[foodPool.Count - 1]; // Lấy food cuối cùng
            foodPool.RemoveAt(foodPool.Count - 1); // Xóa khỏi danh sách
            if (foodToRemove != null)
            {
                Destroy(foodToRemove); // Xóa GameObject thừa
            }
        }

        // Đảm bảo pool có đủ initialPoolSize phần tử
        while (foodPool.Count < initialPoolSize)
        {
            GameObject newFood = Instantiate(foodPrefab);
            newFood.SetActive(false);
            foodPool.Add(newFood);
        }

        // Spawn lại tất cả food trong pool với vị trí mới
        foreach (GameObject food in foodPool)
        {
            if (food != null)
            {
                food.transform.position = GetRandomPosition();
                food.SetActive(true);
            }
        }

        Debug.Log("Food pool size after reset: " + foodPool.Count);
    }

    // Tạo vị trí ngẫu nhiên cho food
    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float z = Random.Range(spawnAreaMin.z, spawnAreaMax.z);
        return new Vector3(x, 0.25f, z);
    }

    // Respawn food khi bị ăn
    public void RespawnFood(GameObject food)
    {
        food.SetActive(false);  // Tắt food sau khi ăn
        food.transform.position = GetRandomPosition();  // Đặt lại vị trí ngẫu nhiên
        food.SetActive(true);  // Kích hoạt lại food
    }

    // Tạo food từ các phần thân đã chết
    /*public void CreateFoodFromBodyParts(List<GameObject> bodyParts)
    {
        if (bodyParts.Count == 0) return;
        int foodCount = Mathf.CeilToInt(bodyParts.Count * 0.5f);

        for (int i = 0; i < foodCount; i++)
        {
            int index = Random.Range(0, bodyParts.Count);
            Vector3 foodPosition = bodyParts[index].transform.position;
            foodPosition.y = 0.25f;
            GameObject food = Instantiate(foodPrefab, foodPosition, Quaternion.identity);

            foodPool.Add(food);// thêm food vừa tạo vào food pool để quản lý
            food.tag = "Food"; // thêm tag "Food" cho food mới được tạo
        }
    }*/
}