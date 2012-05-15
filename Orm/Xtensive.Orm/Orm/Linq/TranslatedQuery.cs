// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.27

using System;
using System.Collections.Generic;

using Xtensive.Tuples;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Rse;

namespace Xtensive.Orm.Linq
{
  /// <summary>
  /// Abstract base class describing LINQ query translation result.
  /// </summary>
  internal abstract class TranslatedQuery
  {
    /// <summary>
    /// The <see cref="ExecutableProvider"/> acting as source for further materialization.
    /// </summary>
    public readonly ExecutableProvider DataSource;

    /// <summary>
    /// Gets the untyped materializer.
    /// </summary>
    public abstract Delegate UntypedMaterializer { get; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="dataSource">The <see cref="DataSource"/> property value.</param>
    protected TranslatedQuery(ExecutableProvider dataSource)
    {
      DataSource = dataSource;
    }
  }
}