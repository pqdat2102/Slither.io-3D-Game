using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class SnakePlayerController : Snake
{
    public FoodSpawner foodSpawner;
    public Canvas gameOverCanvas;
    public Text scoreInGame; // Text UI trong Canvas để hiển thị điểm số trong game
    public Text scoreTextGameOver; // Text UI trong Canvas để hiển thị điểm số khi game over

    // Singleton instance
    public static SnakePlayerController Instance { get; private set; }

    private Renderer headRenderer; // Tham chiếu đến Renderer của head
    private List<Material> materialSequence = new List<Material>(); // Danh sách material riêng cho rắn player

    protected override void Start()
    {
        base.Start();

        // Khởi tạo Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Gán headRenderer từ head
        headRenderer = head.GetComponent<Renderer>();
        if (headRenderer == null)
        {
            Debug.LogError("Khong tim thay phan dau ran trong SnakePlayerController");
            return;
        }

        // Reset vị trí ban đầu cho head
        head.transform.position = new Vector3(5, 0.5f, 5);
        head.transform.rotation = Quaternion.identity;

        // Xóa các phần thân cũ nếu có
        foreach (GameObject body in bodyParts.ToList())
        {
            if (body != null) Destroy(body);
        }
        bodyParts.Clear();
        positionHistory.Clear();

        // Load và áp dụng tổ hợp material riêng cho rắn player
        LoadAndApplyMaterials();

        // Tạo các phần thân ban đầu với màu sắc so le
        for (int i = 0; i < 2; i++)
        {
            Grow();
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.enabled = false;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (scoreInGame != null)
        {
            scoreInGame.text = score.ToString();
        }
        if (scoreTextGameOver != null)
        {
            scoreTextGameOver.text = "Score: " + score;
        }
    }

    protected override void Move()
    {
        if (head != null)
        {
            head.transform.position += head.transform.forward * moveSpeed * Time.deltaTime;
            float steerDirection = Input.GetAxis("Horizontal");
            head.transform.Rotate(Vector3.up * steerDirection * steerSpeed * Time.deltaTime);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag("Food") && other.gameObject != null)
        {
            if (foodSpawner != null)
            {
                foodSpawner.RespawnFood(other.gameObject);
            }
            else
            {
                Debug.LogWarning("Khong tim thay FoodSpawner");
            }
            Grow();
            score += 1;
        }
        else if (other != null && other.CompareTag("SnakeBody") && other.gameObject != head)
        {
            if (!bodyParts.Contains(other.gameObject))
            {
                Die();
            }
        }
    }

    public override void Die()
    {
        foreach (GameObject body in bodyParts.ToList())
        {
            if (body != null)
            {
                Destroy(body);
            }
        }
        /*if (foodSpawner != null)
        {
            foodSpawner.CreateFoodFromBodyParts(bodyParts);
        }*/
        bodyParts.Clear();
        positionHistory.Clear();
        if (gameOverCanvas != null)
        {
            gameOverCanvas.enabled = true;
        }
        Time.timeScale = 0f;

        GameController gameController = FindFirstObjectByType<GameController>();
        if (gameController != null)
        {
            gameController.GameOver();
        }
    }

    public void RestartPlayer()
    {
        foreach (GameObject body in bodyParts.ToList())
        {
            if (body != null) Destroy(body);
        }
        bodyParts.Clear();
        positionHistory.Clear();

        if (head != null)
        {
            head.transform.position = new Vector3(5, 0.5f, 5);
            head.transform.rotation = Quaternion.identity;
        }
        score = 0;
        ApplyScale(baseScale); // Reset scale về baseScale
        gap = 20; // Reset gap về giá trị ban đầu

        // Load và áp dụng tổ hợp material riêng cho rắn player
        LoadAndApplyMaterials();

        // Tạo các phần thân ban đầu với màu sắc so le
        for (int i = 0; i < 2; i++)
        {
            Grow();
        }
        LoadAndApplyMaterials(); // Đảm bảo áp dụng lại sau khi grow
    }

    public new void Grow()
    {
        GameObject body = Instantiate(bodyPrefab);
        body.transform.position = bodyParts.Count > 0 ? bodyParts[bodyParts.Count - 1].transform.position : head.transform.position;
        body.tag = "SnakeBody";

        // Áp dụng material cho phần thân mới theo thứ tự trong materialSequence, với quy tắc đặc biệt cho body 1
        Renderer bodyRenderer = body.GetComponent<Renderer>();
        if (bodyRenderer != null && materialSequence.Count > 0)
        {
            if (bodyParts.Count == 0) // Body 1 (phần thân đầu tiên)
            {
                if (materialSequence.Count >= 2)
                {
                    bodyRenderer.material = materialSequence[1]; // Dùng màu thứ hai nếu danh sách có ít nhất 2 màu
                }
                else
                {
                    bodyRenderer.material = materialSequence[0]; // Dùng màu đầu tiên nếu chỉ có 1 màu
                }
            }
            else // Các body tiếp theo (body 2, 3,...)
            {
                int materialIndex = (bodyParts.Count + 1) % materialSequence.Count; // Bắt đầu từ màu thứ ba nếu có
                bodyRenderer.material = materialSequence[materialIndex];
            }
        }

        // Áp dụng scale cho phần thân mới (được xử lý trong IncreaseSize())
        bodyParts.Add(body);

        IncreaseSize();
    }

    private void LoadAndApplyMaterials()
    {
        // Đọc chuỗi tổ hợp material từ PlayerPrefs
        string materialSequenceString = PlayerPrefs.GetString("PlayerMaterialSequence", "");
        materialSequence.Clear();

        if (!string.IsNullOrEmpty(materialSequenceString))
        {
            string[] materialNames = materialSequenceString.Split(',');
            foreach (string materialName in materialNames)
            {
                Material material = Resources.Load<Material>("Materials/" + materialName);
                if (material != null)
                {
                    materialSequence.Add(material);
                }
                else
                {
                    Debug.LogError($"Material {materialName} not found in Resources/Materials!");
                }
            }
        }
        else
        {
            Debug.LogWarning("No materials found in PlayerPrefs for PlayerMaterialSequence!");
            // Sử dụng material mặc định nếu danh sách rỗng
            Material defaultMaterial = Resources.Load<Material>("Materials/DefaultMaterial");
            if (defaultMaterial != null)
            {
                materialSequence.Add(defaultMaterial);
            }
            else
            {
                Debug.LogError("DefaultMaterial not found in Resources/Materials!");
            }
        }

        // Áp dụng material cho head và bodyParts theo thứ tự, với quy tắc đặc biệt cho body 1
        if (materialSequence.Count > 0)
        {
            // Áp dụng cho head (luôn dùng màu đầu tiên)
            if (headRenderer != null)
            {
                headRenderer.material = materialSequence[0]; // Head dùng material đầu tiên
            }

            // Áp dụng cho bodyParts, với quy tắc đặc biệt cho body 1
            for (int i = 0; i < bodyParts.Count; i++)
            {
                if (bodyParts[i] != null)
                {
                    Renderer bodyRenderer = bodyParts[i].GetComponent<Renderer>();
                    if (bodyRenderer != null)
                    {
                        if (i == 0 && materialSequence.Count >= 2) // Body 1 (phần thân đầu tiên)
                        {
                            bodyRenderer.material = materialSequence[1]; // Dùng màu thứ hai nếu có ít nhất 2 màu
                        }
                        else // Các body tiếp theo (body 2, 3,...)
                        {
                            int materialIndex = (i + 1) % materialSequence.Count; // Bắt đầu từ màu thứ ba nếu có
                            bodyRenderer.material = materialSequence[materialIndex];
                        }
                    }
                }
            }
        }
    }

    public string GetPlayerName()
    {
        return "YOU";
    }
}