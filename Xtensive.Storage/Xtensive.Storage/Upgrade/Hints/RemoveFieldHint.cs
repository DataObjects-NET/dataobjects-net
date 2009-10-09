// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Remove field hint.
  /// </summary>
  [Serializable]
  public class RemoveFieldHint : UpgradeHint
  {
    private const string ToStringFormat = "Remove field: {0}.{1}";

    /// <summary>
    /// Gets the source type.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets the source field.
    /// </summary>
    public string Field { get; private set; }

    /// <summary>
    /// Gets affected column paths.
    /// </summary>
    public ReadOnlyList<string> AffectedColumns { get; internal set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, Type, Field);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">Value for <see cref="Type"/>.</param>
    /// <param name="field">Value for <see cref="Field"/>.</param>
    public RemoveFieldHint(string type, string field)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(type, "sourceType");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(field, "sourceField");
      Type = type;
      Field = field;
      AffectedColumns = new ReadOnlyList<string>(new List<string>());
    }
  }
}