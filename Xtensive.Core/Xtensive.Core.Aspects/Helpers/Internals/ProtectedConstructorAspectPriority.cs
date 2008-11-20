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
    /// Value is <see langword="-220" />.
    /// </summary>
    Declare = -220,

    /// <summary>
    /// Priority of <see cref="ImplementProtectedConstructorBodyAspect"/>.
    /// Value is <see langword="-210" />.
    /// </summary>
    ImplementBody = -210,

    /// <summary>
    /// Priority of <see cref="ImplementProtectedConstructorAccessorAspect"/>.
    /// Value is <see langword="-200" />.
    /// </summary>
    ImplementAccessor = -200,
 
  }
}