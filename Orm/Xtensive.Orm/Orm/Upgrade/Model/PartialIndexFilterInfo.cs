// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.13

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Upgrade.Model
{
  public sealed class PartialIndexFilterInfo : IEquatable<PartialIndexFilterInfo>
  {
    public string Expression { get; }

    #region Equality members

    public bool Equals(PartialIndexFilterInfo other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Equals(other.Expression, Expression);
    }

    public override bool Equals(object obj)
    {
      if (obj is null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType() != typeof(PartialIndexFilterInfo))
        return false;
      return Equals((PartialIndexFilterInfo)obj);
    }

    public override int GetHashCode() => Expression.GetHashCode();

    #endregion

    public override string ToString() => Expression;


    // Constructors

    public PartialIndexFilterInfo(string expression)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(expression, "expression");
      Expression = expression;
    }
  }
}