// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.07

using System;
using System.Collections;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;
using Xtensive.Tuples.Transform.Internals;

namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Base class for any tuple field mapping transform.
  /// Maps fields of destination tuple to fields of a set of source tuples.
  /// </summary>
  [Serializable]
  public class MapTransform : TupleTransformBase
  {
    private readonly bool isReadOnly;
    private int sourceCount;
    internal int[] singleSourceMap;
    internal Pair<int, int>[] map;

    /// <summary>
    /// Means that no mapping is available for the specified field index.
    /// </summary>
    public const int NoMapping = int.MinValue;

    /// <inheritdoc/>
    public override bool IsReadOnly {
      [DebuggerStepThrough]
      get { return isReadOnly; }
    }

    /// <summary>
    /// Gets the count of source <see cref="Tuples"/> this transform maps to the target one.
    /// </summary>
    public int SourceCount
    {
      [DebuggerStepThrough]
      get { return sourceCount; }
    }

    /// <summary>
    /// Gets or sets destination-to-source field map for the first source only.
    /// </summary>
    public int[] SingleSourceMap {
      [DebuggerStepThrough]
      get { return singleSourceMap.Copy(); }
      protected set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        Pair<int, int>[] newMap = new Pair<int, int>[Descriptor.Count];
        int index = 0;
        foreach (int mappedIndex in value)
          newMap[index++] = new Pair<int, int>(0, mappedIndex);
        while (index < newMap.Length)
          newMap[index++] = new Pair<int, int>(0, NoMapping);
        map = newMap;
        singleSourceMap = value;
        sourceCount = 1;
      }
    }

    /// <summary>
    /// Gets or sets destination-to-source field map.
    /// </summary>
    public Pair<int, int>[] Map {
      [DebuggerStepThrough]
      get { return map.Copy(); }
      protected set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        int[] newFirstSourceMap = new int[value.Length];
        int index = 0;
        int newSourceCount = -1;
        foreach (var mappedTo in value) {
          if (mappedTo.First>newSourceCount)
            newSourceCount = mappedTo.First;
          newFirstSourceMap[index++] = mappedTo.First==0 ? mappedTo.Second : -1;
        }
        newSourceCount++;
        map = value;
        if (newSourceCount==1)
          singleSourceMap = newFirstSourceMap;
        else
          singleSourceMap = null;
        sourceCount = newSourceCount;
      }
    }

    /// <inheritdoc/>
    public override Tuple Apply(TupleTransformType transformType, params object[] arguments)
    {
      ArgumentValidator.EnsureArgumentNotNull(arguments, "arguments");
      switch (sourceCount) {
      case 1:
        return Apply(transformType, (Tuple)arguments[0]);
      case 2:
        return Apply(transformType, (Tuple)arguments[0], (Tuple)arguments[1]);
      case 3:
        return Apply(transformType, (Tuple)arguments[0], (Tuple)arguments[1], (Tuple)arguments[2]);
      default:
        return Apply(transformType, arguments.Cast<object, Tuple>());
      }
    }

    /// <summary>
    /// Applies the transformation.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="sources">Transformation sources.</param>
    /// <returns>Transformation result - 
    /// either <see cref="TransformedTuple"/> or <see cref="Tuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    public Tuple Apply(TupleTransformType transformType, params Tuple[] sources)
    {
      ArgumentValidator.EnsureArgumentNotNull(sources, "sources");
      if (sourceCount>sources.Length)
        throw new InvalidOperationException(string.Format(Strings.ExTheNumberOfSourcesIsTooSmallExpected, sourceCount));
      switch (sourceCount) {
      case 1:
        return Apply(transformType, sources[0]);
      case 2:
        return Apply(transformType, sources[0], sources[1]);
      case 3:
        return Apply(transformType, sources[0], sources[1], sources[2]);
      default:
        switch (transformType) {
        case TupleTransformType.Auto:
          foreach (Tuple tuple in sources)
            if (tuple is TransformedTuple)
              goto case TupleTransformType.Tuple;
          goto case TupleTransformType.TransformedTuple;
        case TupleTransformType.TransformedTuple:
          return new MapTransformTuple(this, sources);
        case TupleTransformType.Tuple:
          Tuple result = Tuple.Create(Descriptor);
          sources.CopyTo(result, map);
          return result;
        default:
          throw new ArgumentOutOfRangeException("transformType");
        }
      }
    }

    /// <summary>
    /// Applies the transformation.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="source">Transformation source.</param>
    /// <returns>Transformation result - 
    /// either <see cref="TransformedTuple"/> or <see cref="Tuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    protected Tuple Apply(TupleTransformType transformType, Tuple source)
    {
      if (sourceCount>1)
        throw new InvalidOperationException(string.Format(Strings.ExTheNumberOfSourcesIsTooSmallExpected, sourceCount));
      switch (transformType) {
      case TupleTransformType.Auto:
        if (source is TransformedTuple)
          goto case TupleTransformType.Tuple;
        goto case TupleTransformType.TransformedTuple;
      case TupleTransformType.TransformedTuple:
        return new MapTransformTuple1(this, source);
      case TupleTransformType.Tuple:
        Tuple result = Tuple.Create(Descriptor);
        source.CopyTo(result, singleSourceMap);
        return result;
      default:
        throw new ArgumentOutOfRangeException("transformType");
      }
    }

    /// <summary>
    /// Applies the transformation.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="source1">First transformation source.</param>
    /// <param name="source2">Second transformation source.</param>
    /// <returns>Transformation result - 
    /// either <see cref="TransformedTuple"/> or <see cref="Tuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    protected Tuple Apply(TupleTransformType transformType, Tuple source1, Tuple source2)
    {
      if (sourceCount>2)
        throw new InvalidOperationException(string.Format(Strings.ExTheNumberOfSourcesIsTooSmallExpected, sourceCount));
      switch (transformType) {
      case TupleTransformType.Auto:
        if (source1 is TransformedTuple)
          goto case TupleTransformType.Tuple;
        if (source2 is TransformedTuple)
          goto case TupleTransformType.Tuple;
        goto case TupleTransformType.TransformedTuple;
      case TupleTransformType.TransformedTuple:
        return new MapTransformTuple3(this, source1, source2);
      case TupleTransformType.Tuple:
        FixedList3<Tuple> sources = new FixedList3<Tuple>(source1, source2);
        Tuple result = Tuple.Create(Descriptor);
        sources.CopyTo(result, map);
        return result;
      default:
        throw new ArgumentOutOfRangeException("transformType");
      }
    }

    /// <summary>
    /// Applies the transformation.
    /// </summary>
    /// <param name="transformType">The type of transformation to perform.</param>
    /// <param name="source1">First transformation source.</param>
    /// <param name="source2">Second transformation source.</param>
    /// <param name="source3">Third transformation source.</param>
    /// <returns>Transformation result - 
    /// either <see cref="TransformedTuple"/> or <see cref="Tuple"/> descendant,
    /// dependently on specified <paramref name="transformType"/>.</returns>
    protected Tuple Apply(TupleTransformType transformType, Tuple source1, Tuple source2, Tuple source3)
    {
      if (sourceCount>3)
        throw new InvalidOperationException(string.Format(Strings.ExTheNumberOfSourcesIsTooSmallExpected, sourceCount));
      switch (transformType) {
      case TupleTransformType.Auto:
        if (source1 is TransformedTuple)
          goto case TupleTransformType.Tuple;
        if (source2 is TransformedTuple)
          goto case TupleTransformType.Tuple;
        if (source3 is TransformedTuple)
          goto case TupleTransformType.Tuple;
        goto case TupleTransformType.TransformedTuple;
      case TupleTransformType.TransformedTuple:
        return new MapTransformTuple3(this, source1, source2, source3);
      case TupleTransformType.Tuple:
        FixedList3<Tuple> sources = new FixedList3<Tuple>(source1, source2, source3);
        Tuple result = Tuple.Create(Descriptor);
        sources.CopyTo(result, map);
        return result;
      default:
        throw new ArgumentOutOfRangeException("transformType");
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      string description = string.Format("{0}: {1}, {2}", 
        SourceCount, 
        SourceCount==1 ? 
          singleSourceMap.ToCommaDelimitedString() : 
          map.ToCommaDelimitedString(),
        isReadOnly ? 
          Strings.ReadOnlyShort : 
          Strings.ReadWriteShort);
      return string.Format(Strings.TupleTransformFormat, 
        GetType().GetShortName(), 
        description);
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="isReadOnly"><see cref="IsReadOnly"/> property value.</param>
    /// <param name="descriptor">Initial <see cref="TupleTransformBase.Descriptor"/> property value.</param>
    /// <param name="map"><see cref="Map"/> property value.</param>
    public MapTransform(bool isReadOnly, TupleDescriptor descriptor, Pair<int, int>[] map)
      : this(isReadOnly, descriptor)
    {
      Map = map;
    }
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="isReadOnly"><see cref="IsReadOnly"/> property value.</param>
    /// <param name="descriptor">Initial <see cref="TupleTransformBase.Descriptor"/> property value.</param>
    /// <param name="map"><see cref="SingleSourceMap"/> property value.</param>
    public MapTransform(bool isReadOnly, TupleDescriptor descriptor, int[] map)
      : this(isReadOnly, descriptor)
    {
      SingleSourceMap = map;
    }
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="isReadOnly"><see cref="IsReadOnly"/> property value.</param>
    /// <param name="descriptor">Initial <see cref="TupleTransformBase.Descriptor"/> property value.</param>
    protected MapTransform(bool isReadOnly, TupleDescriptor descriptor)
      : this(isReadOnly)
    {
      ArgumentValidator.EnsureArgumentNotNull(descriptor, "descriptor");
      Descriptor = descriptor;
      this.isReadOnly = isReadOnly;
    }
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="isReadOnly"><see cref="IsReadOnly"/> property value.</param>
    protected MapTransform(bool isReadOnly)
    {
      this.isReadOnly = isReadOnly;
    }
  }
}