using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // Tham chiếu đến đầu rắn
    public Vector3 baseOffset = new Vector3(0, 10, -10); // Offset ban đầu
    public float smoothSpeed = 0.125f;
    private Snake snake; // Tham chiếu đến Snake để lấy scale

    void Start()
    {
        snake = player.GetComponent<Snake>();
        if (snake == null)
        {
            Debug.LogError("CameraFollow: Player không có Snake Player Controller Componet");
        }
    }

    void LateUpdate()
    {
        if (player == null || snake == null) return;

        // Điều chỉnh offset dựa trên scale của rắn
        float currentScale = player.localScale.x;

        Vector3 adjustedOffset = baseOffset * Mathf.Lerp(1f, 1.5f, (currentScale - 1f) / 1f); // Tăng offset tối đa 50% khi scale từ 1 đến 2

        Vector3 desiredPosition = player.position + adjustedOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}