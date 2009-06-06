// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.27

using System;
using System.Diagnostics;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal abstract class TranslatedQuery
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