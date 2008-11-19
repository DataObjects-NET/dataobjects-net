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
    /// </summary>
    Declare = 20,

    /// <summary>
    /// Priority of <see cref="BuildConstructorAspect"/>.
    /// </summary>
    Build = 30 
  }
}