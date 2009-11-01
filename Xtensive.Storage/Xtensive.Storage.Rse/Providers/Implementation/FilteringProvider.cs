// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.02

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Implementation
{
  public sealed class FilteringProvider : ProviderImplementation
  {
    private readonly Provider source;
    private readonly Func<Tuple, bool> predicate;

    /// <inheritdoc/>
    public override ProviderOptionsStruct Options
    {
      get { return source.Options.Internal & ~ProviderOptions.FastCount; }
    }

    /// <inheritdoc/>
    public override IEnumerator<Tuple> GetEnumerator()
    {
      return source.Where(predicate).GetEnumerator();
    }

    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    ///<param name="header">Result header.</param>
    ///<param name="source">Source provider.</param>
    ///<param name="predicate">Filtering predicate.</param>
    public FilteringProvider(RecordHeader header, Provider source, Func<Tuple, bool> predicate)
      : base(header, source)
    {
      this.source = source;
      this.predicate = predicate;
    }
  }
}