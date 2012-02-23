// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.23

using System;
using System.Linq.Expressions;

namespace Xtensive.Orm
{
  public interface IQueryRootBuilder
  {
    Expression GetRootExpression(Type elementType);
  }
}