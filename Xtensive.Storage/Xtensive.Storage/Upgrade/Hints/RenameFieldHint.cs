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
  /// Rename field hint.
  /// </summary>
  [Serializable]
  public class RenameFieldHint : UpgradeHint
  {
    /// <summary>
    /// Gets the target type.
    /// </summary>
    public Type TargetType { get; private set; }

    /// <summary>
    /// Gets the old field name.
    /// </summary>    
    public string OldFieldName { get; set; }

    /// <summary>
    /// Gets new field name.
    /// </summary>
    public string NewFieldName { get; set; }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="targetType">Target type.</param>
    /// <param name="oldFieldName">Old field name.</param>
    /// <param name="newFieldName">New field name.</param>
    public RenameFieldHint(Type targetType, string oldFieldName, string newFieldName)
    {
      TargetType = targetType;
      OldFieldName = oldFieldName;
      NewFieldName = newFieldName;
    }
  }
}