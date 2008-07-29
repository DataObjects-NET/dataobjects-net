// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.01

using Xtensive.Core;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Implemented by objects supporting atomic operations \ atomicity framework.
  /// </summary>
  public interface IAtomicityAware: IContextBound<AtomicityContextBase>
  {
    /// <summary>
    /// Determines whether the specified context is compatible 
    /// with the current object.
    /// </summary>
    /// <param name="context">The context to check for compatibility.</param>
    /// <returns>
    /// <see langword="true"/> if the specified context is compatible
    /// with the current object; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsCompatibleWith(AtomicityContextBase context);
  }
}