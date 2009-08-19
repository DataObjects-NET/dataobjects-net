// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.27

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Linq
{
  public abstract class TranslatedQuery
  {
    public readonly IEnumerable<Tuple> DataSource;

    public abstract Delegate UntypedMaterializer { get; }

    // Constructors

    protected TranslatedQuery(IEnumerable<Tuple> dataSource)
    {
      DataSource = dataSource;
    }
  }
}