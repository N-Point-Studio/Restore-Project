using System.Collections.Generic;

public class ArtefactCluster
{
    public List<IAssemblable> Members = new();

    public void Add(IAssemblable piece)
    {
        if (Members.Contains(piece)) return;

        Members.Add(piece);
        // piece.Cluster = this;
    }

    public void Merge(ArtefactCluster other)
    {
        foreach (var m in other.Members)
            Add(m);
    }

    public void Remove(IAssemblable piece)
    {
        Members.Remove(piece);

        var newCluster = new ArtefactCluster();
        newCluster.Add(piece);
    }
}