// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionFactory = System.Linq.Expressions.Expression;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberExpression"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableMemberExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="MemberExpression.Expression"/>
    /// </summary>
    public SerializableExpression Expression;
    /// <summary>
    /// <see cref="MemberExpression.Member"/>
    /// </summary>
    public MemberInfo Member;
  }
}