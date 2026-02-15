// using UnityEngine;

// public class RotateObject : MonoBehaviour, IRotateable
// {
//     [SerializeField] private float rotateSpeed = 0.2f;
//     private Vector3 currentRotationVelocity;
//     public void OnRotateStarted()
//     {
//         currentRotationVelocity = Vector3.zero;
//     }

//     public void OnRotatePerformed(Vector2 delta)
//     {
//         float rotateY = -delta.x * rotateSpeed;
//         float rotateX = delta.y * rotateSpeed;
//         transform.Rotate(Vector3.up, rotateY, Space.World);
//         transform.Rotate(Vector3.right, rotateX, Space.World);
//     }

//     public void OnRotateEnd()
//     {
//     }
// }
