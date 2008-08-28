// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.22

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlQueryRequest : SqlRequest
  {
    public Dictionary<SqlParameter, Func<object>> ParameterBindings { get; private set; }

    public TupleDescriptor ElementDescriptor { get; private set; }

    public override List<SqlParameter> GetParameters()
    {
      return ParameterBindings.Keys.ToList();
    }

    public void Bind()
    {
      foreach (KeyValuePair<SqlParameter, Func<object>> binding in ParameterBindings)
        binding.Key.Value = binding.Value.Invoke();
    }


    // Constructor

    public SqlQueryRequest(ISqlCompileUnit statement, TupleDescriptor elementDescriptor)
      : base(statement)
    {
      ParameterBindings = new Dictionary<SqlParameter, Func<object>>();
      ElementDescriptor = elementDescriptor;
    }

    public SqlQueryRequest(ISqlCompileUnit statement, TupleDescriptor elementDescriptor, IEnumerable<KeyValuePair<SqlParameter, Func<object>>> parameteBindings)
      : this(statement, elementDescriptor)
    {
      foreach (var binding in parameteBindings)
        ParameterBindings.Add(binding.Key, binding.Value);
    }

  }
}