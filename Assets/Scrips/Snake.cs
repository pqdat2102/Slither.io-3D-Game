using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms.Impl;

public abstract class Snake : MonoBehaviour
{
    public int score;
    public GameObject head;
    public List<GameObject> bodyParts = new List<GameObject>();
    public GameObject bodyPrefab;
    public float moveSpeed = 5f;
    public float steerSpeed = 180f;
    public int gap = 20; // Khoảng cách giữa các phần thân
    protected List<Vector3> positionHistory = new List<Vector3>();

    [SerializeField] protected float baseScale = 1f; // Kích thước cơ bản
    [SerializeField] private float scaleIncrement = 0.02f; // Mỗi lần ăn tăng thêm bao nhiêu scale
    [SerializeField] private float maxScale = 2f; // Giới hạn kích thước tối đa
    
    protected virtual void Start()
    {
        head = gameObject;
        ApplyScale(baseScale); // Áp dụng kích thước ban đầu
    }

    protected virtual void Update()
    {
        Move();
        UpdateBody();
    }

    protected abstract void Move();

    protected void UpdateBody()
    {
        positionHistory.Insert(0, head.transform.position);
        int index = 0;
        foreach (var body in bodyParts)
        {
            if (index * gap < positionHistory.Count && body != null)
            {
                Vector3 point = positionHistory[index * gap];
                body.transform.position = Vector3.Lerp(body.transform.position, point, moveSpeed * Time.deltaTime);
                Vector3 direction = point - body.transform.position;
                if (direction != Vector3.zero)
                {
                    body.transform.rotation = Quaternion.Slerp(body.transform.rotation, Quaternion.LookRotation(direction), steerSpeed * Time.deltaTime);
                }
            }
            index++;
        }
    }

    public void Grow()
    {
        GameObject body = Instantiate(bodyPrefab);
        body.transform.position = bodyParts.Count > 0 ? bodyParts[bodyParts.Count - 1].transform.position : head.transform.position;
        body.tag = "SnakeBody";

        // Lấy kích thước hiện tại của head và áp dụng cho phần thân mới
        float currentScale = head.transform.localScale.x;
        body.transform.localScale = Vector3.one * currentScale;

        bodyParts.Add(body);

        IncreaseSize();
    }

    protected void IncreaseSize()
    {
        float newScale = Mathf.Min(head.transform.localScale.x + scaleIncrement, maxScale);
        ApplyScale(newScale); // Cập nhật cả head và bodyParts cùng lúc
    }

    protected void ApplyScale(float scale)
    {
        // Áp dụng scale cho head
        head.transform.localScale = Vector3.one * scale;

        // Áp dụng scale cho tất cả phần thân
        foreach (var body in bodyParts)
        {
            if (body != null)
            {
                body.transform.localScale = Vector3.one * scale;
            }
        }
    }

    public abstract void Die();

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag("Food") && other.gameObject != null)
        {
            Grow();
            score += 1;
            FoodSpawner spawner = Object.FindFirstObjectByType<FoodSpawner>();
            if (spawner != null)
            {
                spawner.RespawnFood(other.gameObject);
            }
        }
        else if (other != null && other.CompareTag("SnakeBody") && other.gameObject != head)
        {
            if (!bodyParts.Contains(other.gameObject))
            {
                Die();
            }
        }
    }

   /* protected List<Vector3> GetBodyPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (var body in bodyParts)
        {
            if (body != null)
            {
                positions.Add(body.transform.position);
            }
        }
        return positions;
    }*/
}