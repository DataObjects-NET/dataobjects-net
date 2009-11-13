// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlIncludeProvider : SqlProvider
  {

    protected override void OnBeforeEnumerate(Xtensive.Storage.Rse.Providers.EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);

    }

    protected override void OnAfterEnumerate(Xtensive.Storage.Rse.Providers.EnumerationContext context)
    {
      base.OnAfterEnumerate(context);
    }

    // Constructors

    public SqlIncludeProvider(
      HandlerAccessor handlers, QueryRequest request,
      IncludeProvider origin, ExecutableProvider source)
      : base(handlers, request, origin, new []{source})
    {
    }
  }
}