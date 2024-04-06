using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    void Update()
    {
        // 确保我们不只是旋转方块的Y轴，也旋转X和Z轴
        transform.LookAt(Camera.main.transform);

        // 可选的，使其只在Y轴旋转，保持垂直方向不变
        // Vector3 targetPosition = new Vector3(Camera.main.transform.position.x,
        //                                      transform.position.y,
        //                                      Camera.main.transform.position.z);
        // transform.LookAt(targetPosition);
    }
}
