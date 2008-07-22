// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider for range operation over the <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public class RangeProvider : UnaryProvider
  {
    /// <summary>
    /// Range parameter.
    /// </summary>
    public Range<IEntire<Tuple>> Range { get; private set; }

    protected override RecordHeader BuildHeader()
    {
      return Source.Header; 
    }

    protected override void Initialize()
    {}

    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RangeProvider(CompilableProvider provider, Range<IEntire<Tuple>> range)
      : base(provider)
    {
      Range = range;
    }
  }
}