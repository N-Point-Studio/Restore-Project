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
        stateMachine.transform.DOPunchRotation(stateMachine.punchRotation, stateMachine.punchDuration);
        AudioEvents.TriggerPlayAssembleSFX();
        AssembleEvents.OnAssembleFinished?.Invoke();
    }
    public override void Tick(float deltaTime) { }
    public override void Exit() { }

    public ArtefactPieceState CurrentState => stateMachine.state;

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

    public void CorrectRotation(Quaternion rotation) => stateMachine.CorrectRotation(rotation);
}
