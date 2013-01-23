// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.31

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Resources;
using Xtensive.Tuples.Packed;
using Xtensive.Tuples.Transform;

namespace Xtensive.Tuples
{
  /// <summary>
  /// Various extension methods for <see cref="Tuple"/> and <see cref="Tuple"/> types.
  /// </summary>
  public static class TupleExtensions
  {
    #region Copy methods

    /// <summary>
    /// Copies a range of elements from <paramref name="source"/> <see cref="Tuple"/> 
    /// starting at the specified source index 
    /// and pastes them to <paramref name="target"/> <see cref="Tuple"/> 
    /// starting at the specified target index. 
    /// </summary>
    /// <param name="source">Source tuple to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="startIndex">The index in the <paramref name="source"/> tuple at which copying begins.</param>
    /// <param name="targetStartIndex">The index in the <paramref name="target"/> tuple at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    public static void CopyTo(this Tuple source, Tuple target, int startIndex, int targetStartIndex, int length)
    {
      var packedSource = source as PackedTuple;
      var packedTarget = target as PackedTuple;

      if (packedSource!=null && packedTarget!=null)
        PartiallyCopyTupleFast(packedSource, packedTarget, startIndex, targetStartIndex, length);
      else
        PartiallyCopyTupleSlow(source, target, startIndex, targetStartIndex, length);
    }

    /// <summary>
    /// Copies a range of elements from <paramref name="source"/> <see cref="Tuple"/> 
    /// starting at the specified source index 
    /// and pastes them to <paramref name="target"/> <see cref="Tuple"/> 
    /// starting at the first element. 
    /// </summary>
    /// <param name="source">Source tuple to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="startIndex">The index in the <paramref name="source"/> tuple at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    public static void CopyTo(this Tuple source, Tuple target, int startIndex, int length)
    {
      source.CopyTo(target, startIndex, 0, length);
    }

    /// <summary>
    /// Copies a range of elements from <paramref name="source"/> <see cref="Tuple"/> 
    /// starting at the <paramref name="startIndex"/>
    /// and pastes them into <paramref name="target"/> <see cref="Tuple"/> 
    /// starting at the first element. 
    /// </summary>
    /// <param name="source">Source tuple to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="startIndex">The index in the <paramref name="source"/> tuple at which copying begins.</param>
    public static void CopyTo(this Tuple source, Tuple target, int startIndex)
    {
      source.CopyTo(target, startIndex, 0, source.Count - startIndex);
    }

    /// <summary>
    /// Copies all the elements from <paramref name="source"/> <see cref="Tuple"/> 
    /// starting at the first element
    /// and pastes them into <paramref name="target"/> <see cref="Tuple"/> 
    /// starting at the first element.
    /// </summary>
    /// <param name="source">Source tuple to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    public static void CopyTo(this Tuple source, Tuple target)
    {
      source.CopyTo(target, 0, 0, source.Count);
    }

    /// <summary>
    /// Copies a set of elements from <paramref name="source"/> <see cref="Tuple"/> 
    /// to <paramref name="target"/> <see cref="Tuple"/> using 
    /// specified target-to-source field index <paramref name="map"/>.
    /// </summary>
    /// <param name="source">Source tuple to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="map">Target-to-source field index map.
    /// Negative value in this map means "skip this element".</param>
    public static void CopyTo(this Tuple source, Tuple target, int[] map)
    {
      var packedSource = source as PackedTuple;
      var packedTarget = target as PackedTuple;

      if (packedSource!=null && packedTarget!=null)
        CopyTupleWithMappingFast(packedSource, packedTarget, map);
      else
        CopyTupleWithMappingSlow(source, target, map);
    }

    /// <summary>
    /// Copies a set of elements from <paramref name="sources"/> <see cref="Tuple"/>s
    /// to <paramref name="target"/> <see cref="Tuple"/> using 
    /// specified target-to-source field index <paramref name="map"/>.
    /// </summary>
    /// <param name="sources">Source tuples to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="map">Target-to-source field index map.
    /// Negative value in this map means "skip this element".</param>
    public static void CopyTo(this Tuple[] sources, Tuple target, Pair<int, int>[] map)
    {
      var haveSlowSource = false;
      var packedSources = new PackedTuple[sources.Length];

      for (int i = 0; i < sources.Length; i++) {
        var packedSource = sources[i] as PackedTuple;
        if (packedSource==null) {
          haveSlowSource = true;
          break;
        }
        packedSources[i] = packedSource;
      }

      if (!haveSlowSource) {
        var packedTarget = target as PackedTuple;
        if (packedTarget!=null) {
          CopyTupleArrayWithMappingFast(packedSources, packedTarget, map);
          return;
        }
      }

      CopyTupleArrayWithMappingSlow(sources, target, map);
    }

