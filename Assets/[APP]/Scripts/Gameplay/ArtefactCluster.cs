using System.Collections.Generic;

public class ArtefactCluster
{
    private readonly List<IAssemblable> pieces = new();
    public List<IAssemblable> Pieces => pieces;

    public void Add(IAssemblable piece)
    {
        if (pieces.Contains(piece)) return;
        pieces.Add(piece);
        piece.SetCluster(this);
    }

    public void Remove(IAssemblable piece)
    {
        if (!pieces.Contains(piece)) return;
        pieces.Remove(piece);
        piece.SetCluster(null);
    }
}