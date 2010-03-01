// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.06.05

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Change field type enforced (ignore type conversion verification) hint.
  /// </summary>
  [Serializable]
  public sealed class ChangeFieldTypeHint : UpgradeHint
  {
    private const string ToStringFormat = "Change type of field: {0}.{1}";

    /// <summary>
    /// Gets the target type.
    /// </summary>
    public Type Type { get; private set; }

    /// <summary>
    /// Gets the target field name.
    /// </summary>
    public string FieldName { get; private set; }

    /// <summary>
    /// Gets affected column paths.
    /// </summary>
    public ReadOnlyList<string> AffectedColumns { get; internal set; }
    
    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, Type, FieldName);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">Value for <see cref="Type"/>.</param>
    /// <param name="fieldName">Value for <see cref="FieldName"/>.</param>
    public ChangeFieldTypeHint(Type type, string fieldName)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(fieldName, "sourceField");
      Type = type;
      FieldName = fieldName;
      AffectedColumns = new ReadOnlyList<string>(new List<string>());
    }
  }
}