    /// <summary>
    /// Copies a set of elements from <paramref name="sources"/> <see cref="Tuple"/>s
    /// to <paramref name="target"/> <see cref="Tuple"/> using 
    /// specified target-to-source field index <paramref name="map"/>.
    /// </summary>
    /// <param name="sources">Source tuples to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="map">Target-to-source field index map.
    /// Negative value in this map means "skip this element".</param>
    public static void CopyTo(this FixedList3<Tuple> sources, Tuple target, Pair<int, int>[] map)
    {
      var haveSlowSource = false;
      var packedSources = new FixedList3<PackedTuple>();

      for (int i = 0; i < sources.Count; i++) {
        var packedSource = sources[i] as PackedTuple;
        if (packedSource==null) {
          haveSlowSource = true;
          break;
        }
        packedSources.Push(packedSource);
      }

      if (!haveSlowSource) {
        var packedTarget = target as PackedTuple;
        if (packedTarget!=null) {
          Copy3TuplesWithMappingFast(packedSources, packedTarget, map);
          return;
        }
      }

      Copy3TuplesWithMappingSlow(sources, target, map);
    }

    #endregion

    #region Transforms

    /// <summary>
    /// Combines the <paramref name="left"/> with <paramref name="right"/>.
    /// </summary>
    /// <param name="left">The first <see cref="Tuple"/> to combine.</param>
    /// <param name="right">The second <see cref="Tuple"/> to combine.</param>
    /// <returns></returns>
    public static Tuple Combine(this Tuple left, Tuple right)
    {
      var transform = new CombineTransform(false, new[] {left.Descriptor, right.Descriptor});
      return transform.Apply(TupleTransformType.TransformedTuple, left, right);
    }

    /// <summary>
    /// Cuts out <paramref name="segment"/> from <paramref name="tuple"/> <see cref="Tuple"/>.
    /// </summary>
    /// <param name="tuple">The <see cref="Tuple"/> to get segment from.</param>
    /// <param name="segment">The <see cref="Segment{T}"/> to cut off.</param>
    /// <returns></returns>
    public static Tuple GetSegment(this Tuple tuple, Segment<int> segment)
    {
      var map = new int[segment.Length];
      for (int i = 0; i < segment.Length; i++)
        map[i] = segment.Offset + i;

      var types = new ArraySegment<Type>(tuple.Descriptor.FieldTypes, segment.Offset, segment.Length);
      var descriptor = TupleDescriptor.Create(types.AsEnumerable());
      var transform = new MapTransform(false, descriptor, map);
      return transform.Apply(TupleTransformType.TransformedTuple, tuple);
    }

    private static IEnumerable<T> AsEnumerable<T>(this ArraySegment<T> segment)
    {
      ArgumentValidator.EnsureArgumentNotNull(segment, "segment");
      int lastPosition = segment.Offset + segment.Count;
      for (int i = segment.Offset; i < lastPosition; i++)
        yield return segment.Array[i];
    }

    #endregion

    #region Merge methods

    /// <summary>
    /// Merges a range of fields from <paramref name="difference"/>
    /// <see cref="Tuple"/> starting at the specified index with the fields from
    /// <paramref name="origin"/> <see cref="Tuple"/> with the specified
    /// <paramref name="behavior"/>.
    /// </summary>
    /// <param name="origin">Tuple containing original values and receiving the data.</param>
    /// <param name="difference">Tuple with differences to merge with.</param>
    /// <param name="startIndex">The index in the <paramref name="difference"/> tuple at which merging begins.</param>
    /// <param name="length">The number of elements to process.</param>
    /// <param name="behavior">The merge behavior that will be used to resolve conflicts when both values 
    /// from <paramref name="difference"/> and <paramref name="origin"/> are available.</param>
    /// <exception cref="ArgumentException">Tuple descriptors mismatch.</exception>
    public static void MergeWith(this Tuple origin, Tuple difference, int startIndex, int length, MergeBehavior behavior)
    {
      if (difference==null)
        return;
      if (origin.Descriptor!=difference.Descriptor)
        throw new ArgumentException(string.Format(Strings.ExInvalidTupleDescriptorExpectedDescriptorIs, origin.Descriptor), "difference");

      var packedOrigin = origin as PackedTuple;
      var packedDifference = difference as PackedTuple;
      var useFast = packedOrigin!=null && packedDifference!=null;

      switch (behavior) {
      case MergeBehavior.PreferOrigin:
        if (useFast)
          MergeTuplesPreferOriginFast(packedOrigin, packedDifference, startIndex, length);
        else
          MergeTuplesPreferOriginSlow(origin, difference, startIndex, length);
        break;
      case MergeBehavior.PreferDifference:
        if (useFast)
          PartiallyCopyTupleFast(packedDifference, packedOrigin, startIndex, startIndex, length);
        else
          PartiallyCopyTupleSlow(difference, origin, startIndex, startIndex, length);
        break;
      default:
        throw new ArgumentOutOfRangeException("behavior");
      }
    }

