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
    /// Gets the <see cref="SqlPersistRequestKind"/>.
    /// </summary>
    /// <value></value>
    public SqlPersistRequestKind Kind { get; private set; }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj==null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      var other = obj as SqlRequestBuilderTask;
      if (other == null)
        return false;
      if (Type != other.Type)
        return false;
      if (Kind != other.Kind)
        return false;
      if (GetHashCode() != other.GetHashCode())
        return false;
      if (FieldMap==null && other.FieldMap == null)
        return true;
      if (FieldMap != null && other.FieldMap != null && FieldMap.Count == other.FieldMap.Count) {
        for (int i = 0; i < FieldMap.Count; i++)
          if (FieldMap[i] != other.FieldMap[i])
            return false;
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      if (hashCode==0)
        UpdateHashCode();
      return hashCode;
    }

    private void UpdateHashCode()
    {
      hashCode = Type.GetHashCode() ^ Kind.GetHashCode();
    }
    
    // Constructors

    internal SqlRequestBuilderTask(SqlPersistRequestKind kind, TypeInfo type, BitArray fieldMap)
      : this(kind, type)
    {
      FieldMap = fieldMap;
    }

    internal SqlRequestBuilderTask(SqlPersistRequestKind kind, TypeInfo type)
    {
      Kind = kind;
      Type = type;
    }
  }
}