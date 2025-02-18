using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISnakeController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;   // Tốc độ di chuyển
    [SerializeField] private float steerSpeed = 100f; // Giảm tốc độ xoay để cua mượt hơn
    [SerializeField] private float decisionDelay = 3f; // Delay trước khi tìm thức ăn mới
    [SerializeField] private float fixedHeight = 0.5f; // Độ cao cố định

    public FoodSpawner foodSpawner;
    public GameObject bodyPrefab;  // Prefab phần thân rắn
    public int Gap = 2;  // Khoảng cách giữa các phần thân

    private List<GameObject> BodyParts = new List<GameObject>(); // Danh sách phần thân
    private List<Vector3> PositionHistory = new List<Vector3>(); // Lịch sử vị trí đầu rắn

    private GameObject targetFood; // Mục tiêu của rắn
    private bool isWaitingForFood = false; // Kiểm soát trạng thái chờ tìm thức ăn

    private void Start()
    {
        // Tạo các phần thân rắn ban đầu (2 phần thân)
        for (int i = 0; i < 10; i++)
        {
            GrowSnake();
        }
    }

    private void Update()
    {
        // Rắn AI luôn di chuyển về phía trước
        MoveForward();

        // Nếu đang không tìm thức ăn và chưa có mục tiêu, chờ một khoảng thời gian rồi tìm
        if (!isWaitingForFood && (targetFood == null || !targetFood.activeInHierarchy))
        {
            StartCoroutine(FindFoodWithDelay());
        }

        // Nếu có thức ăn, di chuyển về hướng đó
        if (targetFood != null)
        {
            MoveTowardsFood();
        }

        // Lưu vị trí hiện tại vào lịch sử mỗi frame
        Vector3 position = transform.position;
        position.y = fixedHeight; // Giữ y cố định
        PositionHistory.Insert(0, position);

        // Giới hạn số lượng vị trí lưu trữ dựa trên độ dài của rắn
        int maxHistorySize = BodyParts.Count * Gap + 10; // Giữ thêm một ít để mượt hơn
        if (PositionHistory.Count > maxHistorySize)
        {
            PositionHistory.RemoveAt(PositionHistory.Count - 1);
        }

        // Cập nhật vị trí các phần thân rắn
        int index = 0;
        foreach (var body in BodyParts)
        {
            // Kiểm tra nếu danh sách đủ phần tử, tránh lỗi truy xuất ngoài phạm vi
            if (index * Gap < PositionHistory.Count)
            {
                // Lấy vị trí mục tiêu của phần thân theo khoảng cách Gap
                Vector3 point = PositionHistory[index * Gap];

                // Giữ rắn ở độ cao cố định
                point.y = fixedHeight;

                // Di chuyển phần thân rắn đến vị trí mục tiêu
                Vector3 moveDirection = point - body.transform.position;
                body.transform.position = Vector3.Lerp(body.transform.position, point, moveSpeed * Time.deltaTime);

                // Xoay phần thân về hướng mục tiêu một cách mượt mà
                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    body.transform.rotation = Quaternion.Slerp(body.transform.rotation, targetRotation, steerSpeed * Time.deltaTime);
                }
            }
            index++;
        }
    }

    /// <summary>
    /// Rắn AI luôn di chuyển về phía trước
    /// </summary>
    private void MoveForward()
    {
        Vector3 newPosition = transform.position + transform.forward * moveSpeed * Time.deltaTime;

        // Giữ độ cao (y) cố định
        newPosition.y = fixedHeight;

        // Cập nhật vị trí đầu rắn
        transform.position = newPosition;
    }

    /// <summary>
    /// Coroutine để trì hoãn trước khi tìm thức ăn mới
    /// </summary>
    private IEnumerator FindFoodWithDelay()
    {
        isWaitingForFood = true; // Bắt đầu chờ
        yield return new WaitForSeconds(decisionDelay); // Đợi trước khi tìm thức ăn
        FindNearestFood();
        isWaitingForFood = false; // Hết chờ
    }

    /// <summary>
    /// Tìm thức ăn gần nhất trong sân chơi
    /// </summary>
    private void FindNearestFood()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        if (foods.Length == 0) return;

        float minDistance = Mathf.Infinity;
        foreach (GameObject food in foods)
        {
            float distance = Vector3.Distance(transform.position, food.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                targetFood = food;
            }
        }
    }

    /// <summary>
    /// Di chuyển rắn về phía thức ăn nếu đã có mục tiêu
    /// </summary>
    private void MoveTowardsFood()
    {
        Vector3 direction = (targetFood.transform.position - transform.position).normalized;

        // Giữ rắn ở độ cao cố định (y = fixedHeight)
        direction.y = 0;

        // Xoay đầu rắn về hướng thức ăn một cách mượt mà
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, steerSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Khi rắn ăn thức ăn
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Food"))
        {
            GrowSnake();
            foodSpawner.RespawnFood(other.gameObject);
            targetFood = null; // Xóa mục tiêu cũ
            StartCoroutine(FindFoodWithDelay()); // Chờ trước khi tìm thức ăn mới
        }

        if (other.gameObject.CompareTag("Player") && other.gameObject != this.gameObject)
        {
            Die();
        }
    }

    /// <summary>
    /// Thêm phần thân mới vào rắn và đặt đúng vị trí
    /// </summary>
    private void GrowSnake()
    {
        GameObject body = Instantiate(bodyPrefab);

        // Nếu rắn đã có phần thân, đặt phần mới ở cuối đuôi
        if (BodyParts.Count > 0)
        {
            body.transform.position = BodyParts[BodyParts.Count - 1].transform.position;
        }
        else
        {
            body.transform.position = transform.position;
        }

        // Giữ độ cao cố định
        Vector3 fixedPosition = body.transform.position;
        fixedPosition.y = fixedHeight;
        body.transform.position = fixedPosition;

        // Đặt phần thân mới làm con của rắn AI
        body.transform.parent = transform;

        BodyParts.Add(body);
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
