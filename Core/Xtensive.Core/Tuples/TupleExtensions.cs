// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.31

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Resources;
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
      var actionData = new PartCopyData(source, target, startIndex, targetStartIndex, length);
      var descriptor = target.Descriptor;
      var delegates = DelegateHelper.CreateDelegates<ExecutionSequenceHandler<PartCopyData>>(
        null, typeof (TupleExtensions), "PartCopyExecute", descriptor);
      DelegateHelper.ExecuteDelegates(delegates, ref actionData, Direction.Positive);
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
      var actionData = new MapOneCopyData(source, target, map);
      var descriptor = target.Descriptor;
      var delegates = DelegateHelper.CreateDelegates<ExecutionSequenceHandler<MapOneCopyData>>(
        null, typeof (TupleExtensions), "MapOneCopyExecute", descriptor);
      DelegateHelper.ExecuteDelegates(delegates, ref actionData, Direction.Positive);
    }

    /// <summary>
    /// Copies a set of elements from <paramref name="source"/> <see cref="Tuple"/>s
    /// to <paramref name="target"/> <see cref="Tuple"/> using 
    /// specified target-to-source field index <paramref name="map"/>.
    /// </summary>
    /// <param name="source">Source tuples to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="map">Target-to-source field index map.
    /// Negative value in this map means "skip this element".</param>
    public static void CopyTo(this Tuple[] source, Tuple target, Pair<int, int>[] map)
    {
      var actionData = new MapCopyData(source, target, map);
      var descriptor = target.Descriptor;
      var delegates = DelegateHelper.CreateDelegates<ExecutionSequenceHandler<MapCopyData>>(
        null, typeof (TupleExtensions), "MapCopyExecute", descriptor);
      DelegateHelper.ExecuteDelegates(delegates, ref actionData, Direction.Positive);
    }

    /// <summary>
    /// Copies a set of elements from <paramref name="source"/> <see cref="Tuple"/>s
    /// to <paramref name="target"/> <see cref="Tuple"/> using 
    /// specified target-to-source field index <paramref name="map"/>.
    /// </summary>
    /// <param name="source">Source tuples to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="map">Target-to-source field index map.
    /// Negative value in this map means "skip this element".</param>
    public static void CopyTo(this FixedList3<Tuple> source, Tuple target, Pair<int, int>[] map)
    {
      var actionData = new Map3CopyData(ref source, target, map);
      var descriptor = target.Descriptor;
      var delegates = DelegateHelper.CreateDelegates<ExecutionSequenceHandler<Map3CopyData>>(
        null, typeof (TupleExtensions), "Map3CopyExecute", descriptor);
      DelegateHelper.ExecuteDelegates(delegates, ref actionData, Direction.Positive);
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
      var endIndex = startIndex + length;

      var actionData = new MergeData(origin, difference, startIndex, endIndex, behavior);
      var descriptor = origin.Descriptor;
      var delegates = DelegateHelper.CreateDelegates<ExecutionSequenceHandler<MergeData>>(
        null, typeof (TupleExtensions), "MergeExecute", descriptor);
      DelegateHelper.ExecuteDelegates(delegates, ref actionData, Direction.Positive);
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

    #region Private: Part copy: Data & Handler

    private struct PartCopyData
    {
      public Tuple Source;
      public Tuple Target;
      public int TargetStartFieldIndex;
      public int TargetEndFieldIndex;
      public int SourceFieldIndexDiff;

      public PartCopyData(Tuple source, Tuple target, int startIndex, int targetStartIndex, int length)
      {
        Source = source;
        Target = target;
        TargetStartFieldIndex = targetStartIndex;
        TargetEndFieldIndex = targetStartIndex + length - 1;
        SourceFieldIndexDiff = startIndex - targetStartIndex;
      }
    }

    // ReSharper disable UnusedMember.Local
    private static bool PartCopyExecute<TFieldType>(ref PartCopyData actionData, int fieldIndex)
    {
      if (fieldIndex < actionData.TargetStartFieldIndex)
        return false;
      if (fieldIndex > actionData.TargetEndFieldIndex)
        return true;

      var source = actionData.Source;
      TupleFieldState fieldState;
      var value = source.GetValue<TFieldType>(fieldIndex + actionData.SourceFieldIndexDiff, out fieldState);
      if (fieldState.IsAvailable())
        if (fieldState.IsAvailableAndNull())
          actionData.Target.SetValue(fieldIndex, null);
        else
          actionData.Target.SetValue(fieldIndex, value);
      return false;
    }
    // ReSharper restore UnusedMember.Local


    #endregion

    #region Private: MapOne copy: Data & Handler

    private struct MapOneCopyData
    {
      public Tuple Source;
      public Tuple Target;
      public int[] Map;

      public MapOneCopyData(Tuple source, Tuple target, int[] map)
      {
        Source = source;
        Target = target;
        Map = map;
      }
    }

    // ReSharper disable UnusedMember.Local
    private static bool MapOneCopyExecute<TFieldType>(ref MapOneCopyData actionData, int fieldIndex)
    {
      int sourceFieldIndex = actionData.Map[fieldIndex];
      if (sourceFieldIndex < 0)
        return false;
      if (sourceFieldIndex >= actionData.Source.Count)
        return false;

      TupleFieldState fieldState;
      var value = actionData.Source.GetValue<TFieldType>(sourceFieldIndex, out fieldState);
      if (fieldState.IsAvailable())
        if (fieldState.IsAvailableAndNull())
          actionData.Target.SetValue(fieldIndex, null);
        else
          actionData.Target.SetValue(fieldIndex, value);
      return false;
    }
    // ReSharper restore UnusedMember.Local

    #endregion

    #region Private: Map copy: Data & Handler

    private struct MapCopyData
    {
      public Tuple[] Source;
      public Tuple Target;
      public Pair<int, int>[] Map;

      public MapCopyData(Tuple[] source, Tuple target, Pair<int, int>[] map)
      {
        Source = source;
        Target = target;
        Map = map;
      }
    }

    // ReSharper disable UnusedMember.Local
    private static bool MapCopyExecute<TFieldType>(ref MapCopyData actionData, int fieldIndex)
    {
      var mappedTo = actionData.Map[fieldIndex];
      if (mappedTo.First < 0 | mappedTo.Second < 0)
        return false;

      TupleFieldState fieldState;
      var sourceTuple = actionData.Source[mappedTo.First];
      var value = sourceTuple.GetValue<TFieldType>(mappedTo.Second, out fieldState);
      if (fieldState.IsAvailable())
        if (fieldState.IsAvailableAndNull())
          actionData.Target.SetValue(fieldIndex, null);
        else
          actionData.Target.SetValue(fieldIndex, value);
      return false;
    }
    // ReSharper restore UnusedMember.Local

    #endregion

    #region Private: Map3 copy: Data & Handler

    private struct Map3CopyData
    {
      public FixedList3<Tuple> Source;
      public Tuple Target;
      public Pair<int, int>[] Map;

      public Map3CopyData(ref FixedList3<Tuple> source, Tuple target, Pair<int, int>[] map)
      {
        Source = source;
        Target = target;
        Map = map;
      }
    }

    // ReSharper disable UnusedMember.Local
    private static bool Map3CopyExecute<TFieldType>(ref Map3CopyData actionData, int fieldIndex)
    {
      Pair<int, int> mappedTo = actionData.Map[fieldIndex];
      if (mappedTo.First < 0 | mappedTo.Second < 0)
        return false;

      TupleFieldState fieldState;
      Tuple sourceTuple = actionData.Source[mappedTo.First];
      var value = sourceTuple.GetValue<TFieldType>(mappedTo.Second, out fieldState);
      if (fieldState.IsAvailable())
        if (fieldState.IsAvailableAndNull())
          actionData.Target.SetValue(fieldIndex, null);
        else
          actionData.Target.SetValue(fieldIndex, value);
      return false;
    }
    // ReSharper restore UnusedMember.Local

    #endregion

    #region Private: Merge: Data & Handler

    private struct MergeData
    {
      public Tuple Origin;
      public Tuple Difference;
      public int StartIndex;
      public int EndIndex;
      public MergeBehavior Behavior;

      public MergeData(Tuple origin, Tuple difference, int startIndex, int endIndex, MergeBehavior behavior)
      {
        Origin = origin;
        Difference = difference;
        StartIndex = startIndex;
        EndIndex = endIndex;
        Behavior = behavior;
      }
    }

    // ReSharper disable UnusedMember.Local
    private static bool MergeExecute<TFieldType>(ref MergeData actionData, int fieldIndex)
    {
      if (fieldIndex < actionData.StartIndex)
        return false;
      if (fieldIndex > actionData.EndIndex)
        return true;

      var origin = actionData.Origin;
      var difference = actionData.Difference;
      var behavior = actionData.Behavior;

      TupleFieldState fieldState;
      var value = difference.GetValue<TFieldType>(fieldIndex, out fieldState);
      if (fieldState.IsAvailable()) {
        if (behavior == MergeBehavior.PreferOrigin && origin.GetFieldState(fieldIndex).IsAvailable())
          return false;
        if (fieldState.IsAvailableAndNull())
          origin.SetValue(fieldIndex, null);
        else
          origin.SetValue(fieldIndex, value);
      }
      return false;
    }
    // ReSharper restore UnusedMember.Local

    #endregion
  }
}