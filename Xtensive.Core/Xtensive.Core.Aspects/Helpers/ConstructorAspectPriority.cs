// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.18

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Defines priority of <see cref="ConstructorAspect"/> tasks.
  /// </summary>
  public enum ConstructorAspectPriority
  {
    /// <summary>
    /// Priority of <see cref="DeclareConstructorAspect"/>.
    /// Value is <see langword="-2" />.
    /// </summary>
    Declare = -2,

    /// <summary>
    /// Priority of <see cref="BuildConstructorAspect"/>.
    /// Value is <see langword="-1" />.
    /// </summary>
    Build = -1, 
  }
}