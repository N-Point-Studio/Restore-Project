using UnityEngine;

public interface IAssemblyService
{
    void OnAssembleDragging(Vector3 position);
    void TryAssemble(IAssemblable source, IAssemblable target);
    void Disassemble(IAssemblable piece);
}