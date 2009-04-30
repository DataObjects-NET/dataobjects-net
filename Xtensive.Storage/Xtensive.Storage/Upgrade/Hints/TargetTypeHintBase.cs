// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.30

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Upgrade.Hints
{
  /// <summary>
  /// An abstract base class for upgrade hint 
  /// with <see cref="TargetType"/> property.
  /// </summary>
  [Serializable]
  public abstract class TargetTypeHintBase : UpgradeHint
  {
    /// <summary>
    /// Gets the target type this hint is related to.
    /// </summary>
    public Type TargetType { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="targetType"><see cref="TargetType"/> value.</param>
    protected TargetTypeHintBase(Type targetType)
    {
      ArgumentValidator.EnsureArgumentNotNull(targetType, "targetType");
      TargetType = targetType;
    }
  }
}