// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.12.11

using System.Linq.Expressions;

namespace Xtensive.Orm.Linq.Model
{
  internal sealed class GroupByQuery
  {
    public Expression Source { get; set; }
    public LambdaExpression KeySelector { get; set; }
    public LambdaExpression ElementSelector { get; set; }
    public LambdaExpression ResultSelector { get; set; }
  }
}