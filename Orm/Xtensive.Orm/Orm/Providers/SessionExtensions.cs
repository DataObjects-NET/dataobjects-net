// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.10.14

using System;
using Xtensive.Orm.Operations;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// <see cref="Session"/> related extension methods.
  /// </summary>
  public static class SessionExtensions
  {
    /// <summary>
    /// Gets the string representation of specified 
    /// <see cref="Session"/> safely (with null check).
    /// </summary>
    /// <param name="session">The session to get the string representation for.</param>
    /// <returns>The string representation of specified <paramref name="session"/>;
    /// "n\a", if <paramref name="session"/> is <see langword="null" />.</returns>
    public static string ToStringSafely(this Session session)
    {
      return session==null ? Strings.NA : session.ToString();
    }

    internal static (IOperationRegistry operations, IOperationFactory operationsFactory, bool allowedRegistration) GetOperationsContext(this Session session)
    {
      var operations = session.Operations;
      var allowRegistration = operations.IsRegistrationEnabled;
      IOperationFactory operationsFactory = null;
      if (allowRegistration) {
        operationsFactory = session.Domain.Services.Get<IOperationFactory>();
        if (operationsFactory is null)
          throw new InvalidOperationException("Factory of operations can't be found.");
      }

      return(operations, operationsFactory, allowRegistration);
    }
  }
}