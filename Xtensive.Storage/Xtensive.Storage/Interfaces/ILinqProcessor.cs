// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.15

using System.Linq.Expressions;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  public interface IQueryPreProcessor
  {
    Expression Apply(Expression query);
  }
}