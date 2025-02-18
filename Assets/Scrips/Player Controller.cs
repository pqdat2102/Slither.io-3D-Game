using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;  // Tốc độ di chuyển của đầu rắn
    [SerializeField] float steerSpeed = 180f; // Tốc độ xoay của đầu rắn

    public int Gap;  // Khoảng cách giữa các phần thân rắn

    public GameObject bodyPrefab;  // Prefab của phần thân rắn

    // Danh sách các phần thân rắn
    private List<GameObject> BodyParts = new List<GameObject>();

    // Danh sách lưu trữ lịch sử vị trí của đầu rắn
    private List<Vector3> PositionHistory = new List<Vector3>();

    public FoodSpawner foodSpawner;

    private int Score;
    public Text score;

    private void Start()
    {
        for(int i = 0; i < 10; i++)
        {
            GrowSnake();
        }
    }

    private void Update()
    {
        Move();

        // Cập nhật lịch sử vị trí của đầu rắn (chèn vị trí hiện tại vào đầu danh sách)
        PositionHistory.Insert(0, transform.position);

        int index = 0; 
        foreach (var body in BodyParts)
        {
            // Lấy vị trí mục tiêu của phần thân theo Gap (khoảng cách)
            Vector3 point = PositionHistory[Mathf.Clamp(index * Gap, 0, PositionHistory.Count - 1)];

            // Tính toán vector di chuyển từ vị trí hiện tại của phần thân đến vị trí mục tiêu
            Vector3 moveDirection = point - body.transform.position;

            // Di chuyển phần thân theo hướng vector đã tính
            body.transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // Quay phần thân hướng về vị trí mục tiêu
            body.transform.LookAt(point);

            index++;
        }

        score.text = Score.ToString();
    }

    // Hàm điều khiển di chuyển đầu rắn
    private void Move()
    {
        // Di chuyển đầu rắn về phía trước theo hướng nó đang đối mặt (transform.forward)
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // Lấy input từ người chơi để xoay đầu rắn (trục ngang)
        float steerDirection = Input.GetAxis("Horizontal");

        // Xoay đầu rắn theo hướng người chơi yêu cầu
        transform.Rotate(Vector3.up * steerDirection * steerSpeed * Time.deltaTime);
    }

    // Hàm tạo ra một phần thân mới và thêm vào danh sách BodyParts
    private void GrowSnake()
    {
        // Thêm một phần thân mới vào cuối danh sách BodyParts
        GameObject body = Instantiate(bodyPrefab);
        body.transform.position = BodyParts.Count > 0 ? BodyParts[BodyParts.Count - 1].transform.position : transform.position;


        // Gắn phần thân mới vào cuối danh sách
        BodyParts.Add(body);
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Food")
        {
            GrowSnake();
            foodSpawner.RespawnFood(other.gameObject);
            Score++;
        }

        if (other.gameObject.CompareTag("Enemy") && other.gameObject != this.gameObject)
        {
            Die();
        }
    }

    private void Die()
    {
        // Tắt tất cả các phần thân thay vì xóa
        foreach (var body in BodyParts)
        {
            body.SetActive(false);  // Tắt phần thân
        }

        /*// Xóa phần thân khỏi danh sách BodyParts
        BodyParts.Clear();
*/
        // Gọi FoodSpawner để tạo thức ăn
        if (foodSpawner != null)
        {
            foodSpawner.CreateFoodFromBodyParts(BodyParts);
        }
    }
}
