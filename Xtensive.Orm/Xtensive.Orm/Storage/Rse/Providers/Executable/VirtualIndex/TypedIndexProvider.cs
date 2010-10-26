// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.26

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Model;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing;
using System.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Providers.Executable;
using Xtensive.Storage.Rse.Providers.Executable.VirtualIndex.Internal;
using Xtensive.Helpers;


namespace Xtensive.Storage.Rse.Providers.Executable.VirtualIndex
{
  /// <summary>
  /// General virtual <see cref="IndexAttributes.Typed"/> index provider for all indexing storage handlers.
  /// </summary>
  [Serializable]
  public class TypedIndexProvider : UnaryExecutableProvider<Compilable.IndexProvider>,
    IOrderedEnumerable<Tuple,Tuple>,
    ICountable
  {
    private readonly int typeIdColumn;
    private readonly int typeId;
    private Func<Tuple, Tuple> typeInjector;

    /// <inheritdoc/>
    public long Count {
      get {
        var countable = Source.GetService<ICountable>(true);
        return countable.Count;
      }
    }

    #region Root delegating members

    /// <inheritdoc/>
    public Func<Entire<Tuple>, Tuple, int> AsymmetricKeyCompare
    {
      get { return Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).AsymmetricKeyCompare; }
    }

    /// <inheritdoc/>
    public AdvancedComparer<Entire<Tuple>> EntireKeyComparer
    {
      get { return Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).EntireKeyComparer; }
    }

    /// <inheritdoc/>
    public AdvancedComparer<Tuple> KeyComparer
    {
      get { return Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyComparer; }
    }

    /// <inheritdoc/>
    public Converter<Tuple, Tuple> KeyExtractor
    {
      get { return Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).KeyExtractor; }
    }

    #endregion

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(Range<Entire<Tuple>> range)
    {
      var items = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetItems(range);
      return items.Select(typeInjector);
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetItems(RangeSet<Entire<Tuple>> range)
    {
      var items = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetItems(range);
      return items.Select(typeInjector);
    }

    /// <inheritdoc/>
    public IEnumerable<Tuple> GetKeys(Range<Entire<Tuple>> range)
    {
      var items = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).GetItems(range);
      return items.Select(typeInjector).Select(tuple => KeyExtractor(tuple));
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Ray<Entire<Tuple>> ray)
    {
      var seek = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).Seek(ray);
      var result = seek.ResultType == SeekResultType.None
        ? default(Tuple)
        : typeInjector(seek.Result);
      return new SeekResult<Tuple>(seek.ResultType, result);
    }

    /// <inheritdoc/>
    public SeekResult<Tuple> Seek(Tuple key)
    {
      var seek = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).Seek(key);
      var result = seek.ResultType == SeekResultType.None
        ? default(Tuple)
        : typeInjector(seek.Result);
      return new SeekResult<Tuple>(seek.ResultType, result);
    }

    /// <inheritdoc/>
    public IIndexReader<Tuple, Tuple> CreateReader(Range<Entire<Tuple>> range)
    {
      return new TypedIndexReader(this, range, Source, typeInjector);
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true).Select(typeInjector);
    }

    protected override void Initialize()
    {
      base.Initialize();
      var transform = new CutInTransform<int>(true, typeIdColumn, Source.Header.TupleDescriptor);
      typeInjector = t => transform.Apply(TupleTransformType.Auto, t, typeId);
    }


    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider{TOrigin}.Origin"/> property value.</param>
    /// <param name="provider">Source executable provider.</param>
    /// <param name="typeIdColumn">Index of typeId column.</param>
    /// <param name="typeId">Identifiers of the type.</param>
    public TypedIndexProvider(Compilable.IndexProvider origin, ExecutableProvider provider, int typeIdColumn, int typeId)
      : base(origin, provider)
    {
      AddService<IOrderedEnumerable<Tuple, Tuple>>();
      AddService<ICountable>();
      this.typeIdColumn = typeIdColumn;
      this.typeId = typeId;
//      source = provider.GetService<IOrderedEnumerable<Tuple,Tuple>>(true);
    }
  }
}