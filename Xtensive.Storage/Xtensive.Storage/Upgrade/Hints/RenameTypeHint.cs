// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.29

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Rename type hint.
  /// </summary>
  [Serializable]
  public sealed class RenameTypeHint : UpgradeHint
  {
    private const string ToStringFormat = "Rename type: {0} -> {1}";

    /// <summary>
    /// Gets the new type.
    /// </summary>
    public Type NewType { get; private set; }

    /// <summary>
    /// Gets the name of old type.
    /// </summary>
    public string OldType { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, OldType, NewType.GetFullName());
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="oldType">The old type.</param>
    /// <param name="newType">The new type.</param>
    public RenameTypeHint(string oldType, Type newType)
    {
      ArgumentValidator.EnsureArgumentNotNull(newType, "newType");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(oldType, "oldType");

      if (!oldType.Contains("."))
        oldType = newType.Namespace + "." + oldType;
      OldType = oldType;
      NewType = newType;
    }
  }
}