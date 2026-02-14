using Unity.VisualScripting;
using UnityEngine;

public class AssemblyService : IAssemblyService, IInitializable
{
    private float snapDistance = 0.5f;

    public void TryAssemble(IAssemblable source, IAssemblable target)
    {
        if (source == target) return;

        float distance = Vector3.Distance(
            source.Transform.position,
            target.Transform.position
        );

        if (distance > snapDistance) return;

        MergeClusters(source, target);

        source.OnAssembled();
        target.OnAssembled();

        Snap(source, target);
    }

    private void MergeClusters(IAssemblable a, IAssemblable b)
    {
        var clusterA = a.Cluster;
        var clusterB = b.Cluster;

        if (clusterA == null && clusterB == null)
        {
            var newCluster = new ArtefactCluster();
            newCluster.Add(a);
            newCluster.Add(b);
        }
        else if (clusterA != null && clusterB == null)
        {
            clusterA.Add(b);
        }
        else if (clusterA == null && clusterB != null)
        {
            clusterB.Add(a);
        }
        else if (clusterA != clusterB)
        {
            foreach (var piece in clusterB.Pieces)
                clusterA.Add(piece);
        }
    }

    private void Snap(IAssemblable source, IAssemblable target)
    {
        source.Transform.position = target.Transform.position;
    }

    public void Disassemble(IAssemblable piece)
    {
        piece.Cluster?.Remove(piece);
        piece.OnDisassembled();
    }

    public void OnAssembleDragging(Vector3 position)
    {
        Debug.Log("Ada kok");
    }

    public void Initialize()
    {
        throw new System.NotImplementedException();
    }
}