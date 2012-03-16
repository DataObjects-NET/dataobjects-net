// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.24

using System;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Resources;

namespace Xtensive.Serialization
{
  /// <summary>
  /// <see cref="IReference"/> related extension methods.
  /// </summary>
  public static class ReferenceExtensions
  {
    /// <summary>
    /// Determines whether the specified reference is pointing to <see langword="null" />.
    /// </summary>
    /// <param name="reference">The reference to check.</param>
    /// <returns>
    /// <see langword="True"/> if the specified reference is pointing to <see langword="null" />; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsNull(this IReference reference)
    {
      return reference==null || reference.Value.IsNullOrEmpty();
    }

    /// <summary>
    /// Ensures the reference is not <see cref="IsNull"/>.
    /// </summary>
    /// <param name="reference">The reference to check.</param>
    /// <exception cref="InvalidOperationException"><see cref="IsNull"/> returns <see langword="true" /> 
    /// for the specified reference.</exception>
    public static void EnsureNotNull(this IReference reference)
    {
      if (reference.IsNull())
        throw new InvalidOperationException(Strings.ExReferenceIsNull);
    }

    /// <summary>
    /// Determines whether the specified reference is already resolved.
    /// </summary>
    /// <param name="reference">The reference to check.</param>
    /// <returns>
    /// <see langword="True"/> if the specified reference is already resolved; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsResolved(this IReference reference)
    {
      object target;
      return reference.TryResolve(out target);
    }

    /// <summary>
    /// Resolves the specified reference.
    /// </summary>
    /// <param name="reference">The reference to resolve.</param>
    /// <returns>The result of resolution (reference target).</returns>
    /// <exception cref="InvalidOperationException">The reference can't be resolved yet.</exception>
    public static object Resolve(this IReference reference) 
    {
      object target;
      if (!reference.TryResolve(out target)) 
        throw new InvalidOperationException(string.Format(
          Strings.ExReferenceIsNotResolvedYet, 
          reference));
      return target;
    }

    /// <summary>
    /// The typed version of <see cref="Resolve"/>.
    /// </summary>
    /// <typeparam name="T">Type of the object to resolve the reference to.</typeparam>
    /// <param name="reference">The reference to resolve.</param>
    /// <returns>The <see cref="Resolve"/> result.</returns>
    public static T Resolve<T>(this IReference reference)
    {
      return (T) reference.Resolve();
    }
  }
}