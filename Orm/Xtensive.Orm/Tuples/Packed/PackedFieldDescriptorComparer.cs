using System.Collections.Generic;

namespace Xtensive.Tuples.Packed
{
  internal class PackedFieldDescriptorComparer : IComparer<PackedFieldDescriptor>
  {
    public static readonly PackedFieldDescriptorComparer Instance = 
      new PackedFieldDescriptorComparer();

    public int Compare(PackedFieldDescriptor x, PackedFieldDescriptor y)
    {
      var t = y.ValueBitCount - x.ValueBitCount;
      if (t != 0)
        return t;
      return x.FieldIndex - y.FieldIndex;
    }
  }
}
