// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

namespace Xtensive.Security
{
  /// <summary>
  /// Security token provider.
  /// </summary>
  public interface ISecurityTokenProvider
  {
    /// <summary>
    /// Gets the security token.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <param name="options">The additional options.</param>
    /// <returns>Security token for the specified user and options.</returns>
    string GetSecurityToken(string userName, string options);
  }
}