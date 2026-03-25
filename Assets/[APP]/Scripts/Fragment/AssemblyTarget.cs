[System.Serializable]
public class AssemblyTarget
{
    public AssemblyTarget(ArtefactPieceStateMachine frag, CustomTransform transform)
    {
        this.targetFragment = frag;
        this.correctPosition = transform;
    }
    public ArtefactPieceStateMachine targetFragment;
    public CustomTransform correctPosition;
}
