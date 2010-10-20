// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Security
{
  /// <summary>
  /// Delegate-based authentication provider.
  /// </summary>
  [Serializable]
  public sealed class DelegateAuthenticationProvider : IAuthenticationProvider
  {
    private readonly Func<string, string, bool> authenticate;

    /// <inheritdoc/>
    public bool Authenticate(string userName, string password)
    {
      return authenticate.Invoke(userName, password);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="authenticate">The <see cref="Authenticate"/> handler.</param>
    public DelegateAuthenticationProvider(Func<string, string, bool> authenticate)
    {
      ArgumentValidator.EnsureArgumentNotNull(authenticate, "authenticate");

      this.authenticate = authenticate;
    }
  }
}