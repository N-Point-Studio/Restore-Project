// using UnityEngine;

// public class ZoomableObject : MonoBehaviour, IRotate
// {
//     [SerializeField] private float rotateSpeed = 0.5f;

//     public void OnStart()
//     {
//         // Optional: bisa dipakai buat flag rotating
//     }

//     public void OnEnd()
//     {
//         // Optional: stop rotating
//     }

//     public void OnRotatePerformed(Vector2 delta)
//     {
//         float rotateY = -delta.x * rotateSpeed;
//         float rotateX = delta.y * rotateSpeed;

//         // Horizontal drag → rotate Y (world up)
//         transform.Rotate(Vector3.up, rotateY, Space.World);

//         // Vertical drag → rotate X (local right biar natural)
//         transform.Rotate(Vector3.right, rotateX, Space.Self);
//     }
// }