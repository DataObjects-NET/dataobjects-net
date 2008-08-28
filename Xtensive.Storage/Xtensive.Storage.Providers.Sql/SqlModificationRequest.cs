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
  public class SqlModificationRequest : SqlRequest
  {
    public override List<SqlParameter> GetParameters()
    {
      return ParameterBindings.Keys.ToList();
    }

    public Dictionary<SqlParameter, Func<Tuple, object>> ParameterBindings { get; private set; }

    public void BindTo(Tuple target)
    {
      foreach (KeyValuePair<SqlParameter, Func<Tuple, object>> binding in ParameterBindings)
        binding.Key.Value = binding.Value.Invoke(target);
    }


    // Constructor

    public SqlModificationRequest(ISqlCompileUnit statement)
      : base(statement)
    {
      ParameterBindings = new Dictionary<SqlParameter, Func<Tuple, object>>();
    }
  }
}