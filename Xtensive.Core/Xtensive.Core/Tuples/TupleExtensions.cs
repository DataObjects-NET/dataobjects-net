// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.31

using System;
using System.Collections;
using Xtensive.Core.Collections;
using Xtensive.Core.Resources;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Various extension methods for <see cref="Tuple"/> and <see cref="Tuple"/> types.
  /// </summary>
  public static class TupleExtensions
  {
    private static readonly InitializerHandler initializerHandler = new InitializerHandler();
    private static readonly Func<TupleFieldState, TupleFieldState, bool> defaultPredicate = (request, result) => (result)==0;
    private static readonly Func<TupleFieldState, TupleFieldState, bool> availabilityPredicate = (request, result) => (request & result) > 0;

    #region Generic Tuple methods

    /// <summary>
    /// Sets the field value by its index.
    /// </summary>
    /// <param name="tuple"><see cref="Tuple"/> to set value to.</param>
    /// <param name="fieldIndex">Index of the field to set value of.</param>
    /// <param name="fieldValue">Field value.</param>
    /// <typeparam name="T">The type of value to set.</typeparam>
    /// <exception cref="InvalidCastException">Type of stored value and <typeparamref name="T"/>
    /// are incompatible.</exception>
    public static void SetValue<T>(this Tuple tuple, int fieldIndex, T fieldValue)
    {
      tuple.SetValue(fieldIndex, fieldValue);
    }

    /// <summary>
    /// Gets the value field value by its index, if it is available;
    /// otherwise returns <see langword="default(T)"/>.
    /// </summary>
    /// <param name="tuple">Value container.</param>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value, if it is available;
    /// otherwise, <see langword="default(T)"/>.</returns>
    /// <typeparam name="T">The type of value to get.</typeparam>
    /// <exception cref="InvalidCastException">Value is available, but it can't be cast
    /// to specified type. E.g. if value is <see langword="null"/>, field is struct, 
    /// but <typeparamref name="T"/> is not a <see cref="Nullable{T}"/> type.</exception>
    public static T GetValueOrDefault<T>(this Tuple tuple, int fieldIndex)
    {
      return (T) tuple.GetValueOrDefault(fieldIndex);
    }

    /// <summary>
    /// Gets the value field value by its index.
    /// </summary>
    /// <param name="tuple">Value container.</param>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value.</returns>
    /// <typeparam name="T">The type of value to get.</typeparam>
    /// <remarks>
    /// If field value is not available (see <see cref="Tuple.IsAvailable"/>),
    /// an exception will be thrown.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Field value is not available.</exception>
    /// <exception cref="InvalidCastException">Value is available, but it can't be cast
    /// to specified type. E.g. if value is <see langword="null"/>, field is struct, 
    /// but <typeparamref name="T"/> is not a <see cref="Nullable{T}"/> type.</exception>
    public static T GetValue<T>(this Tuple tuple, int fieldIndex)
    {
      return (T) tuple.GetValue(fieldIndex);
    }

    #endregion

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
      // A version with boxing. Works 6 times faster!
      for (int i = 0; i < length; i++) {
        int sourceIndex = startIndex + i;
        if (source.IsAvailable(sourceIndex))
          target.SetValue(targetStartIndex + i, source.GetValueOrDefault(sourceIndex));
      }
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
      // A version with boxing. Works 6 times faster!
      for (int i = 0; i < map.Length; i++) {
        var sourceIndex = map[i];
        if (sourceIndex >= 0 && source.IsAvailable(sourceIndex))
          target.SetValue(i, source.GetValueOrDefault(sourceIndex));
      }
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
      for (int i = 0; i < map.Length; i++) {
        var mappedTo = map[i];
        var sourceIndex = mappedTo.Second;
        if (mappedTo.First >= 0 && sourceIndex >= 0) {
          var sourceTuple = source[mappedTo.First];
          if (sourceTuple.IsAvailable(sourceIndex))
            target.SetValue(i, sourceTuple.GetValueOrDefault(sourceIndex));
        }
      }
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
      for (int i = 0; i < map.Length; i++) {
        var mappedTo = map[i];
        var sourceIndex = mappedTo.Second;
        if (mappedTo.First >= 0 && sourceIndex >= 0) {
          var sourceTuple = source[mappedTo.First];
          if (sourceTuple.IsAvailable(sourceIndex))
            target.SetValue(i, sourceTuple.GetValueOrDefault(sourceIndex));
        }
      }
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
    /// Cuts out <paramref name="segment"/> from <paramref name="left"/> <see cref="Tuple"/>.
    /// </summary>
    /// <param name="left">The <see cref="Tuple"/> to get segment from.</param>
    /// <param name="segment">The <see cref="Segment{T}"/> to cut off.</param>
    /// <returns></returns>
    public static Tuple GetSegment(this Tuple left, Segment<int> segment)
    {
      var map = new int[segment.Length];
      for (int i = 0; i < segment.Length; i++)
        map[i] = segment.Offset + i;

      var types = new Collections.ArraySegment<Type>(left.Descriptor.fieldTypes, segment.Offset, segment.Length);
      var descriptor = TupleDescriptor.Create(types);
      var transform = new MapTransform(false, descriptor, map);
      return transform.Apply(TupleTransformType.TransformedTuple, left);
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
        throw new ArgumentException(
          string.Format(Strings.ExInvalidTupleDescriptorExpectedDescriptorIs, origin.Descriptor),
          "difference");
      var endIndex = startIndex + length;
      for (int i = startIndex; i < endIndex; i++) {
        if (!difference.IsAvailable(i))
          continue;
        if (origin.IsAvailable(i) && behavior==MergeBehavior.PreferOrigin)
          continue;
        origin.SetValue(i, difference.GetValueOrDefault(i));
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
    /// from <paramref name="difference"/> and <paramref name="origin"/> are available.</param>
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
    /// from <paramref name="difference"/> and <paramref name="origin"/> are available.</param>
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
    /// from <paramref name="difference"/> and <paramref name="origin"/> are available.</param>
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
    /// <param name="state">The state to compare with.</param>
    /// <returns>Newly created <see cref="BitArray"/> instance which holds inspection result.</returns>
    public static BitArray GetFieldStateMap(this Tuple target, TupleFieldState state)
    {
      Func<TupleFieldState, TupleFieldState, bool> predicate;
      switch (state) {
      case TupleFieldState.Default:
        predicate = defaultPredicate;
        break;
      default:
        predicate = availabilityPredicate;
        break;
      }
      return target.GetFieldStateMap(state, predicate);
    }

    private static BitArray GetFieldStateMap(this Tuple target, TupleFieldState state, Func<TupleFieldState, TupleFieldState, bool> predicate)
    {
      var result = new BitArray(target.Descriptor.Count);

      for (int i = 0; i < target.Descriptor.Count; i++)
        result[i] = predicate(state, target.GetFieldState(i));

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

      // TODO: declare method Initialize for Tuple and generate them

      var actionData = new InitializerData(target, nullableMap);
      target.Descriptor.Execute(initializerHandler, ref actionData, Direction.Positive);
    }

    #region Private: Initializer: Data & Handler

    private struct InitializerData
    {
      public readonly Tuple Target;
      private readonly BitArray nullableMap;

      public bool IsNullable(int fieldIndex)
      {
        return nullableMap[fieldIndex];
      }

      public InitializerData(Tuple target, BitArray fieldMap)
      {
        Target = target;
        nullableMap = fieldMap;
      }
    }

    private class InitializerHandler : ITupleActionHandler<InitializerData>
    {
      public bool Execute<TFieldType>(ref InitializerData actionData, int fieldIndex)
      {
        if (actionData.IsNullable(fieldIndex))
          actionData.Target.SetValue(fieldIndex, null);
        else
          actionData.Target.SetValue(fieldIndex, default(TFieldType));
        return false;
      }
    }

    #endregion
  }
}