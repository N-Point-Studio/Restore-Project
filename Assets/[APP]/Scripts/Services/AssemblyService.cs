using System.Linq;
using UnityEngine;

public interface IAssemblable
{
    ArtefactCluster Cluster { get; }
    Transform Transform { get; }
    void OnAssembled();
    void OnDisassembled();
}

public interface IAssemblyService
{
    bool TryAssemble(IAssemblable a, IAssemblable b);
    void Disassemble(IAssemblable piece);
}

public class AssemblyService : IAssemblyService
{
    public bool TryAssemble(IAssemblable a, IAssemblable b)
    {
        if (a.Cluster == b.Cluster)
            return false;

        a.Cluster.Merge(b.Cluster);

        foreach (var member in a.Cluster.Members)
            member.OnAssembled();

        return true;
    }

    public void Disassemble(IAssemblable piece)
    {
        piece.Cluster.Remove(piece);

        piece.OnDisassembled();
    }
}