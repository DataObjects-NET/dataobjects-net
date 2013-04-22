// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.29

using System;
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
    public PersistRequestKind Kind { get; private set; }

    /// <summary>
    /// Gets flag indicating if validation should be performed.
    /// </summary>
    public bool ValidateVersion { get; private set; }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj==null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      var other = obj as PersistRequestBuilderTask;
      if (other==null)
        return false;
      if (Type!=other.Type)
        return false;
      if (Kind!=other.Kind)
        return false;
      if (ValidateVersion!=other.ValidateVersion)
        return false;
      if (FieldMap==null && other.FieldMap==null)
        return true;
      if (FieldMap!=null && other.FieldMap!=null && FieldMap.Count==other.FieldMap.Count) {
        for (int i = 0; i < FieldMap.Count; i++)
          if (FieldMap[i]!=other.FieldMap[i])
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

    internal PersistRequestBuilderTask(PersistRequestKind kind, TypeInfo type, BitArray fieldMap, bool validateVersion)
    {
      if (validateVersion && kind==PersistRequestKind.Insert)
        throw new ArgumentException(Strings.ExValidateVersionEqTrueIsIncompatibleWithPersistRequestKindEqInsert);

      Kind = kind;
      Type = type;
      FieldMap = fieldMap;
      ValidateVersion = validateVersion;
    }
  }
}