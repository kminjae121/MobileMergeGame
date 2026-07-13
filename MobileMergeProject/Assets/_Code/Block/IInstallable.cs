using System.Collections.Generic;
using UnityEngine;

namespace _Code.Block
{
    public interface IInstallable
    {
        IReadOnlyList<Vector2Int> Cells { get; }
        Color BlockColor { get; }
        int CellCount { get; }
    }
}
