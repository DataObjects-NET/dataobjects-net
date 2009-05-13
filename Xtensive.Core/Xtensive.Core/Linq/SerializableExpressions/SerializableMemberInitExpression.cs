// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberInitExpression"/>
  /// </summary>
  [Serializable]
  public sealed class SerializableMemberInitExpression : SerializableExpression
  {
    public SerializableMemberInitExpression()
    {
      throw new NotImplementedException();
    }
  }
}