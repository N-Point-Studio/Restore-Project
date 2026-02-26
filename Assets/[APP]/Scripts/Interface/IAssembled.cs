using UnityEngine;

public interface IAssembled
{
    string PieceId { get; }
    Transform GetTransform();
    IAssembled GetAssembleParrent();
    ConnectionSocket GetAvailableSocketFor(string id);
    void ReleaseSocketWith(string otherId);
    void OnAssembled(IAssembled parent, Transform transform);
    void OnDetached();
}
