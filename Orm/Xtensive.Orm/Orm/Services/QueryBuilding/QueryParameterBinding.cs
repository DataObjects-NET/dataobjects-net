// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.26

using System;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Services
{
  public sealed class QueryParameterBinding
  {
    public Type ValueType
    {
      get
      {
        var mapping = RealBinding.TypeMapping;
        return mapping!=null ? mapping.Type : null;
      }
    }

    public Func<object> ValueAccessor
    {
      get { return RealBinding.ValueAccessor; }
    }

    public SqlExpression ParameterReference
    {
      get { return RealBinding.ParameterReference; }
    }

    internal Providers.Sql.QueryParameterBinding RealBinding { get; private set; }

    // Constructors

    internal QueryParameterBinding(Providers.Sql.QueryParameterBinding realBinding)
    {
      RealBinding = realBinding;
    }
  }
}