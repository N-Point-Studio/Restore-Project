using System.Collections.Generic;
using UnityEngine;

public class ArtefactClusterStateMachine : StateMachine, IInteractObject, IInspectable, IDragObject, ICluster
{
    private List<IAssemble> children = new List<IAssemble>();
    public IReadOnlyList<IAssemble> Children => children;
    private IAssemble parent;

    public Transform GetTransform() => transform;
    public IAssemble GetAssembleParent() => parent;

    public IAssemble GetRoot()
    {
        return parent == null ? this : parent.GetRoot();
    }

    public void OnAssembled(IAssemble newParent, Transform targetTransform)
    {
        this.parent = newParent;
    }

    public void OnDetached()
    {
        this.parent = null;
    }

    public void AddChild(IAssemble child)
    {
        if (!children.Contains(child))
        {
            children.Add(child);

            // 1. Pindahkan posisi hierarchy di Unity! (PENTING)
            // Parameter 'true' menjaga agar posisi 3D di layar tidak bergeser saat dipindah.
            child.GetTransform().SetParent(this.transform, true);

            // 2. Beri tahu si anak bahwa dia sekarang punya parent baru
            child.OnAssembled(this, this.transform);
        }
    }

    public void RemoveChild(IAssemble child)
    {
        if (children.Contains(child))
        {
            children.Remove(child);

            // 1. Keluarkan dari wadah hierarchy Unity
            child.GetTransform().SetParent(null, true);

            // 2. Beri tahu si anak bahwa dia sudah mandiri (lepas)
            child.OnDetached();
        }
    }

    public void CalculateCenter()
    {
        // throw new System.NotImplementedException();
    }

    public void EnterInspect(Transform targetPosition)
    {
        // throw new System.NotImplementedException();
    }

    public void ExitInspect()
    {
        // throw new System.NotImplementedException();
    }

    public void OnDragEnded(Vector3 worldPos)
    {
        // throw new System.NotImplementedException();
    }

    public void OnDragPerformed(Vector3 worldPos)
    {
        // throw new System.NotImplementedException();
    }

    public void OnDragStarted(Vector3 worldPos)
    {
        // throw new System.NotImplementedException();
    }

    public void OnInteractDetected()
    {
        // throw new System.NotImplementedException();
    }

    public void OnInteractEnded()
    {
        // throw new System.NotImplementedException();
    }

}
