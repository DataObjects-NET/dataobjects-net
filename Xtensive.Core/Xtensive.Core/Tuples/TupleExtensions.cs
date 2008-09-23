// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.31

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Resources;
using Xtensive.Core.Tuples.Internals;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Various extension methods for <see cref="ITuple"/> and <see cref="Tuple"/> types.
  /// </summary>
  public static class TupleExtensions
  {
    private static readonly InitializerHandler     initializerHandler   = new InitializerHandler();
    private static readonly Dictionary<TupleFieldState, Func<TupleFieldState, TupleFieldState, bool>> fieldStatePredicates;

    #region Generic ITuple methods

    /// <summary>
    /// Sets the field value by its index.
    /// </summary>
    /// <param name="tuple"><see cref="Tuple"/> to set value to.</param>
    /// <param name="fieldIndex">Index of the field to set value of.</param>
    /// <param name="fieldValue">Field value.</param>
    /// <typeparam name="T">The type of value to set.</typeparam>
    /// <exception cref="InvalidCastException">Type of stored value and <typeparamref name="T"/>
    /// are incompatible.</exception>
    public static void SetValue<T>(this ITuple tuple, int fieldIndex, T fieldValue)
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
    public static T GetValueOrDefault<T>(this ITuple tuple, int fieldIndex)
    {
      var value = tuple.GetValueOrDefault(fieldIndex);
      if (value == null)
        return default(T);
      return (T)value;
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
    public static T GetValue<T>(this ITuple tuple, int fieldIndex)
    {
      if (!tuple.IsAvailable(fieldIndex))
        throw new InvalidOperationException(Strings.ExValueIsNotAvailable);
      return tuple.GetValueOrDefault<T>(fieldIndex);
    }


    #endregion
    
    #region Copy methods

    /// <summary>
    /// Copies a range of elements from <paramref name="source"/> <see cref="ITuple"/> 
    /// starting at the specified source index 
    /// and pastes them to <paramref name="target"/> <see cref="ITuple"/> 
    /// starting at the specified target index. 
    /// </summary>
    /// <param name="source">Source tuple to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="startIndex">The index in the <paramref name="source"/> tuple at which copying begins.</param>
    /// <param name="targetStartIndex">The index in the <paramref name="target"/> tuple at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    public static void CopyTo(this ITuple source, ITuple target, int startIndex, int targetStartIndex, int length)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentIsInRange(startIndex, 0, source.Count, "startIndex");
      ArgumentValidator.EnsureArgumentIsInRange(targetStartIndex, 0, target.Count, "targetStartIndex");
      
      // A version with boxing. Works 6 times faster!
      for (int i = 0; i < length; i++) {
        int sourceIndex = startIndex + i;
        if (source.IsAvailable(sourceIndex))
          target.SetValue(targetStartIndex + i, source.GetValueOrDefault(sourceIndex));
      }

//      // Generic version. Slower.
//      var actionData = new PartCopyData(source, target, startIndex, targetStartIndex, length);
//      source.Descriptor.Execute(partCopyHandler, ref actionData, Direction.Positive);
    }

    /// <summary>
    /// Copies a range of elements from <paramref name="source"/> <see cref="ITuple"/> 
    /// starting at the specified source index 
    /// and pastes them to <paramref name="target"/> <see cref="ITuple"/> 
    /// starting at the first element. 
    /// </summary>
    /// <param name="source">Source tuple to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="startIndex">The index in the <paramref name="source"/> tuple at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    public static void CopyTo(this ITuple source, ITuple target, int startIndex, int length)
    {
      source.CopyTo(target, startIndex, 0, length);
    }

    /// <summary>
    /// Copies a range of elements from <paramref name="source"/> <see cref="ITuple"/> 
    /// starting at the <paramref name="startIndex"/>
    /// and pastes them into <paramref name="target"/> <see cref="ITuple"/> 
    /// starting at the first element. 
    /// </summary>
    /// <param name="source">Source tuple to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="startIndex">The index in the <paramref name="source"/> tuple at which copying begins.</param>
    public static void CopyTo(this ITuple source, ITuple target, int startIndex)
    {
      source.CopyTo(target, startIndex, 0, source.Count - startIndex);
    }

    /// <summary>
    /// Copies all the elements from <paramref name="source"/> <see cref="ITuple"/> 
    /// starting at the first element
    /// and pastes them into <paramref name="target"/> <see cref="ITuple"/> 
    /// starting at the first element.
    /// </summary>
    /// <param name="source">Source tuple to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    public static void CopyTo(this ITuple source, ITuple target)
    {
      source.CopyTo(target, 0, 0, source.Count);
    }

    /// <summary>
    /// Copies a set of elements from <paramref name="source"/> <see cref="ITuple"/> 
    /// to <paramref name="target"/> <see cref="ITuple"/> using 
    /// specified target-to-source field index <paramref name="map"/>.
    /// </summary>
    /// <param name="source">Source tuple to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="map">Target-to-source field index map.
    /// Negative value in this map means "skip this element".</param>
    public static void CopyTo(this Tuple source, Tuple target, int[] map)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(target, "target");

      // A version with boxing. Works 6 times faster!
      for (int i = 0; i < map.Length; i++) {
        var sourceIndex = map[i];
        if (sourceIndex > 0 && source.IsAvailable(sourceIndex))
          target.SetValue(i, source.GetValueOrDefault(sourceIndex));
      }

//      var actionData = new MapOneCopyData(source, target, map);
//      target.Descriptor.Execute(mapOneCopyHandler, ref actionData, Direction.Positive);
    }

    /// <summary>
    /// Copies a set of elements from <paramref name="source"/> <see cref="ITuple"/>s
    /// to <paramref name="target"/> <see cref="ITuple"/> using 
    /// specified target-to-source field index <paramref name="map"/>.
    /// </summary>
    /// <param name="source">Source tuples to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="map">Target-to-source field index map.
    /// Negative value in this map means "skip this element".</param>
    public static void CopyTo(this Tuple[] source, Tuple target, Pair<int,int>[] map)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(target, "target");

      for (int i = 0; i < map.Length; i++) {
        var mappedTo = map[i];
        var sourceIndex = mappedTo.Second;
        if (mappedTo.First > 0 && sourceIndex > 0) {
          var sourceTuple = source[mappedTo.First];
          if (sourceTuple.IsAvailable(sourceIndex))
            target.SetValue(i, sourceTuple.GetValueOrDefault(sourceIndex));
        }
      }

