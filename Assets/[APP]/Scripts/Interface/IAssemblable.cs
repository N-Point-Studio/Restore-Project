using UnityEngine;

public interface IAssemblable
{
    ArtefactCluster Cluster { get; }
    Transform Transform { get; }

    void SetCluster(ArtefactCluster cluster);
    void OnAssembled();
    void OnDisassembled();
}
