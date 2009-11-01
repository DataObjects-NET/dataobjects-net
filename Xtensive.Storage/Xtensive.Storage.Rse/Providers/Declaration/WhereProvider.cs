// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Linq.Expressions;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Declaration
{
  public class WhereProvider : CompilableProvider
  {
    public CompilableProvider Provider { get; private set; }
    public Expression<Func<Tuple, bool>> Predicate { get; private set; }

    protected override RecordHeader BuildHeader()
    {
      return Provider.Header;
    }


    // Constructor

    public WhereProvider(CompilableProvider provider, Expression<Func<Tuple, bool>> predicate)
      : base(provider)
    {
      Provider = provider;
      Predicate = predicate;
    }
  }
}