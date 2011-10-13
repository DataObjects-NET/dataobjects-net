// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.13

using System;
using System.Diagnostics;
using System.Text;
using Xtensive.Core;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class PartialIndexFilterInfo : IEquatable<PartialIndexFilterInfo>
  {
    private readonly string normalizedExpression;

    public string Expression { get; private set; }

    #region Equality members

    public bool Equals(PartialIndexFilterInfo other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Equals(other.normalizedExpression, normalizedExpression);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType() != typeof(PartialIndexFilterInfo))
        return false;
      return Equals((PartialIndexFilterInfo)obj);
    }

    public override int GetHashCode()
    {
      return normalizedExpression.GetHashCode();
    }

    #endregion

    public override string ToString()
    {
      return Expression;
    }

    private static string Normalize(string expression)
    {
      var builder = new StringBuilder(expression);
      builder.Replace(" ", string.Empty);
      builder.Replace("(", string.Empty);
      builder.Replace(")", string.Empty);
      return builder.ToString();
    }

    // Constructors

    public PartialIndexFilterInfo(string expression)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(expression, "expression");
      Expression = expression;
      normalizedExpression = Normalize(expression);
    }
  }
}