//      var actionData = new MapCopyData(source, target, map);
//      target.Descriptor.Execute(mapCopyHandler, ref actionData, Direction.Positive);
    }

    /// <summary>
    /// Copies a set of elements from <paramref name="source"/> <see cref="ITuple"/>s
    /// to <paramref name="target"/> <see cref="ITuple"/> using 
    /// specified target-to-source field index <paramref name="map"/>.
    /// </summary>
    /// <param name="source">Source tuples to copy.</param>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="map">Target-to-source field index map.
    /// Negative value in this map means "skip this element".</param>
    public static void CopyTo(this FixedList3<Tuple> source, Tuple target, Pair<int,int>[] map)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");

      for (int i = 0; i < map.Length; i++) {
        var mappedTo = map[i];
        var sourceIndex = mappedTo.Second;
        if (mappedTo.First > 0 && sourceIndex > 0) {
          var sourceTuple = source[mappedTo.First];
          if (sourceTuple.IsAvailable(sourceIndex))
            target.SetValue(i, sourceTuple.GetValueOrDefault(sourceIndex));
        }
      }

//      var actionData = new Map3CopyData(ref source, target, map);
//      target.Descriptor.Execute(map3CopyHandler, ref actionData, Direction.Positive);
    }

    #endregion

    /// <summary>
    /// Creates <see cref="RegularTuple"/> instance "filled" with the same field values
    /// as the specified <paramref name="source"/> tuple.
    /// </summary>
    /// <param name="source">The tuple to clone as <see cref="RegularTuple"/>.</param>
    /// <returns>A new instance of <see cref="RegularTuple"/> with the same field values
    /// as the specified <paramref name="source"/> tuple.</returns>
    public static RegularTuple ToRegular(this ITuple source)
    {
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
      return ReadOnlyTransform.Instance.Apply(transformType, source);
    }

    /// <summary>
    /// Combines the <paramref name="source1"/> with <paramref name="source2"/>.
    /// </summary>
    /// <param name="source1">The source <see cref="Tuple"/> to combine with the <paramref name="source2"/>.</param>
    /// <param name="source2">The <see cref="Tuple"/> to combine with.</param>
    /// <returns></returns>
    public static Tuple CombineWith(this Tuple source1, Tuple source2)
    {
      ArgumentValidator.EnsureArgumentNotNull(source1, "source1");
      ArgumentValidator.EnsureArgumentNotNull(source2, "source2");
      
      var transform = new CombineTransform(false, new[] { source1.Descriptor, source2.Descriptor});
      return transform.Apply(TupleTransformType.TransformedTuple, source1, source2);
    }

    /// <summary>
    /// Initializes the specified <see cref="Tuple"/> with default values.
    /// </summary>
    /// <param name="target">Tuple to initialize.</param>
    /// <param name="nullableMap"><see cref="BitArray"/> instance that flags that field should have null value.</param>
    /// <exception cref="ArgumentException">Tuple descriptor field count is not equal to <paramref name="nullableMap"/> count.</exception>
    public static void Initialize(this ITuple target, BitArray nullableMap)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(nullableMap, "nullableMap");

      if (target.Descriptor.Count!=nullableMap.Count)
        throw new ArgumentException(String.Format(Strings.ExInvalidFieldMapSizeExpectedX, target.Descriptor.Count));

      // TODO: declare method Initialize for Tuple and generate them

      var actionData = new InitializerData(target, nullableMap);
      target.Descriptor.Execute(initializerHandler, ref actionData, Direction.Positive);
    }

    /// <summary>
    /// Gets the field state map of the specified <see cref="Tuple"/>.
    /// </summary>
    /// <param name="target">The <see cref="Tuple"/> to inspect.</param>
    /// <param name="state">The state to compare with.</param>
    /// <returns>Newly created <see cref="BitArray"/> instance which holds inspection result.</returns>
    public static BitArray GetFieldStateMap(this ITuple target, TupleFieldState state)
    {
      return target.GetFieldStateMap(state, fieldStatePredicates[state]);
    }

    private static BitArray GetFieldStateMap(this ITuple target, TupleFieldState state, Func<TupleFieldState, TupleFieldState, bool> predicate)
    {
      var result = new BitArray(target.Descriptor.Count);

      for (int i = 0; i < target.Descriptor.Count; i++)
        result[i] = predicate(state, target.GetFieldState(i));

      return result;
    }

    #region Merge methods

    /// <summary>
    /// Merges a range of fields from <paramref name="source"/>
    /// <see cref="ITuple"/> starting at the specified index with the fields from
    /// <paramref name="target"/> <see cref="ITuple"/> with the specified
    /// <paramref name="behavior"/>.
    /// </summary>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="source">Source tuple to merge with.</param>
    /// <param name="startIndex">The index in the <paramref name="source"/> tuple at which merging begins.</param>
    /// <param name="length">The number of elements to process.</param>
    /// <param name="behavior">The merge behavior that will be used to resolve conflicts when both values 
    /// from <paramref name="source"/> and <paramref name="target"/> are available.</param>
    /// <exception cref="ArgumentException">Tuple descriptors mismatch.</exception>
    public static void MergeWith(this ITuple target, ITuple source, int startIndex, int length, MergeConflictBehavior behavior)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentIsInRange(startIndex, 0, source.Count, "startIndex");
      if (target.Descriptor!=source.Descriptor)
        throw new ArgumentException(
          string.Format(Strings.ExInvalidTupleDescriptorExpectedDescriptorIs, target.Descriptor),
          "source");
      var endIndex = startIndex + length;
      for (int i = startIndex; i < endIndex; i++) {
        if (!source.IsAvailable(i))
          continue;
        if (target.IsAvailable(i) && behavior == MergeConflictBehavior.PreferTarget)
          continue;
        target.SetValue(i, source.GetValueOrDefault(i));
      }

