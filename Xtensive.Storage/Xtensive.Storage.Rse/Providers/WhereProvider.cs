// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Linq.Expressions;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers
{
  [Serializable]
  public class WhereProvider : CompilableProvider
  {
    public CompilableProvider Source { get; private set; }
    public Expression<Func<Tuple, bool>> Predicate { get; private set; }

    protected override RecordHeader BuildHeader()
    {
      return Source.Header;
    }


    // Constructor

    public WhereProvider(CompilableProvider source, Expression<Func<Tuple, bool>> predicate)
      : base(source)
    {
      Source = source;
      Predicate = predicate;
    }
  }
}