// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.11

using System.Collections.Generic;
using Xtensive.Sql.Dom;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlScalarRequest : SqlRequest
  {
    public override List<SqlParameter> GetParameters()
    {
      return null;
    }


    public SqlScalarRequest(ISqlCompileUnit statement)
      : base(statement)
    {
    }
  }
}