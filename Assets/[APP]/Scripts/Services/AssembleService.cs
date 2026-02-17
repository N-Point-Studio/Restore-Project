using System;
using System.Collections.Generic;
using UnityEngine;
public interface IAssemblable
{
    string PieceId { get; }
    Transform Transform { get; }

    AssemblyGroup CurrentGroup { get; set; }

    bool CanAssembleWith(IAssemblable other);
    Transform GetSnapPoint(string otherPieceId);
}


[Serializable]
public class AssemblyRule
{
    public string pieceA;
    public string pieceB;
}

public class AssembleService
{
    private List<AssemblyRule> rules;
    private const float SnapDistance = 0.5f;

    public AssembleService(List<AssemblyRule> rules)
    {
        this.rules = rules;
    }

    public void TryAssemble(IAssemblable a, IAssemblable b)
    {
        if (a == null || b == null) return;
        if (!IsValidRule(a.PieceId, b.PieceId)) return;

        float distance = Vector3.Distance(a.Transform.position, b.Transform.position);
        if (distance > SnapDistance) return;

        SnapPieces(a, b);
    }

    private bool IsValidRule(string idA, string idB)
    {
        foreach (var rule in rules)
        {
            if ((rule.pieceA == idA && rule.pieceB == idB) ||
                (rule.pieceA == idB && rule.pieceB == idA))
                return true;
        }
        return false;
    }

    private void SnapPieces(IAssemblable a, IAssemblable b)
    {
        Transform snapPoint = a.GetSnapPoint(b.PieceId);
        if (snapPoint == null) return;

        b.Transform.position = snapPoint.position;
        b.Transform.rotation = snapPoint.rotation;

        MergeGroups(a, b);
    }

    private void MergeGroups(IAssemblable a, IAssemblable b)
    {
        if (a.CurrentGroup == null && b.CurrentGroup == null)
        {
            GameObject rootObj = new GameObject("AssemblyGroup");
            AssemblyGroup newGroup = new AssemblyGroup(rootObj.transform);

            newGroup.AddMember(a);
            newGroup.AddMember(b);
        }
        else if (a.CurrentGroup != null && b.CurrentGroup == null)
        {
            a.CurrentGroup.AddMember(b);
        }
        else if (a.CurrentGroup == null && b.CurrentGroup != null)
        {
            b.CurrentGroup.AddMember(a);
        }
        else if (a.CurrentGroup != b.CurrentGroup)
        {
            a.CurrentGroup.Merge(b.CurrentGroup);
        }
    }
}