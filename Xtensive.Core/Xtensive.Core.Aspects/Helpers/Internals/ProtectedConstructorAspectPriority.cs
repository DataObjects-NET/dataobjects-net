// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.18

using Xtensive.Core.Aspects.Helpers.Internals;

namespace Xtensive.Core.Aspects.Helpers.Internals
{
  /// <summary>
  /// Priorities of aspects applied by <see cref="ProtectedConstructorAspect"/>.
  /// </summary>
  public enum ProtectedConstructorAspectPriority
  {
    /// <summary>
    /// Priority of <see cref="DeclareConstructorAspect"/>.
    /// Value is <see langword="-201" />.
    /// </summary>
    Declare = -201,

    /// <summary>
    /// Priority of <see cref="ImplementProtectedConstructorBodyAspect"/>.
    /// Value is <see langword="-200" />.
    /// </summary>
    ImplementBody = -200, 
  }
}