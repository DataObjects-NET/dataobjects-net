// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.29

using System.Collections;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlRequestBuilderTask
  {
    private int hashCode;

    public TypeInfo Type { get; private set; }

    public BitArray FieldMap { get; private set; }

    public SqlRequestKind Kind { get; private set; }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj==null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      SqlRequestBuilderTask other = obj as SqlRequestBuilderTask;
      if (other == null)
        return false;
      if (Type != other.Type)
        return false;
      if (Kind != other.Kind)
        return false;
      return GetHashCode()==other.GetHashCode();
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      if (hashCode == 0)
        UpdateHashCode();
      return hashCode;
    }

    /// <inheritdoc/>
    public void UpdateHashCode()
    {
      int result = Type.GetHashCode() ^ Kind.GetHashCode();
      if (FieldMap!=null)
        for (int i = 0; i < FieldMap.Count; i++)
          if (FieldMap[i])
            result ^= i;
      hashCode = result;
    }


    // Constructors

    public SqlRequestBuilderTask(SqlRequestKind kind, TypeInfo type, BitArray fieldMap)
      : this(kind, type)
    {
      FieldMap = fieldMap;
    }

    public SqlRequestBuilderTask(SqlRequestKind kind, TypeInfo type)
    {
      Kind = kind;
      Type = type;
    }
  }
}