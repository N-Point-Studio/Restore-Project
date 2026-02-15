using UnityEngine;

public class ArtefactPieceInspectState : ArtefactPieceBaseState, IInteractable
{
    public ArtefactPieceInspectState(ArtefactPieceStateMachine stateMachine) : base(stateMachine) { }

    public void OnStart(Vector3 worldPos, Vector2 screenPos) { }

    public void OnMove(Vector3 worldPos, Vector2 screenPos)
    {
        // Kita hitung delta di mediator atau di sini menggunakan last position
        // Untuk sederhananya, asumsikan kita dapat delta atau posisi baru
        // Logika rotasimu yang lama pindah ke sini
        Debug.Log("Rotating in Inspect Mode");
    }

    public void OnHold()
    {
        // Sesuai idemu: Hold akan memicu benda balik ke meja
        stateMachine.ResetTransform();
        stateMachine.SwitchState(new ArtefactPieceIdleState(stateMachine));
        Debug.Log("Artefak dikembalikan ke meja via Hold");
    }

    public void OnZoom(float delta)
    {
        float newScale = stateMachine.transform.localScale.x + delta;
        newScale = Mathf.Clamp(newScale, stateMachine.minScale, stateMachine.maxScale);
        stateMachine.transform.localScale = Vector3.one * newScale;
    }

    public void OnEnd(Vector3 worldPos, Vector2 screenPos) { }

    public override void Enter()
    {
        throw new System.NotImplementedException();
    }

    public override void Tick(float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    public override void Exit()
    {
        throw new System.NotImplementedException();
    }
}