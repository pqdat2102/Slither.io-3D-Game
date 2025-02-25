using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CharacterSelection : MonoBehaviour
{
    public GameObject[] characters; // Danh sách prefab rắn (Snake 1, Snake 2, Snake 3,...), kéo từ Inspector
    public CharacterMaterials[] characterMaterials; // Danh sách material cho từng character, kéo từ Inspector

    private int selectedIndex = 0; // Chỉ số character hiện tại

    private void Start()
    {
        // Đảm bảo selectedIndex hợp lệ
        if (characters.Length > 0)
        {
            selectedIndex = 0; // Chọn character đầu tiên mặc định
        }

        // Kiểm tra và đồng bộ hóa characterMaterials với characters
        if (characterMaterials == null || characterMaterials.Length != characters.Length)
        {
            characterMaterials = new CharacterMaterials[characters.Length];
            for (int i = 0; i < characters.Length; i++)
            {
                characterMaterials[i] = new CharacterMaterials();
            }
        }

        // Hiển thị character hiện tại (prefab) trên scene
        UpdateCharacterDisplay();
    }

    // Chuyển đến character tiếp theo (dựa trên prefab trong characters)
    public void NextCharacter()
    {
        if (characters.Length > 0)
        {
            selectedIndex = (selectedIndex + 1) % characters.Length;
            UpdateCharacterDisplay();
            Debug.Log($"Selected Character: {characters[selectedIndex].name}");
        }
    }

    // Chuyển đến character trước đó (dựa trên prefab trong characters)
    public void PreviousCharacter()
    {
        if (characters.Length > 0)
        {
            selectedIndex--;
            if (selectedIndex < 0)
            {
                selectedIndex += characters.Length;
            }
            UpdateCharacterDisplay();
            Debug.Log($"Selected Character: {characters[selectedIndex].name}");
        }
    }

    // Cập nhật hiển thị prefab của character trên scene
    private void UpdateCharacterDisplay()
    {
        if (characters.Length > 0 && selectedIndex >= 0 && selectedIndex < characters.Length)
        {
            // Tắt tất cả prefab trong characters
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] != null)
                {
                    characters[i].SetActive(false);
                }
            }
            // Bật prefab tương ứng với selectedIndex
            if (characters[selectedIndex] != null)
            {
                characters[selectedIndex].SetActive(true);
            }
        }
    }

    // Bắt đầu game với character hiện tại
    public void StartGame()
    {
        if (selectedIndex >= 0 && selectedIndex < characters.Length)
        {
            // Tạo chuỗi tổ hợp material cho character hiện tại
            string materialSequence = "";
            for (int i = 0; i < characterMaterials[selectedIndex].materials.Count; i++)
            {
                if (characterMaterials[selectedIndex].materials[i] != null)
                {
                    materialSequence += characterMaterials[selectedIndex].materials[i].name;
                    if (i < characterMaterials[selectedIndex].materials.Count - 1) materialSequence += ","; // Thêm dấu phẩy để phân tách
                }
            }

            // Lưu chuỗi tổ hợp material vào PlayerPrefs cho rắn hiện tại (giả sử là player)
            PlayerPrefs.SetString("PlayerMaterialSequence", materialSequence);
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("No character selected to start the game!");
        }
    }
}


[System.Serializable]
public class CharacterMaterials
{
    public List<Material> materials = new List<Material>(); // Danh sách material cho từng character, thêm bằng dấu "+" trong Inspector
}
