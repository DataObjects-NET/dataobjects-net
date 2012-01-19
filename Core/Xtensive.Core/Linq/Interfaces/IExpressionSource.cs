// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.28

using System.Linq.Expressions;

namespace Xtensive.Linq
{
  /// <summary>
  /// An object that can be converted to <see cref="Expression"/>.
  /// </summary>
  public interface IExpressionSource
  {
    /// <summary>
    /// Converts the object to expression.
    /// </summary>
    /// <returns>An expression equivalent to (or associated with) this object.</returns>
    Expression ToExpression();
  }
}