// using UnityEngine;

// public class ArtefactPieceDragState : ArtefactPieceBaseState, IDraggable, IInspectable
// {
//     private bool isDragging;
//     private Transform pieceTransform;
//     private Vector3 gotoPos;

//     private Vector3 startWorldPos;

//     public ArtefactPieceDragState(
//         ArtefactPieceStateMachine stateMachine
//     ) : base(stateMachine)
//     {

//     }

//     public override void Enter()
//     {
//         Debug.Log("Drag state enter");

//         pieceTransform = stateMachine.Transform;

//         isDragging = true;
//         gotoPos = startWorldPos;

//         pieceTransform.position = startWorldPos;
//     }

//     public override void Tick(float deltaTime)
//     {
//         if (!isDragging) return;

//         pieceTransform.position = gotoPos;
//     }

//     public override void Exit()
//     {
//         isDragging = false;
//     }

//     public void OnDragStart(Vector3 worldPosition)
//     {
//         isDragging = true;
//         gotoPos = worldPosition;
//     }

//     public void OnDragPerformed(Vector3 worldPosition)
//     {
//         if (!isDragging) return;

//         gotoPos = worldPosition;
//     }

//     public void OnDragEnd(Vector3 worldPosition)
//     {
//         if (!isDragging) return;

//         stateMachine.ResetTransform();
//         isDragging = false;
//     }

//     public void EnterInspect()
//     {
//         stateMachine.SwitchState(new ArtefactPieceInspectState(stateMachine));
//     }

//     public void ExitInspect()
//     {
//     }
// }