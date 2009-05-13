// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="NewExpression"/>
  /// </summary>
  [Serializable]
  public sealed class SerializableNewExpression : SerializableExpression
  {
    /// <summary>
    /// <see cref="NewExpression.Arguments"/>
    /// </summary>
    public SerializableExpression[] Arguments;
    /// <summary>
    /// <see cref="NewExpression.Constructor"/>
    /// </summary>
    public ConstructorInfo Constructor;
    /// <summary>
    /// <see cref="NewExpression.Members"/>
    /// </summary>
    public MemberInfo[] Members;
  }
}