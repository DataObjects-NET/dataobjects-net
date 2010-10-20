// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Security
{
  /// <summary>
  /// An implementation of authentication service.
  /// </summary>
  [Serializable]
  public class AuthenticationService : MarshalByRefObject,
    IAuthenticationService
  {
    /// <summary>
    /// Gets or sets the underlying authentication provider.
    /// </summary>
    protected IAuthenticationProvider AuthenticationProvider { get; set; }

    /// <summary>
    /// Gets or sets the underlying security token provider.
    /// </summary>
    protected ISecurityTokenProvider SecurityTokenProvider { get; set; }

    /// <summary>
    /// Gets or sets the underlying signature provider.
    /// </summary>
    protected ISignatureProvider SignatureProvider { get; set; }

    /// <inheritdoc/>
    public string Authenticate(string userName, string password)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(userName, "userName");
      if (password==null)
        password = string.Empty;

      if (!AuthenticationProvider.Authenticate(userName, password))
        return null;
      string securityToken = SecurityTokenProvider.GetSecurityToken(userName, password);
      if (securityToken.IsNullOrEmpty())
        return null;
      return 
        SignatureProvider.AddSignature(
          securityToken);

    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="authenticationProvider">The authentication provider.</param>
    /// <param name="securityTokenProvider">The security token provider.</param>
    /// <param name="signatureProvider">The signature provider.</param>
    public AuthenticationService(IAuthenticationProvider authenticationProvider, 
      ISecurityTokenProvider securityTokenProvider, ISignatureProvider signatureProvider)
    {
      AuthenticationProvider = authenticationProvider;
      SignatureProvider = signatureProvider;
      SecurityTokenProvider = securityTokenProvider;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="authenticateHandler">The <see cref="IAuthenticationProvider.Authenticate"/> handler.</param>
    /// <param name="getSecurityTokenHandler">The <see cref="ISecurityTokenProvider.GetSecurityToken"/> handler.</param>
    /// <param name="signatureProvider">The signature provider.</param>
    public AuthenticationService(Func<string, string, bool> authenticateHandler, 
      Func<string, string, string> getSecurityTokenHandler, ISignatureProvider signatureProvider)
    {
      AuthenticationProvider = new DelegateAuthenticationProvider(authenticateHandler);
      SecurityTokenProvider = new DelegateSecurityTokenProvider(getSecurityTokenHandler);
      SignatureProvider = signatureProvider;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected AuthenticationService()
    {
    }
  }
}