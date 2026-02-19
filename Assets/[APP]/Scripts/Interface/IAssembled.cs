using UnityEngine;

public interface IAssembled
{
    void OnAssembled(ArtefactPieceStateMachine parent);
    void OnDetached();
}
