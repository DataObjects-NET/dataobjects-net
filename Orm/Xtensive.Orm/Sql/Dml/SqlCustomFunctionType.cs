﻿// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.06

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public sealed class SqlCustomFunctionType : IEquatable<SqlCustomFunctionType>
  {
    public string Name { get; private set; }

    #region Equality members

    public bool Equals(SqlCustomFunctionType other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return string.Equals(Name, other.Name);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return Equals((SqlCustomFunctionType) obj);
    }

    public override int GetHashCode()
    {
      return Name.GetHashCode();
    }

    public static bool operator ==(SqlCustomFunctionType left, SqlCustomFunctionType right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(SqlCustomFunctionType left, SqlCustomFunctionType right)
    {
      return !Equals(left, right);
    }

    #endregion

    // Constructors

    public SqlCustomFunctionType(string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(name, "name");

      Name = name;
    }
  }
}