//      MergeData actionData = new MergeData(source, target, startIndex, length, behavior);
//      source.Descriptor.Execute(mergeHandler, ref actionData, Direction.Positive);
    }

    /// <summary>
    /// Merges a range of fields from <paramref name="source"/>
    /// <see cref="ITuple"/> starting at the specified index with the fields from
    /// <paramref name="target"/> <see cref="ITuple"/> with the default <see cref="MergeConflictBehavior"/>.
    /// </summary>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="source">Source tuple to merge with.</param>
    /// <param name="startIndex">The index in the <paramref name="source"/> tuple at which merging begins.</param>
    /// <param name="length">The number of elements to process.</param>
    /// from <paramref name="source"/> and <paramref name="target"/> are available.</param>
    public static void MergeWith(this ITuple target, ITuple source, int startIndex, int length)
    {
      MergeWith(target, source, startIndex, length, MergeConflictBehavior.Default);
    }

    /// <summary>
    /// Merges a range of fields from <paramref name="source"/>
    /// <see cref="ITuple"/> starting at the specified index with the fields from
    /// <paramref name="target"/> <see cref="ITuple"/> with the specified
    /// <paramref name="behavior"/>.
    /// </summary>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="source">Source tuple to process.</param>
    /// <param name="startIndex">The index in the <paramref name="source"/> tuple at which merging begins.</param>
    /// <param name="behavior">The merge behavior that will be used to resolve conflicts when both values 
    /// from <paramref name="source"/> and <paramref name="target"/> are available.</param>
    public static void MergeWith(this ITuple target, ITuple source, int startIndex, MergeConflictBehavior behavior)
    {
      MergeWith(target, source, startIndex, target.Count, behavior);
    }

    /// <summary>
    /// Merges a range of fields from <paramref name="source"/>
    /// <see cref="ITuple"/> starting at the specified index with the fields from
    /// <paramref name="target"/> <see cref="ITuple"/> with the default value of <see cref="MergeConflictBehavior"/>.
    /// </summary>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="source">Source tuple to process.</param>
    /// <param name="startIndex">The index in the <paramref name="source"/> tuple at which merging begins.</param>
    /// from <paramref name="source"/> and <paramref name="target"/> are available.</param>
    public static void MergeWith(this ITuple target, ITuple source, int startIndex)
    {
      MergeWith(target, source, startIndex, target.Count, MergeConflictBehavior.Default);
    }

    /// <summary>
    /// Merges a range of fields from <paramref name="source"/>
    /// <see cref="ITuple"/> starting at the specified index with the fields from
    /// <paramref name="target"/> <see cref="ITuple"/> with the default value of <see cref="MergeConflictBehavior"/>.
    /// </summary>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="source">Source tuple to process.</param>
    /// from <paramref name="source"/> and <paramref name="target"/> are available.</param>
    public static void MergeWith(this ITuple target, ITuple source, MergeConflictBehavior behavior)
    {
      MergeWith(target, source, 0, target.Count, behavior);
    }

    /// <summary>
    /// Merges a range of fields from <paramref name="source"/>
    /// <see cref="ITuple"/> starting at the specified index with the fields from
    /// <paramref name="target"/> <see cref="ITuple"/> with the default value of <see cref="MergeConflictBehavior"/>.
    /// </summary>
    /// <param name="target">Tuple that receives the data.</param>
    /// <param name="source">Source tuple to process.</param>
    /// from <paramref name="source"/> and <paramref name="target"/> are available.</param>
    public static void MergeWith(this ITuple target, ITuple source)
    {
      MergeWith(target, source, 0, target.Count, MergeConflictBehavior.Default);
    }

    #endregion

    #region Private: Initializer: Data & Handler

    private struct InitializerData
    {
      public ITuple Target;
      private BitArray nullableMap;

      public bool IsNullable(int fieldIndex)
      {
        return nullableMap[fieldIndex];
      }

      public InitializerData(ITuple target, BitArray fieldMap)
      {
        Target = target;
        this.nullableMap = fieldMap;
      }
    }

    private class InitializerHandler: ITupleActionHandler<InitializerData>
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


    // Type initializer

    static TupleExtensions()
    {
      fieldStatePredicates = new Dictionary<TupleFieldState, Func<TupleFieldState, TupleFieldState, bool>>();
      fieldStatePredicates.Add(TupleFieldState.Default, (request, result) => (result)==0);
      fieldStatePredicates.Add(TupleFieldState.IsAvailable, (request, result) => (request & result) > 0);
      fieldStatePredicates.Add(TupleFieldState.IsNull, fieldStatePredicates[TupleFieldState.IsAvailable]);
    }
  }
}