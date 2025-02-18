using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float steerSpeed = 180f;
    [SerializeField] protected float fixedHeight = 0.5f;

    // Thêm khai báo bodyPrefab ở đây
    [SerializeField] protected GameObject bodyPrefab; // Prefab của phần thân rắn

    protected List<GameObject> BodyParts = new List<GameObject>();
    protected List<Vector3> PositionHistory = new List<Vector3>();
    protected GameObject targetFood;

    // Tạo các phần thân rắn
    public virtual void GrowSnake()
    {
        // Kiểm tra nếu bodyPrefab chưa được gán
        if (bodyPrefab == null)
        {
            Debug.LogError("bodyPrefab chưa được gán cho Snake!");
            return;
        }

        GameObject body = Instantiate(bodyPrefab);
        body.transform.position = BodyParts.Count > 0 ? BodyParts[BodyParts.Count - 1].transform.position : transform.position;
        body.transform.parent = transform;
        BodyParts.Add(body);
    }

    // Di chuyển đầu rắn
    public virtual void MoveForward()
    {
        Vector3 newPosition = transform.position + transform.forward * moveSpeed * Time.deltaTime;
        newPosition.y = fixedHeight;
        transform.position = newPosition;
    }

    // Cập nhật lịch sử và phần thân
    public void UpdateBodyParts()
    {
        PositionHistory.Insert(0, transform.position);
        int index = 0;
        foreach (var body in BodyParts)
        {
            Vector3 point = PositionHistory[Mathf.Clamp(index * 2, 0, PositionHistory.Count - 1)];
            point.y = fixedHeight;
            body.transform.position = Vector3.Lerp(body.transform.position, point, moveSpeed * Time.deltaTime);
            body.transform.rotation = Quaternion.Slerp(body.transform.rotation, Quaternion.LookRotation(point - body.transform.position), steerSpeed * Time.deltaTime);
            index++;
        }
    }

    // Kiểm tra va chạm với thức ăn hoặc cơ thể
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Food"))
        {
            GrowSnake();
        }
    }
}