    /// <summary>
    /// Merges a range of fields from <paramref name="difference"/>
    /// <see cref="Tuple"/> starting at the specified index with the fields from
    /// <paramref name="origin"/> <see cref="Tuple"/> with the default <see cref="MergeBehavior"/>.
    /// </summary>
    /// <param name="origin">Tuple containing original values and receiving the data.</param>
    /// <param name="difference">Tuple with differences to merge with.</param>
    /// <param name="startIndex">The index in the <paramref name="difference"/> tuple at which merging begins.</param>
    /// <param name="length">The number of elements to process.</param>
    public static void MergeWith(this Tuple origin, Tuple difference, int startIndex, int length)
    {
      MergeWith(origin, difference, startIndex, length, MergeBehavior.Default);
    }

    /// <summary>
    /// Merges a range of fields from <paramref name="difference"/>
    /// <see cref="Tuple"/> starting at the specified index with the fields from
    /// <paramref name="origin"/> <see cref="Tuple"/> with the specified
    /// <paramref name="behavior"/>.
    /// </summary>
    /// <param name="origin">Tuple containing original values and receiving the data.</param>
    /// <param name="difference">Tuple with differences to merge with.</param>
    /// <param name="startIndex">The index in the <paramref name="difference"/> tuple at which merging begins.</param>
    /// <param name="behavior">The merge behavior that will be used to resolve conflicts when both values 
    /// from <paramref name="difference"/> and <paramref name="origin"/> are available.</param>
    public static void MergeWith(this Tuple origin, Tuple difference, int startIndex, MergeBehavior behavior)
    {
      MergeWith(origin, difference, startIndex, origin.Count, behavior);
    }

    /// <summary>
    /// Merges a range of fields from <paramref name="difference"/>
    /// <see cref="Tuple"/> starting at the specified index with the fields from
    /// <paramref name="origin"/> <see cref="Tuple"/> with the default value of <see cref="MergeBehavior"/>.
    /// </summary>
    /// <param name="origin">Tuple containing original values and receiving the data.</param>
    /// <param name="difference">Tuple with differences to merge with.</param>
    /// <param name="startIndex">The index in the <paramref name="difference"/> tuple at which merging begins.</param>
    public static void MergeWith(this Tuple origin, Tuple difference, int startIndex)
    {
      MergeWith(origin, difference, startIndex, origin.Count, MergeBehavior.Default);
    }

    /// <summary>
    /// Merges a range of fields from <paramref name="difference"/>
    /// <see cref="Tuple"/> starting at the specified index with the fields from
    /// <paramref name="origin"/> <see cref="Tuple"/> with the default value of <see cref="MergeBehavior"/>.
    /// </summary>
    /// <param name="origin">Tuple containing original values and receiving the data.</param>
    /// <param name="difference">Tuple with differences to merge with.</param>
    /// <param name="behavior">The merge behavior that will be used to resolve conflicts when both values 
    /// from <paramref name="difference"/> and <paramref name="origin"/> are available.</param>
    public static void MergeWith(this Tuple origin, Tuple difference, MergeBehavior behavior)
    {
      MergeWith(origin, difference, 0, origin.Count, behavior);
    }

    /// <summary>
    /// Merges a range of fields from <paramref name="difference"/>
    /// <see cref="Tuple"/> starting at the specified index with the fields from
    /// <paramref name="origin"/> <see cref="Tuple"/> with the default value of <see cref="MergeBehavior"/>.
    /// </summary>
    /// <param name="origin">Tuple containing original values and receiving the data.</param>
    /// <param name="difference">Tuple with differences to merge with.</param>
    public static void MergeWith(this Tuple origin, Tuple difference)
    {
      MergeWith(origin, difference, 0, origin.Count, MergeBehavior.Default);
    }

    #endregion

    #region ToXxx methods

