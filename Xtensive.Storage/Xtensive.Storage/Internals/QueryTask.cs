// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  public sealed class QueryTask : IEnumerable<Tuple>
  {
    public readonly ExecutableProvider DataSource;

    public readonly ParameterContext ParameterContext;

    public List<Tuple> Result { get; set; }

    /// <exception cref="InvalidOperationException">Query task is not executed yet.</exception>
    /// <inheritdoc/>
    public IEnumerator<Tuple> GetEnumerator()
    {
      if (Result == null)
        throw new InvalidOperationException("Query task is not executed yet.");
      return Result.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    public QueryTask(ExecutableProvider dataSource, ParameterContext parameterContext)
    {
      ArgumentValidator.EnsureArgumentNotNull(dataSource, "dataSource");
      DataSource = dataSource;
      ParameterContext = parameterContext;
    }
  }
}