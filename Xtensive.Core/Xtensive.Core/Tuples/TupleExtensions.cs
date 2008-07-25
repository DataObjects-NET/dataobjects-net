// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.31

using System;
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
    private static PartCopyHandler   partCopyHandler   = new PartCopyHandler();
    private static MergeHandler      mergeHandler   = new MergeHandler();
    private static MapOneCopyHandler mapOneCopyHandler = new MapOneCopyHandler();
    private static MapCopyHandler    mapCopyHandler    = new MapCopyHandler();
    private static Map3CopyHandler   map3CopyHandler   = new Map3CopyHandler();

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

      PartCopyData actionData = new PartCopyData(source, target, startIndex, targetStartIndex, length);
      source.Descriptor.Execute(partCopyHandler, ref actionData, Direction.Positive);
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

      MapOneCopyData actionData = new MapOneCopyData(source, target, map);
      target.Descriptor.Execute(mapOneCopyHandler, ref actionData, Direction.Positive);
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

      MapCopyData actionData = new MapCopyData(source, target, map);
      target.Descriptor.Execute(mapCopyHandler, ref actionData, Direction.Positive);
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

      Map3CopyData actionData = new Map3CopyData(ref source, target, map);
      target.Descriptor.Execute(map3CopyHandler, ref actionData, Direction.Positive);
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
      RegularTuple result = Tuple.Create(source.Descriptor);
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
          String.Format(Strings.ExInvalidTupleDescriptorExpectedDescriptorIs, target.Descriptor),
          "source");

      MergeData actionData = new MergeData(source, target, startIndex, length, behavior);
      source.Descriptor.Execute(mergeHandler, ref actionData, Direction.Positive);
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

    #region Private: Part copy: Data & Handler

    private struct PartCopyData
    {
      public ITuple Source;
      public ITuple Target;
      public int SourceStartFieldIndex;
      public int SourceEndFieldIndex;
      public int ResultStartFieldIndex;
      public int ResultStartFieldIndexDiff;

      public PartCopyData(ITuple source, ITuple target, int sourceStartFieldIndex, int resultStartFieldIndex, int count)
      {
        Source = source;
        Target = target;
        SourceStartFieldIndex = sourceStartFieldIndex;
        SourceEndFieldIndex   = sourceStartFieldIndex + count-1;
        ResultStartFieldIndex = resultStartFieldIndex;
        ResultStartFieldIndexDiff = resultStartFieldIndex - sourceStartFieldIndex;
      }
    }

    private class PartCopyHandler: ITupleActionHandler<PartCopyData>
    {
      public bool Execute<TFieldType>(ref PartCopyData actionData, int fieldIndex)
      {
        if (fieldIndex < actionData.SourceStartFieldIndex)
          return false;
        if (fieldIndex > actionData.SourceEndFieldIndex)
          return true;

        if (actionData.Source.IsAvailable(fieldIndex)) {
          if (actionData.Source.Descriptor[fieldIndex].IsValueType && actionData.Source.IsNull(fieldIndex))
            actionData.Target.SetValue(fieldIndex + actionData.ResultStartFieldIndexDiff, null);
          else
            actionData.Target.SetValue(fieldIndex + actionData.ResultStartFieldIndexDiff, actionData.Source.GetValue<TFieldType>(fieldIndex));
        }
        return false;
      }
    }

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

    private class MapOneCopyHandler: ITupleActionHandler<MapOneCopyData>
    {
      public bool Execute<TFieldType>(ref MapOneCopyData actionData, int fieldIndex)
      {
        int sourceFieldIndex = actionData.Map[fieldIndex];
        if (sourceFieldIndex<0)
          return false;
        if (actionData.Source.IsAvailable(sourceFieldIndex)) {
          if (actionData.Source.Descriptor[sourceFieldIndex].IsValueType && actionData.Source.IsNull(sourceFieldIndex))
            actionData.Target.SetValue(fieldIndex, null);
          else
            actionData.Target.SetValue(fieldIndex, actionData.Source.GetValue<TFieldType>(sourceFieldIndex));
        }
        return false;
      }
    }

    #endregion

    #region Private: Map copy: Data & Handler

    private struct MapCopyData
    {
      public Tuple[] Source;
      public Tuple Target;
      public Pair<int,int>[] Map;

      public MapCopyData(Tuple[] source, Tuple target, Pair<int,int>[] map)
      {
        Source = source;
        Target = target;
        Map = map;
      }
    }

    private class MapCopyHandler: ITupleActionHandler<MapCopyData>
    {
      public bool Execute<TFieldType>(ref MapCopyData actionData, int fieldIndex)
      {
        Pair<int,int> mappedTo = actionData.Map[fieldIndex];
        if (mappedTo.First<0 | mappedTo.Second<0)
          return false;
        Tuple sourceTuple = actionData.Source[mappedTo.First];
        if (sourceTuple.IsAvailable(mappedTo.Second)) {
          if (sourceTuple.Descriptor[mappedTo.Second].IsValueType && sourceTuple.IsNull(mappedTo.Second))
            actionData.Target.SetValue(fieldIndex, null);
          else
            actionData.Target.SetValue(fieldIndex, sourceTuple.GetValue<TFieldType>(mappedTo.Second));
        }
        return false;
      }
    }

    #endregion

    #region Private: Map3 copy: Data & Handler

    private struct Map3CopyData
    {
      public FixedList3<Tuple> Source;
      public Tuple Target;
      public Pair<int,int>[] Map;

      public Map3CopyData(ref FixedList3<Tuple> source, Tuple target, Pair<int,int>[] map)
      {
        Source = source;
        Target = target;
        Map = map;
      }
    }

    private class Map3CopyHandler: ITupleActionHandler<Map3CopyData>
    {
      public bool Execute<TFieldType>(ref Map3CopyData actionData, int fieldIndex)
      {
        Pair<int,int> mappedTo = actionData.Map[fieldIndex];
        if (mappedTo.First<0 | mappedTo.Second<0)
          return false;
        Tuple sourceTuple = actionData.Source[mappedTo.First];
        if (sourceTuple.IsAvailable(mappedTo.Second)) {
          if (sourceTuple.Descriptor[mappedTo.Second].IsValueType && sourceTuple.IsNull(mappedTo.Second))
            actionData.Target.SetValue(fieldIndex, null);
          else
            actionData.Target.SetValue(fieldIndex, sourceTuple.GetValue<TFieldType>(mappedTo.Second));
        }
        return false;
      }
    }

    #endregion

    #region Private: Merge: Data & Handler

    private struct MergeData
    {
      public ITuple Source;
      public ITuple Target;
      public int StartIndex;
      public int EndIndex;
      public MergeConflictBehavior Behavior;

      public MergeData(ITuple source, ITuple target, int startIndex, int length, MergeConflictBehavior behavior)
      {
        Source = source;
        Target = target;
        StartIndex = startIndex;
        EndIndex   = startIndex + length-1;
        Behavior = behavior;
      }
    }

    private class MergeHandler: ITupleActionHandler<MergeData>
    {
      public bool Execute<TFieldType>(ref MergeData actionData, int fieldIndex)
      {
        if (fieldIndex < actionData.StartIndex)
          return false;
        if (fieldIndex > actionData.EndIndex)
          return true;
        if (!actionData.Source.IsAvailable(fieldIndex))
          return false;
        if (actionData.Source.IsAvailable(fieldIndex) && actionData.Target.IsAvailable(fieldIndex) 
          && actionData.Behavior == MergeConflictBehavior.PreferTarget)
          return false;

        if (actionData.Source.Descriptor[fieldIndex].IsValueType && actionData.Source.IsNull(fieldIndex))
          actionData.Target.SetValue(fieldIndex, null);
        else
          actionData.Target.SetValue(fieldIndex, actionData.Source.GetValue<TFieldType>(fieldIndex));
        return false;
      }
    }

    #endregion
  }
}