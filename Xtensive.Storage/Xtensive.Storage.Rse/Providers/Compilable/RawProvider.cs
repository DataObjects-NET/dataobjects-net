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
  /// Compilable provider that "delivers" specified 
  /// array of <see cref="Tuple"/> instances.
  /// </summary>
  [Serializable]
  public class RawProvider : CompilableProvider
  {
    private readonly RecordSetHeader header;

    /// <summary>
    /// Source tuples.
    /// </summary>
    public Tuple[] Tuples { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return header;
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RawProvider(RecordSetHeader header, params Tuple[] tuples)
    {
      Tuples = tuples;
      this.header = header;
    }
  }
}