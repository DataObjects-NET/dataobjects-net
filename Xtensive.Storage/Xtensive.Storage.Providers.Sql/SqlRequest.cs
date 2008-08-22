// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.22

using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlRequest
  {
    public ISqlCompileUnit Statement { get; private set; }

    public List<SqlParameter> Parameters { get; private set; }

    public TupleDescriptor TupleDescriptor { get; private set; }


    // Constructor

    public SqlRequest(ISqlCompileUnit statement, TupleDescriptor tupleDescriptor)
    {
      Statement = statement;
      TupleDescriptor = tupleDescriptor;
      Parameters = new List<SqlParameter>();
    }
  }
}