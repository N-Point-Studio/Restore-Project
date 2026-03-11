using System.Collections.Generic;
using UnityEngine;

public interface IAssembled
{
    string PieceId { get; }
    Transform GetTransform();
    IAssembled GetAssembleParrent();
    ConnectionSocket GetAvailableSocketFor(string id);
    List<ConnectionSocket> GetSockets();
    void ReleaseSocketWith(string otherId);
    void OnAssembled(IAssembled parent, Transform transform);
    void OnDetached();
}

// 1. Interface Dasar (Berlaku untuk Part TUNGGAL maupun CLUSTER)
public interface IAssemble
{
    Transform GetTransform();
    IAssemble GetAssembleParent(); // (Btw, typo kecil di kodemu: Parrent -> Parent)
    void OnAssembled(IAssemble parent, Transform targetTransform);
    void OnDetached();
    IAssemble GetRoot();
}

// 2. Interface Khusus Part Tunggal (Turunan dari IAssembled)
public interface IArtefactPart : IAssemble
{
    string PieceId { get; }
    List<ConnectionSocket> GetSockets();
    ConnectionSocket GetAvailableSocketFor(string id);
    void ReleaseSocketWith(string otherId);
}

public interface ICluster : IAssemble
{
    // Menyimpan daftar semua part yang ada di dalam grup/cluster ini
    IReadOnlyList<IAssemble> Children { get; }

    // Memasukkan part ke dalam cluster
    void AddChild(IAssemble child);

    // Mengeluarkan part dari cluster
    void RemoveChild(IAssemble child);

    void CalculateCenter();
}