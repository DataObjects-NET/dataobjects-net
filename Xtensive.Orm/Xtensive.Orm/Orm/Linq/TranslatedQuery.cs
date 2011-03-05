// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.27

using System;
using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Xtensive.Storage.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Storage.Rse;

namespace Xtensive.Orm.Linq
{
  /// <summary>
  /// Abstract base class describing LINQ query translation result.
  /// </summary>
  public abstract class TranslatedQuery
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="dataSource">The <see cref="DataSource"/> property value.</param>
    protected TranslatedQuery(ExecutableProvider dataSource)
    {
      DataSource = dataSource;
    }
  }
}