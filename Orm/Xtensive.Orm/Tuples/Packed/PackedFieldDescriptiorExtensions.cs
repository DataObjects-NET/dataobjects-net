// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Tuples.Packed
{
  internal static class PackedFieldDescriptorExtensions
  {
    private const int OffsetBitCount = 6;
    private const int OffsetMask = (1 << OffsetBitCount) - 1;

    public static PackedFieldAccessor GetAccessor(in this PackedFieldDescriptor descriptor) => PackedFieldAccessor.All[descriptor.AccessorIndex];
    public static bool IsObjectField(in this PackedFieldDescriptor descriptor) => descriptor.GetAccessor().Rank < 0;
    public static int GetObjectIndex(in this PackedFieldDescriptor descriptor) => descriptor.DataPosition;
    public static int GetValueIndex(in this PackedFieldDescriptor descriptor) => descriptor.DataPosition >> OffsetBitCount;
    public static int GetValueBitOffset(in this PackedFieldDescriptor descriptor) => descriptor.DataPosition & OffsetMask;
    public static int GetStateIndex(in this PackedFieldDescriptor descriptor) => descriptor.StatePosition >> OffsetBitCount;
    public static int GetStateBitOffset(in this PackedFieldDescriptor descriptor) => descriptor.StatePosition & OffsetMask;
  }
}