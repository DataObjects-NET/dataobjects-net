// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.23

using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers.Executable.VirtualIndex.Internal
{
  /// <summary>
  /// Base class for <see cref="RecordQuery"/> <see cref="IIndexReader{TKey,TItem}"/>s.
  /// </summary>
  internal abstract class VirtualIndexReader: IIndexReader<Tuple, Tuple>
  {
    private readonly ExecutableProvider provider;
    private Range<Entire<Tuple>> range; // Non-readonly - to avoid stack growth
    private readonly Direction direction;

    /// <summary>
    /// Gets the provider this reader is created for.
    /// </summary>
    public ExecutableProvider Provider
    {
      get { return provider; }
    }

    /// <inheritdoc/>
    public Range<Entire<Tuple>> Range
    {
      get { return range; }
    }

    /// <inheritdoc/>
    public Direction Direction
    {
      get { return direction; }
    }

    /// <inheritdoc/>
    object IEnumerator.Current
    {
      get { return Current; }
    }

    /// <inheritdoc/>
    public abstract Tuple Current { 
      get; 
    }

    /// <inheritdoc/>
    public abstract bool MoveNext();
    
    /// <inheritdoc/>
    public abstract void MoveTo(Entire<Tuple> key);

    /// <inheritdoc/>
    public abstract void Reset();

    #region GetEnumerator<...> methods

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public abstract IEnumerator<Tuple> GetEnumerator();

    #endregion


    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider"><see cref="Provider"/> property value.</param>
    /// <param name="range">The <see cref="Range"/> property value.</param>
    protected VirtualIndexReader(ExecutableProvider provider, Range<Entire<Tuple>> range)
    {
      this.provider = provider;
      this.range = range;
      var hasKeyComparers = provider.GetService<IHasKeyComparers<Tuple>>();
      direction = hasKeyComparers == null ? 
                                            Direction.Positive : 
                                                                 range.GetDirection(hasKeyComparers.EntireKeyComparer);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public abstract void Dispose();
  }
}