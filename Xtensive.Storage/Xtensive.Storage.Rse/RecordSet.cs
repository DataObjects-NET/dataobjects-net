// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.08.22

using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Declaration;

namespace Xtensive.Storage.Rse
{
  public sealed class RecordSet : IEnumerable<Tuple>
  {
    private readonly CompilableProvider provider;

    /// <inheritdoc/>
    public RecordHeader Header
    {
      get { return Provider.Header; }
    }

    /// <inheritdoc/>
    public CompilableProvider Provider
    {
      get { return provider; }
    }

    /// <inheritdoc/>
    public IEnumerator<Tuple> GetEnumerator()
    {
      return provider.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    internal RecordSet(CompilableProvider provider)
    {
      this.provider = provider;
    }
  }
}