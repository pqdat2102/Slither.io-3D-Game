using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnakeAIController : Snake
{
    [SerializeField] private float decisionDelay = 2f;
    [SerializeField] private float fixedHeight = 0.5f;
    [SerializeField] private float foodTimeout = 3f;
    [SerializeField] private float avoidDistance = 2f;
    [SerializeField] private float randomSteerChance = 0.1f;

    public int snakeId;
    public SnakeAISpawner aiSnakeSpawner;
    public FoodSpawner foodSpawner;

    private GameObject targetFood;
    private bool isWaitingForFood = false;
    private float foodTimer = 0f;
    private List<GameObject> foodList = new List<GameObject>();
    private bool isDead = false;
    private string aiName;

    protected override void Start()
    {
        base.Start();
        InitializeSnake();
        GenerateRandomName();
        SetRandomInitialDirection();
    }

    protected override void Update()
    {
        if (isDead) return;
        base.Update();
        if (!isWaitingForFood && (targetFood == null || !targetFood.activeInHierarchy))
        {
            StartCoroutine(FindFoodWithDelay());
        }
        foodTimer += Time.deltaTime;
        if (foodTimer > foodTimeout && targetFood != null)
        {
            SwitchToNextFood();
            foodTimer = 0f;
        }
        AvoidObstacles();
        if (Random.value < randomSteerChance && targetFood == null)
        {
            RandomSteer();
        }
    }

    protected override void Move()
    {
        Vector3 newPosition = head.transform.position + head.transform.forward * moveSpeed * Time.deltaTime;
        newPosition.y = fixedHeight;
        head.transform.position = newPosition;

        if (targetFood != null)
        {
            MoveTowardsFood();
        }
        else
        {
            RandomSteer();
        }
    }

    private void SetRandomInitialDirection() // random hướng đi lúc tạo ra rắn
    {
        if (head != null)
        {
            float randomAngle = Random.Range(-45f, 45f);
            Quaternion randomRotation = Quaternion.Euler(0f, randomAngle, 0f) * head.transform.rotation;
            head.transform.rotation = randomRotation;
        }
    }

    private IEnumerator FindFoodWithDelay() // Delay thời gian tìm kiếm ( cho rắn di chuyển một lúc rồi mới tìm thức ăn tiếp )
    {
        isWaitingForFood = true;
        yield return new WaitForSeconds(decisionDelay);
        FindClosestSafeFoods();
        isWaitingForFood = false;
    }

    private void FindClosestSafeFoods() // Tìm kiếm những thức ăn gần con rắn đó
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        if (foods.Length == 0) return;

        foodList.Clear();
        foreach (GameObject food in foods)
        {
            Vector3 directionToFood = (food.transform.position - head.transform.position).normalized;
            RaycastHit hit;
            if (!Physics.Raycast(head.transform.position, directionToFood, out hit, Vector3.Distance(head.transform.position, food.transform.position), LayerMask.GetMask("SnakeBody")))
            {
                foodList.Add(food);
            }
        }
        foodList.Sort((f1, f2) => Vector3.Distance(head.transform.position, f1.transform.position).CompareTo(Vector3.Distance(head.transform.position, f2.transform.position)));
        targetFood = foodList.Count > 0 ? foodList[0] : null;
    }

    private void MoveTowardsFood() // Đi thẳng đến thức ăn gần nhất 
    {
        Vector3 direction = (targetFood.transform.position - head.transform.position).normalized;
        direction.y = 0;
        if (!WillCollide(direction))
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            head.transform.rotation = Quaternion.RotateTowards(head.transform.rotation, targetRotation, steerSpeed * Time.deltaTime);
        }
        else
        {
            RandomSteer();
        }
    }

    private void AvoidObstacles()
    {
        Collider[] hitColliders = Physics.OverlapSphere(head.transform.position, avoidDistance, LayerMask.GetMask("SnakeBody"));
        foreach (Collider hit in hitColliders)
        {
            if (!bodyParts.Contains(hit.gameObject))
            {
                Vector3 avoidDirection = (head.transform.position - hit.transform.position).normalized;
                Quaternion avoidRotation = Quaternion.LookRotation(avoidDirection);
                head.transform.rotation = Quaternion.RotateTowards(head.transform.rotation, avoidRotation, steerSpeed * Time.deltaTime);
                break;
            }
        }
    }

    private bool WillCollide(Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(head.transform.position, direction, out hit, avoidDistance, LayerMask.GetMask("SnakeBody")))
        {
            return !bodyParts.Contains(hit.collider.gameObject);
        }
        return false;
    }

    private void RandomSteer() // Quay đầu rắn ngẫy nhiên
    {
        float randomAngle = Random.Range(-45f, 45f);
        Quaternion randomRotation = Quaternion.Euler(0f, randomAngle, 0f) * head.transform.rotation;
        head.transform.rotation = Quaternion.RotateTowards(head.transform.rotation, randomRotation, steerSpeed * Time.deltaTime);
    }

    private void SwitchToNextFood() // Hướng rắn đến thức ăn gần nhất tiếp theo trong foodList
    {
        if (foodList.Count > 1)
        {
            targetFood = foodList[1];
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.CompareTag("Food"))
        {
            foodSpawner.RespawnFood(other.gameObject);
            targetFood = null;
            foodTimer = 0f;
        }
    }

    public override void Die()
    {
        isDead = true;
        foreach (GameObject body in bodyParts)
        {
            Destroy(body);
        }
        /*if (foodSpawner != null)
        {
            foodSpawner.CreateFoodFromBodyParts(bodyParts);
        }*/
        bodyParts.Clear();
        positionHistory.Clear();

        aiSnakeSpawner.ResetAISnake(this);
        isDead = false;
        GenerateRandomName();
    }

    public void InitializeSnake()
    {
        score = 0;
        // Xóa các phần thân cũ nếu còn sót lại
        foreach (GameObject body in bodyParts)
        {
            if (body != null) Destroy(body);
        }
        bodyParts.Clear();
        positionHistory.Clear();

        // Reset scale về baseScale
        ApplyScale(baseScale); // Sử dụng hàm từ lớp cha Snake

        // Tạo lại 2 phần thân
        for (int i = 0; i < 2; i++)
        {
            Grow();
        }
        SetRandomInitialDirection();
    }

    private void GenerateRandomName()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string randomName = "AI_";
        for (int i = 0; i < 3; i++)
        {
            randomName += chars[Random.Range(0, chars.Length)];
        }
        aiName = randomName;
    }

    public string GetAIName()
    {
        return aiName;
    }
}