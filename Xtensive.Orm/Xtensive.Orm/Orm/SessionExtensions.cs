// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.10.14

using System.Collections.Generic;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm
{
  /// <summary>
  /// <see cref="Session"/> related extension methods.
  /// </summary>
  public static class SessionExtensions
  {
    /// <summary>
    /// Gets the <see cref="Session.Name"/> of specified 
    /// <see cref="Session"/> safely (with null check).
    /// </summary>
    /// <param name="session">The session to get the name for.</param>
    /// <returns>The name of specified <paramref name="session"/>;
    /// "n\a", if <paramref name="session"/> is <see langword="null" />.</returns>
    public static string GetNameSafely(this Session session)
    {
      return session==null ? Strings.NA : (session.Name ?? Strings.Null);
    }

    /// <summary>
    /// Gets the <see cref="Session.FullName"/> of specified 
    /// <see cref="Session"/> safely (with null check).
    /// </summary>
    /// <param name="session">The session to get the full name for.</param>
    /// <returns>The full name of specified <paramref name="session"/>;
    /// "n\a", if <paramref name="session"/> is <see langword="null" />.</returns>
    public static string GetFullNameSafely(this Session session)
    {
      return session==null ? Strings.NA : session.FullName;
    }
  }
}