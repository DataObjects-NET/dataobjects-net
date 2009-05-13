// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="ParameterExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableParameterExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="ParameterExpression.Name"/>.
    /// </summary>
    public string Name;
  }
}