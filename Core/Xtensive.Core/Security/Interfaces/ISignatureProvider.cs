// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

namespace Xtensive.Security
{
  /// <summary>
  /// Digital signature provider.
  /// </summary>
  public interface ISignatureProvider
  {
    /// <summary>
    /// Signs (and normally, encrypts) the specified token.
    /// </summary>
    /// <param name="token">The token to sign;
    /// it can't be <see langword="null" /> or an empty string.</param>
    /// <returns>Signed (and possibly, encrypted) token.</returns>
    string AddSignature(string token);

    /// <summary>
    /// Checks the specified signed token.
    /// </summary>
    /// <param name="signedToken">The signed token to check.</param>
    /// <returns>Original (decrypted) token, if check has been passed;
    /// otherwise, <see langword="null" />.</returns>
    string RemoveSignature(string signedToken);
  }
}