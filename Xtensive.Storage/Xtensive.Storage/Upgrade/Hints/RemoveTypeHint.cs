// Copyright (C) 2003-2010 Xtensive LLC.
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
  /// Remove type hint.
  /// </summary>
  [Serializable]
  public class RemoveTypeHint : UpgradeHint
  {
    private const string ToStringFormat = "Remove type: {0}";

    /// <summary>
    /// Gets the source type.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets affected column paths.
    /// </summary>
    public ReadOnlyList<string> AffectedTables { get; internal set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, Type);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">Value for <see cref="Type"/>.</param>
    public RemoveTypeHint(string type)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(type, "sourceType");
      Type = type;
      AffectedTables = new ReadOnlyList<string>(new List<string>());
    }
  }
}