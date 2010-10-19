// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

namespace Xtensive.Security
{
  /// <summary>
  /// Underlying authentication provider.
  /// </summary>
  public interface IAuthenticationProvider
  {
    /// <summary>
    /// Authenticates the specified user.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <param name="password">The password.</param>
    /// <returns>
    /// Insecure session token;
    /// <see langword="null"/>, if authentication has failed.
    /// </returns>
    bool Authenticate(string userName, string password);
  }
}