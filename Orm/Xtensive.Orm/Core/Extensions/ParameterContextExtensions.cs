// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.21

namespace Xtensive.Core
{
  /// <summary>
  /// Various extension methods related to this namespace.
  /// </summary>
  public static class ParameterContextExtensions
  {
    /// <summary>
    /// Activates specified <see cref="ParameterContext"/> if it is not null;
    /// otherwise does nothing.
    /// </summary>
    /// <param name="context">The context to activate.</param>
    /// <returns><see cref="ParameterScope"/> if <paramref name="context"/> is not <see langword="null"/>;
    /// otherwise <see langword="null"/>.</returns>
    public static ParameterScope ActivateSafely(this ParameterContext context)
    {
      return context!=null ? context.Activate() : null;
    }
  }
}