// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.08

using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Implementation
{
  public sealed class RawProvider : ProviderImplementation,
    ISupportRandomAccess<Tuple>
  {
    private readonly Tuple[] tuples;

    /// <inheritdoc/>
    public override ProviderOptionsStruct Options
    {
      get { return ProviderOptions.FastCount | ProviderOptions.FastFirst | ProviderOptions.RandomAccess; }
    }

    /// <inheritdoc/>
    public Tuple this[int index]
    {
      get { return tuples[index]; }
    }

    public override IEnumerator<Tuple> GetEnumerator()
    {
      return ((IEnumerable<Tuple>)tuples).GetEnumerator();
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="header">Record header.</param>
    /// <param name="tuples">Source tuples.</param>
    public RawProvider(RecordHeader header, params Tuple[] tuples)
      : base(header)
    {
      this.tuples = tuples;
    }
  }
}