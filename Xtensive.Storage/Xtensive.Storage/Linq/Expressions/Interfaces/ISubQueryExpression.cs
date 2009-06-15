// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.06.15

using System.Linq.Expressions;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq.Expressions
{
  internal interface ISubQueryExpression
  {
    ProjectionExpression ProjectionExpression { get; }
    ApplyParameter ApplyParameter { get; }
    Expression ReplaceApplyParameter(ApplyParameter newApplyParameter);
  }
}