using System.Collections.Generic;
using UnityEngine;

public class ArtefactClusterStateMachine : StateMachine, IClick, IDrag, IAssembled, IInspectable
{
    public string PieceId => throw new System.NotImplementedException();

    public bool IsInspected => throw new System.NotImplementedException();

    public void EnterInspect(Transform targetPosition)
    {
        throw new System.NotImplementedException();
    }

    public void ExitInspect()
    {
        throw new System.NotImplementedException();
    }

    public IAssembled GetAssembleParrent()
    {
        throw new System.NotImplementedException();
    }

    public ConnectionSocket GetAvailableSocketFor(string id)
    {
        throw new System.NotImplementedException();
    }

    public List<ConnectionSocket> GetSockets()
    {
        throw new System.NotImplementedException();
    }

    public Transform GetTransform()
    {
        throw new System.NotImplementedException();
    }

    public void OnAssembled(IAssembled parent, Transform transform)
    {
        throw new System.NotImplementedException();
    }

    public void OnClick()
    {
        throw new System.NotImplementedException();
    }

    public void OnDetached()
    {
        throw new System.NotImplementedException();
    }

    public void OnDragPerformed(Vector3 worldPos)
    {
        throw new System.NotImplementedException();
    }

    public void OnInteractEnd()
    {
        throw new System.NotImplementedException();
    }

    public void OnInteractStart()
    {
        throw new System.NotImplementedException();
    }

    public void ReleaseSocketWith(string otherId)
    {
        throw new System.NotImplementedException();
    }
}
