// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.27

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  public abstract class TranslatedQuery
  {
    public readonly RecordSet DataSource;

    public abstract Delegate UntypedMaterializer { get; }

    // Constructors

    protected TranslatedQuery(RecordSet dataSource)
    {
      DataSource = dataSource;
    }
  }
}