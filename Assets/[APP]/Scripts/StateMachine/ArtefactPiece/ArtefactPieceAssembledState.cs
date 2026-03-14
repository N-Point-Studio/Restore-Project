using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ArtefactPieceAssembledState : ArtefactPieceBaseState, IArtefactPart
{

    public string PieceId => stateMachine.pieceId;
    public Transform GetTransform() => stateMachine.transform;
    public ArtefactPieceAssembledState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.state = ArtefactPieceState.Assembled;
        stateMachine.transform.DOPunchRotation(new Vector3(5, 5, 0), 0.4f);
        InteractionEvents.OnAssembleInteractionFinished?.Invoke();
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }

    public ConnectionSocket GetAvailableSocketFor(string id) => stateMachine.GetAvailableSocketFor(id);

    public void ReleaseSocketWith(string otherId) => stateMachine.ReleaseSocketWith(otherId);

    public List<ConnectionSocket> GetSockets() => stateMachine.sockets;

    public void OnAssembled(Transform targetTransform)
    {

    }

    public void OnDetached()
    {
        stateMachine.SwitchState(new ArtefactPieceReturningState(stateMachine));
    }

    public Renderer GetRenderer() => stateMachine.GetRenderer();
}
