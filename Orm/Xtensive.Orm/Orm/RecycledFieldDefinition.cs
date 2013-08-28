// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Andrey Turkov
// Created:    2013.08.21

using System;
using Xtensive.Core;

namespace Xtensive.Orm
{
  /// <summary>
  /// Recycled field
  /// </summary>
  public class RecycledFieldDefinition : RecycledDefinition
  {
    /// <summary>
    /// Owner type with recycled field.
    /// </summary>
    public Type OwnerType { get; private set; }

    /// <summary>
    /// Name of recycled field.
    /// </summary>
    public string FieldName { get; private set; }

    /// <summary>
    /// Type of recycled field.
    /// </summary>
    public Type FieldType { get; private set; }

    /// <summary>
    /// Original field name.
    /// </summary>
    public string OriginalFieldName { get; private set; }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="ownerType">Owner type with recycled field.</param>
    /// <param name="fieldName">Name of recycled field.</param>
    /// <param name="fieldType">Type of recycled field.</param>
    public RecycledFieldDefinition(Type ownerType, string fieldName, Type fieldType)
    {
      SetRequiredFields(ownerType, fieldName, fieldType);
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="ownerType">Owner type with recycled field.</param>
    /// <param name="fieldName">Name of recycled field.</param>
    /// <param name="fieldType">Type of recycled field.</param>
    /// <param name="originalFieldName">Original field name.</param>
    public RecycledFieldDefinition(Type ownerType, string fieldName, Type fieldType, string originalFieldName)
    {
      SetRequiredFields(ownerType, fieldName, fieldType);
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(originalFieldName, "OriginalFieldName");
      OriginalFieldName = originalFieldName;
    }

    private void SetRequiredFields(Type ownerType, string fieldName, Type fieldType)
    {
      ArgumentValidator.EnsureArgumentNotNull(ownerType, "OwnerType");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(fieldName, "FieldName");
      ArgumentValidator.EnsureArgumentNotNull(fieldType, "FieldType");
      OwnerType = ownerType;
      FieldName = fieldName;
      FieldType = fieldType;
    }

    public override string ToString()
    {
      var str = string.Format(
        "Owner {0}, field {1}, type {2}",
        OwnerType,
        FieldName,
        FieldType);
      if (!string.IsNullOrEmpty(OriginalFieldName))
        str += ", original " + OriginalFieldName;
      return str;
    }
  }
}
