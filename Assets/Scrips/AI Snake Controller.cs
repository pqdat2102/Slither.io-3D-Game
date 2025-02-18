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
    public int Gap = 30;  // Khoảng cách giữa các phần thân

    private List<GameObject> BodyParts = new List<GameObject>(); // Danh sách phần thân
    private List<Vector3> PositionHistory = new List<Vector3>(); // Lịch sử vị trí đầu rắn

    private GameObject targetFood; // Mục tiêu của rắn
    private bool isWaitingForFood = false; // Kiểm soát trạng thái chờ tìm thức ăn
    private float foodTimer = 0f; // Timer để kiểm tra thức ăn trong 5 giây
    private List<GameObject> foodList = new List<GameObject>();  // Danh sách các thức ăn gần nhất

    private void Start()
    {
        // Tạo các phần thân rắn ban đầu (10 phần thân)
        for (int i = 0; i < 10; i++)
        {
            GrowSnake();
        }
    }

    private void Update()
    {
        // Rắn AI luôn di chuyển về phía trước
        MoveForward();

        // Cập nhật lịch sử vị trí của đầu rắn
        Vector3 position = transform.position;
        position.y = fixedHeight; // Giữ y cố định
        PositionHistory.Insert(0, position);

       /* // Giới hạn số lượng vị trí lưu trữ dựa trên độ dài của rắn
        int maxHistorySize = BodyParts.Count * Gap + 10; // Giữ thêm một ít để mượt hơn
        if (PositionHistory.Count > maxHistorySize)
        {
            PositionHistory.RemoveAt(PositionHistory.Count - 1);
        }
*/
        // Cập nhật vị trí các phần thân rắn
        int index = 0;
        foreach (var body in BodyParts)
        {
            // Kiểm tra nếu danh sách đủ phần tử, tránh lỗi truy xuất ngoài phạm vi
            if (index * Gap < PositionHistory.Count)
            {
                // Lấy vị trí mục tiêu của phần thân theo khoảng cách Gap
                Vector3 point = PositionHistory[index * Gap];
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

        // Nếu không có thức ăn và không đang tìm thức ăn, tìm thức ăn gần nhất
        if (!isWaitingForFood && (targetFood == null || !targetFood.activeInHierarchy))
        {
            StartCoroutine(FindFoodWithDelay());
        }

        // Nếu có thức ăn, di chuyển về hướng đó
        if (targetFood != null)
        {
            MoveTowardsFood();
        }

        // Kiểm tra thời gian, nếu quá 5 giây, chuyển sang thức ăn gần tiếp theo
        foodTimer += Time.deltaTime;
        if (foodTimer > 5f && targetFood != null)
        {
            SwitchToNextFood();
            foodTimer = 0f;
        }
    }

    // Rắn AI luôn di chuyển về phía trước
    private void MoveForward()
    {
        Vector3 newPosition = transform.position + transform.forward * moveSpeed * Time.deltaTime;

        // Giữ độ cao (y) cố định
        newPosition.y = fixedHeight;

        // Cập nhật vị trí đầu rắn
        transform.position = newPosition;
    }

    // Coroutine để trì hoãn trước khi tìm thức ăn mới
    private IEnumerator FindFoodWithDelay()
    {
        isWaitingForFood = true; // Bắt đầu chờ
        yield return new WaitForSeconds(decisionDelay); // Đợi trước khi tìm thức ăn
        FindClosestFoods();
        isWaitingForFood = false; // Hết chờ
    }

    // Tìm 5 thức ăn gần nhất
    private void FindClosestFoods()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        if (foods.Length == 0) return;

        foodList.Clear();

        foreach (GameObject food in foods)
        {
            float distance = Vector3.Distance(transform.position, food.transform.position);
            foodList.Add(food);
        }

        // Sắp xếp thức ăn theo khoảng cách từ gần nhất
        foodList.Sort((f1, f2) => Vector3.Distance(transform.position, f1.transform.position).CompareTo(Vector3.Distance(transform.position, f2.transform.position)));

        // Lựa chọn thức ăn gần nhất
        targetFood = foodList.Count > 0 ? foodList[0] : null;
    }

    // Di chuyển rắn về phía thức ăn nếu đã có mục tiêu
    private void MoveTowardsFood()
    {
        if (targetFood == null) return;

        Vector3 direction = (targetFood.transform.position - transform.position).normalized;

        // Giữ rắn ở độ cao cố định (y = fixedHeight)
        direction.y = 0;

        // Xoay đầu rắn về hướng thức ăn một cách từ từ
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, steerSpeed * Time.deltaTime);
    }

    // Chuyển sang thức ăn gần tiếp theo
    private void SwitchToNextFood()
    {
        if (foodList.Count > 1)
        {
            targetFood = foodList[1]; // Chuyển sang thức ăn gần thứ 2
        }
    }

    // Khi rắn ăn thức ăn
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Food"))
        {
            GrowSnake();
            foodSpawner.RespawnFood(other.gameObject);
            targetFood = null; // Xóa mục tiêu cũ
            foodTimer = 0f; // Reset timer
        }

        if (other.gameObject.CompareTag("Player") && other.gameObject != this.gameObject)
        {
            Die();
        }
    }

    // Thêm phần thân mới vào rắn và đặt đúng vị trí
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

    // Khi rắn chết
    private void Die()
    {
        // Tắt tất cả các phần thân thay vì xóa
        foreach (var body in BodyParts)
        {
            body.SetActive(false);  // Tắt phần thân
        }

        // Gọi FoodSpawner để tạo thức ăn
        if (foodSpawner != null)
        {
            foodSpawner.CreateFoodFromBodyParts(BodyParts);
        }

        // Xóa phần thân khỏi danh sách BodyParts
        BodyParts.Clear();
    }

    public void ResetAI()
    {
        // Đặt lại vị trí và trạng thái của AI
        transform.position = new Vector3(5, 0.5f, 15);
        transform.rotation = Quaternion.identity;

        // Tạo lại các phần thân của AI
        foreach (var body in BodyParts)
        {
            Destroy(body);
        }
        BodyParts.Clear();

        for (int i = 0; i < 10; i++)
        {
            GrowSnake();
        }
    }
}
