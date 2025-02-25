using UnityEngine;

public class LoadCharacter : MonoBehaviour
{
    public GameObject[] characterPrefabs;
    public Transform spawnPoint;
    public CameraFollow cameraFollow;
    public GameController gameController; // Tham chiếu đến GameController

    void Start()
    {
        int selectedCharacter = PlayerPrefs.GetInt("selectedCharacter");
        GameObject prefab = characterPrefabs[selectedCharacter];

        // Tạo instance của prefab tại spawnPoint
        GameObject playerInstance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // Gán transform của rắn này cho camera
        if (cameraFollow != null)
        {
            cameraFollow.player = playerInstance.transform;
        }

        // Gán SnakePlayerController cho GameController
        if (gameController != null)
        {
            gameController.playerController = playerInstance.GetComponent<SnakePlayerController>();
            if (gameController.playerController != null)
            {
                gameController.playerController.RestartPlayer();
            }
        }
    }
}