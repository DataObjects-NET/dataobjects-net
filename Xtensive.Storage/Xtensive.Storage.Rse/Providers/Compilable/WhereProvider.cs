// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that declares filtering operation over the <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public class WhereProvider : UnaryProvider
  {
    /// <summary>
    /// Filtering predicate expression.
    /// </summary>
    public Expression<Func<Tuple, bool>> Predicate { get; private set; }

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
    public WhereProvider(CompilableProvider source, Expression<Func<Tuple, bool>> predicate)
      : base(source)
    {
      Predicate = predicate;
    }
  }
}