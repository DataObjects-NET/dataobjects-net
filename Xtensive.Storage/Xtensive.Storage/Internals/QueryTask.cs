// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  public sealed class QueryTask
  {
    public readonly ExecutableProvider DataSource;

    public readonly ParameterContext ParameterContext;

    public List<Tuple> Result { get; set; }


    // Constructors

    public QueryTask(ExecutableProvider dataSource, ParameterContext parameterContext)
    {
      ArgumentValidator.EnsureArgumentNotNull(dataSource, "dataSource");
      DataSource = dataSource;
      ParameterContext = parameterContext;
    }
  }
}