    /// <summary>
    /// Creates <see cref="RegularTuple"/> instance "filled" with the same field values
    /// as the specified <paramref name="source"/> tuple.
    /// </summary>
    /// <param name="source">The tuple to clone as <see cref="RegularTuple"/>.</param>
    /// <returns>A new instance of <see cref="RegularTuple"/> with the same field values
    /// as the specified <paramref name="source"/> tuple.</returns>
    public static RegularTuple ToRegular(this Tuple source)
    {
      if (source==null)
        return null;
      var result = Tuple.Create(source.Descriptor);
      source.CopyTo(result);
      return result;
    }

    /// <summary>
    /// Converts <paramref name="source"/> tuple to read-only one.
    /// </summary>
    /// <param name="source">The tuple to convert to read-only.</param>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <returns>Read-only version of <paramref name="source"/> tuple.</returns>
    public static Tuple ToReadOnly(this Tuple source, TupleTransformType transformType)
    {
      if (source==null)
        return null;
      return ReadOnlyTransform.Instance.Apply(transformType, source);
    }

    /// <summary>
    /// Converts <paramref name="source"/> tuple to fast read-only one.
    /// </summary>
    /// <param name="source">The tuple to convert to fast read-only.</param>
    /// <returns>Fast read-only version of <paramref name="source"/> tuple.</returns>
    public static Tuple ToFastReadOnly(this Tuple source)
    {
      if (source==null)
        return null;
      if (source.GetType()==typeof (FastReadOnlyTuple))
        return source;
      return new FastReadOnlyTuple(source);
    }

    #endregion

    /// <summary>
    /// Gets the field state map of the specified <see cref="Tuple"/>.
    /// </summary>
    /// <param name="target">The <see cref="Tuple"/> to inspect.</param>
    /// <param name="requestedState">The state to compare with.</param>
    /// <returns>Newly created <see cref="BitArray"/> instance which holds inspection result.</returns>
    public static BitArray GetFieldStateMap(this Tuple target, TupleFieldState requestedState)
    {
      var count = target.Descriptor.Count;
      var result = new BitArray(count);

      switch (requestedState) {
      case TupleFieldState.Default:
        for (int i = 0; i < count; i++)
          result[i] = target.GetFieldState(i)==0;
        break;
      default:
        for (int i = 0; i < count; i++)
          result[i] = (requestedState & target.GetFieldState(i)) != 0;
        break;
      }
      return result;
    }

    /// <summary>
    /// Initializes the specified <see cref="Tuple"/> with default values.
    /// </summary>
    /// <param name="target">Tuple to initialize.</param>
    /// <param name="nullableMap"><see cref="BitArray"/> instance that flags that field should have null value.</param>
    /// <exception cref="ArgumentException">Tuple descriptor field count is not equal to <paramref name="nullableMap"/> count.</exception>
    public static void Initialize(this Tuple target, BitArray nullableMap)
    {
      if (target.Descriptor.Count!=nullableMap.Count)
        throw new ArgumentException(String.Format(Strings.ExInvalidFieldMapSizeExpectedX, target.Descriptor.Count));

      for (int i = 0; i < target.Count; i++) {
        if (nullableMap[i])
          target.SetFieldState(i, TupleFieldState.Available | TupleFieldState.Null);
        else
          target.SetFieldState(i, TupleFieldState.Available);
      }
    }

    private static void CopyValue(Tuple source, int sourceIndex, Tuple target, int targetIndex)
    {
      TupleFieldState fieldState;
      var value = source.GetValue(sourceIndex, out fieldState);
      if (!fieldState.IsAvailable())
        return;
      target.SetValue(targetIndex, fieldState.IsAvailableAndNull() ? null : value);
    }

    private static void CopyPackedValue(PackedTuple source, int sourceIndex, PackedTuple target, int targetIndex)
    {
      var sourceDescriptor = source.PackedDescriptor.FieldDescriptors[sourceIndex];
      var targetDescriptor = target.PackedDescriptor.FieldDescriptors[targetIndex];

      var fieldState = source.GetFieldState(sourceIndex);
      if (!fieldState.IsAvailable())
        return;

      if (fieldState.IsAvailableAndNull()) {
        target.SetValue(targetIndex, null);
        return;
      }

      var accessor = sourceDescriptor.Accessor;
      if (accessor!=targetDescriptor.Accessor)
        throw new InvalidOperationException(string.Format(
          Strings.ExInvalidCast,
          source.PackedDescriptor[sourceIndex],
          target.PackedDescriptor[targetIndex]));

      target.SetFieldState(targetIndex, TupleFieldState.Available);
      accessor.CopyValue(source, sourceDescriptor, target, targetDescriptor);
    }

