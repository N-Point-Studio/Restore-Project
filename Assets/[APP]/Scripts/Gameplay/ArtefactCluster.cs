using System.Collections.Generic;
using UnityEngine;

public class AssemblyGroup
{
    public Transform Root { get; private set; }
    private List<IAssemblable> members = new();

    public AssemblyGroup(Transform root)
    {
        Root = root;
    }

    public void AddMember(IAssemblable piece)
    {
        members.Add(piece);
        piece.CurrentGroup = this;
        piece.Transform.SetParent(Root);
    }

    public void Merge(AssemblyGroup otherGroup)
    {
        foreach (var member in otherGroup.members)
        {
            AddMember(member);
        }
    }
}