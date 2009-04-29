// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.29

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Upgrade.Hints
{
  /// <summary>
  /// Rename type hint.
  /// </summary>
  [Serializable]
  public class RenameTypeHint : UpgradeHint
  {
    /// <summary>
    /// Gets or sets the old type name.
    /// </summary>
    public string OldName { get; private set;}

    /// <summary>
    /// Gets or sets the result type.
    /// </summary>
    public Type TargetType { get; private set; }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="oldName">The old type name.</param>
    /// <param name="resultType">The result type.</param>
    public RenameTypeHint(string oldName, Type resultType)
    {
      OldName = oldName;
      TargetType = resultType;
    }
  }
}