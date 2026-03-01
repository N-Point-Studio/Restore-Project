// using UnityEngine;

// public class ToolObject : MonoBehaviour, IDrag
// {
//     [SerializeField] private Transform GrabPosition;
//     [SerializeField] private SurfaceDetection surface = null;

//     private Camera mainCamera;
//     private Vector3 initialPosition;
//     private Collider colliderComponent;

//     private bool isDragging;
//     private bool isReturning;

//     private void Awake()
//     {
//         mainCamera = Camera.main;
//         initialPosition = transform.position;
//         colliderComponent = GetComponent<Collider>();
//     }

//     private void Update()
//     {
//         // Smooth return ke posisi awal
//         if (isReturning)
//         {
//             MoveBack(10f);

//             if (Vector3.Distance(transform.position, initialPosition) < 0.01f)
//             {
//                 isReturning = false;
//                 colliderComponent.enabled = true;

//                 if (surface != null)
//                 {
//                     surface.isUsed = false;
//                     surface.DisableDetection();
//                 }
//             }
//         }
//     }

//     // =============================
//     // IDrag Implementation
//     // =============================

//     public void OnStart()
//     {
//         isDragging = true;
//         isReturning = false;

//         if (surface != null)
//             surface.isUsed = true;
//     }

//     public void OnDragPerformed(Vector3 worldPos)
//     {
//         if (!isDragging) return;

//         // Kita mau GrabPosition tepat di worldPos
//         Vector3 desiredGrabPos = new Vector3(
//             worldPos.x,
//             worldPos.y,
//             GrabPosition.position.z
//         );

//         // Hitung offset dari root ke grab
//         Vector3 offset = transform.position - GrabPosition.position;

//         // Posisi root = posisi jari + offset
//         Vector3 targetRootPos = desiredGrabPos + offset;

//         // Tetap kunci di Z awal
//         targetRootPos.z = initialPosition.z;

//         transform.position = Vector3.Lerp(
//             transform.position,
//             targetRootPos,
//             Time.deltaTime * 15f
//         );

//         if (surface != null)
//         {
//             Vector2 screenPos = mainCamera.WorldToScreenPoint(GrabPosition.position);
//             surface.Detect(screenPos);
//         }
//     }

//     public void OnEnd()
//     {
//         if (!isDragging) return;

//         isDragging = false;
//         isReturning = true;

//         colliderComponent.enabled = false;
//     }

//     // =============================
//     // Movement Helpers
//     // =============================

//     private void MoveBack(float speed)
//     {
//         transform.position = Vector3.Lerp(
//             transform.position,
//             initialPosition,
//             Time.deltaTime * speed
//         );
//     }
// }