using System.Collections.Generic;

namespace Xtensive.Tuples.Packed
{

  internal struct PackingOrderInfo
  {
    public int FieldIndex;
    public int ValueBitCount;
  }

  internal class PackingOrderInfoComparer : IComparer<PackingOrderInfo>
  {
    public static readonly PackingOrderInfoComparer Instance =
      new PackingOrderInfoComparer();

    public int Compare(PackingOrderInfo x, PackingOrderInfo y)
    {
      var bitCountDiff = y.ValueBitCount - x.ValueBitCount;
      return bitCountDiff != 0
        ? bitCountDiff
        : x.FieldIndex - y.FieldIndex;
    }
  }
}
