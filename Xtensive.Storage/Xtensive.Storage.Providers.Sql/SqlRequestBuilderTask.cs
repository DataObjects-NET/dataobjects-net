// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.29

using System.Collections;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Task for <see cref="SqlRequestBuilder"/>.
  /// </summary>
  public sealed class SqlRequestBuilderTask
  {
    private int hashCode;

    /// <summary>
    /// Gets the type.
    /// </summary>
    public TypeInfo Type { get; private set; }

    /// <summary>
    /// Gets the field map that describes updated fields.
    /// </summary>
    public BitArray FieldMap { get; private set; }

    /// <summary>
    /// Gets the <see cref="SqlUpdateRequestKind"/>.
    /// </summary>
    /// <value></value>
    public SqlUpdateRequestKind Kind { get; private set; }

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

    internal SqlRequestBuilderTask(SqlUpdateRequestKind kind, TypeInfo type, BitArray fieldMap)
      : this(kind, type)
    {
      FieldMap = fieldMap;
    }

    internal SqlRequestBuilderTask(SqlUpdateRequestKind kind, TypeInfo type)
    {
      Kind = kind;
      Type = type;
    }
  }
}