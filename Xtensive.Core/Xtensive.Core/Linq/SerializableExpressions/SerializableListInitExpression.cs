// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.Linq.Expressions;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="ListInitExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableListInitExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="ListInitExpression.NewExpression"/>
    /// </summary>
    public SerializableNewExpression NewExpression;
    /// <summary>
    /// <see cref="ListInitExpression.Initializers"/>
    /// </summary>
    public SerializableElementInit[] Initializers;
  }
}