// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.15

using System;
using System.Linq.Expressions;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberAssignment"/>
  /// </summary>
  [Serializable]
  public class SerializableMemberAssignment : SerializableMemberBinding
  {
    /// <summary>
    /// <see cref="MemberAssignment.Expression"/>
    /// </summary>
    public SerializableExpression Expression;
  }
}