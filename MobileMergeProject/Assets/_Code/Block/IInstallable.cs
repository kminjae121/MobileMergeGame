using System.Collections.Generic;
using UnityEngine;

namespace _Code.Block
{
    public interface IInstallable
    {
        IReadOnlyList<Vector2Int> Cells { get; }
        int CellCount { get; }
    }
}
