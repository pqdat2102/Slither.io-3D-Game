using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelection1 : MonoBehaviour
{
    public GameObject[] characters; // Danh sách màu của rắn hiển thị trên scene Select Color
    public Material[] playerMaterials; // Danh sách material trùng với màu của rắn
    private int selectedIndex = 0;

    public void NextCharacter()
    {
        characters[selectedIndex].SetActive(false);
        selectedIndex = (selectedIndex + 1) % characters.Length;
        characters[selectedIndex].SetActive(true);
    }

    public void PreviousCharacter()
    {
        characters[selectedIndex].SetActive(false);
        selectedIndex--;
        if (selectedIndex < 0)
        {
            selectedIndex += characters.Length;
        }
        characters[selectedIndex].SetActive(true);
    }

    public void StartGame()
    {
        // Lưu tên material đã chọn vào PlayerPrefs
        string materialName = playerMaterials[selectedIndex].name;
        PlayerPrefs.SetString("PlayerMaterialName", materialName);
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    private void Start()
    {
        // Đảm bảo chỉ một nhân vật được kích hoạt ban đầu
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(i == selectedIndex);
        }
    }
}