// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using System.Linq.Expressions;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  public class FilterExpression
  {
    public SqlExpression Expression { get; private set; }


    // Constructor

    public FilterExpression(SqlExpression expression)
    {
      Expression = expression;
    }
  }
}