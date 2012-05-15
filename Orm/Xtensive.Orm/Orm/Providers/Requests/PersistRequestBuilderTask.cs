// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.29

using System.Collections;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A task for <see cref="PersistRequestBuilder"/>.
  /// </summary>
  public sealed class PersistRequestBuilderTask
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
    /// Gets the <see cref="PersistRequestKind"/>.
    /// </summary>
    /// <value></value>
    public PersistRequestKind Kind { get; private set; }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj==null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      var other = obj as PersistRequestBuilderTask;
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

    internal PersistRequestBuilderTask(PersistRequestKind kind, TypeInfo type, BitArray fieldMap)
      : this(kind, type)
    {
      FieldMap = fieldMap;
    }

    internal PersistRequestBuilderTask(PersistRequestKind kind, TypeInfo type)
    {
      Kind = kind;
      Type = type;
    }
  }
}