    private static void PartiallyCopyTupleSlow(Tuple source, Tuple target, int sourceStartIndex, int targetStartIndex, int length)
    {
      for (int i = 0; i < length; i++)
        CopyValue(source, sourceStartIndex + i, target, targetStartIndex + i);
    }

    private static void PartiallyCopyTupleFast(PackedTuple source, PackedTuple target, int sourceStartIndex, int targetStartIndex, int length)
    {
      for (int i = 0; i < length; i++)
        CopyPackedValue(source, sourceStartIndex + i, target, targetStartIndex + i);
    }

    private static void CopyTupleWithMappingSlow(Tuple source, Tuple target, int[] map)
    {
      for (int targetIndex = 0; targetIndex < map.Length; targetIndex++) {
        var sourceIndex = map[targetIndex];
        if (sourceIndex >= 0)
          CopyValue(source, sourceIndex, target, targetIndex);
      }
    }

    private static void CopyTupleWithMappingFast(PackedTuple source, PackedTuple target, int[] map)
    {
      for (int targetIndex = 0; targetIndex < map.Length; targetIndex++) {
        var sourceIndex = map[targetIndex];
        if (sourceIndex >= 0)
          CopyPackedValue(source, sourceIndex, target, targetIndex);
      }
    }

    private static void CopyTupleArrayWithMappingSlow(Tuple[] sources, Tuple target, Pair<int, int>[] map)
    {
      for (int targetIndex = 0; targetIndex < map.Length; targetIndex++) {
        var sourceInfo = map[targetIndex];
        var sourceTupleIndex = sourceInfo.First;
        var sourceFieldIndex = sourceInfo.Second;
        if (sourceTupleIndex >= 0 && sourceFieldIndex >= 0)
          CopyValue(sources[sourceTupleIndex], sourceFieldIndex, target, targetIndex);
      }
    }

    private static void CopyTupleArrayWithMappingFast(PackedTuple[] sources, PackedTuple target, Pair<int, int>[] map)
    {
      for (int targetIndex = 0; targetIndex < map.Length; targetIndex++) {
        var sourceInfo = map[targetIndex];
        var sourceTupleIndex = sourceInfo.First;
        var sourceFieldIndex = sourceInfo.Second;
        if (sourceTupleIndex >= 0 && sourceFieldIndex >= 0)
          CopyPackedValue(sources[sourceTupleIndex], sourceFieldIndex, target, targetIndex);
      }
    }

    private static void Copy3TuplesWithMappingSlow(FixedList3<Tuple> sources, Tuple target, Pair<int, int>[] map)
    {
      for (int targetIndex = 0; targetIndex < map.Length; targetIndex++) {
        var sourceInfo = map[targetIndex];
        var sourceTupleIndex = sourceInfo.First;
        var sourceFieldIndex = sourceInfo.Second;
        if (sourceTupleIndex >= 0 && sourceFieldIndex >= 0)
          CopyValue(sources[sourceTupleIndex], sourceFieldIndex, target, targetIndex);
      }
    }

    private static void Copy3TuplesWithMappingFast(FixedList3<PackedTuple> sources, PackedTuple target, Pair<int, int>[] map)
    {
      for (int targetIndex = 0; targetIndex < map.Length; targetIndex++) {
        var sourceInfo = map[targetIndex];
        var sourceTupleIndex = sourceInfo.First;
        var sourceFieldIndex = sourceInfo.Second;
        if (sourceTupleIndex >= 0 && sourceFieldIndex >= 0)
          CopyPackedValue(sources[sourceTupleIndex], sourceFieldIndex, target, targetIndex);
      }
    }

    private static void MergeTuplesPreferOriginSlow(Tuple origin, Tuple difference, int startIndex, int length)
    {
      var bound = startIndex + length;
      for (int index = startIndex; index < bound; index++) {
        TupleFieldState fieldState;
        if (!origin.GetFieldState(index).IsAvailable())
          CopyValue(difference, index, origin, index);
      }
    }

    private static void MergeTuplesPreferOriginFast(PackedTuple origin, PackedTuple difference, int startIndex, int length)
    {
      var bound = startIndex + length;
      for (int index = startIndex; index < bound; index++) {
        TupleFieldState fieldState;
        if (!origin.GetFieldState(index).IsAvailable())
          CopyPackedValue(difference, index, origin, index);
      }
    }
  }
}