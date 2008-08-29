// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.09

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Enumerates specified array of <see cref="Tuple"/> instances.
  /// </summary>
  [Serializable]
  public class RawProvider : CompilableProvider
  {
    private readonly RecordSetHeader header;

    /// <summary>
    /// Raw data source - an array of tuples.
    /// </summary>
    public Func<Tuple[]> Source { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return header;
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="header">The <see cref="Provider.Header"/> property value.</param>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    public RawProvider(RecordSetHeader header, Func<Tuple[]> source)
    {
      Source = source;
      this.header = header;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="header">The <see cref="Provider.Header"/> property value.</param>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    public RawProvider(RecordSetHeader header, Tuple[] source)
      : this(header, () => source)
    {
    }
  }
}