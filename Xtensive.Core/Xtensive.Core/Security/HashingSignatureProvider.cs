// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Xtensive.Core;
using Xtensive.Helpers;
using System.Linq;
using Xtensive.Internals.DocTemplates;
using Xtensive.Threading;

namespace Xtensive.Security
{
  /// <summary>
  /// Implementation of hashing signature provider.
  /// </summary>
  [Serializable]
  public class HashingSignatureProvider : ISignatureProvider,
    IDeserializationCallback
  {
    [NonSerialized]
    private ThreadSafeCached<HashAlgorithm> cachedHasher;
    [NonSerialized]
    private object _lock = new object();

    #region Properties

    /// <summary>
    /// Gets or sets the hasher constructor delegate.
    /// </summary>
    protected Func<HashAlgorithm> HasherConstructor { get; set; }
    
    /// <summary>
    /// Gets the hasher.
    /// </summary>
    protected HashAlgorithm Hasher {
      get {
        return cachedHasher.GetValue(HasherConstructor);
      }
    }

    /// <summary>
    /// Gets or sets the encoding.
    /// </summary>
    public Encoding Encoding { get; protected set; }

    /// <summary>
    /// Gets or sets the escape character.
    /// </summary>
    public char Escape { get; set; }

    /// <summary>
    /// Gets or sets the delimiter character.
    /// </summary>
    public char Delimiter { get; set; }

    #endregion

    /// <inheritdoc/>
    public string AddSignature(string token)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(token, "token");
      byte[] byteToken = Encoding.GetBytes(token);
      byte[] byteSignature;
      lock (_lock) {
        byteSignature = Hasher.ComputeHash(byteToken);
      }
      return
        new[] {
          Encoding.GetString(byteToken),
          Convert.ToBase64String(byteSignature)
        }.RevertibleJoin(Escape, Delimiter);
    }

    /// <inheritdoc/>
    public string RemoveSignature(string signedToken)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(signedToken, "signedToken");
      string[] parts = signedToken.RevertibleSplit(Escape, Delimiter).ToArray();
      if (parts.Length!=2)
        return null;
      string token = parts[0];
      string signature = parts[1];
      byte[] byteToken = Encoding.GetBytes(token);
      byte[] byteSignature;
      lock (_lock) {
        byteSignature = Hasher.ComputeHash(byteToken);
      }
      if (Convert.ToBase64String(byteSignature)!=signature)
        return null;
      return token;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="hasherConstructor">The <see cref="Hasher"/> constructor delegate.</param>
    public HashingSignatureProvider(Func<HashAlgorithm> hasherConstructor)
      : this()
    {
      ArgumentValidator.EnsureArgumentNotNull(hasherConstructor, "hasherConstructor");
      
      HasherConstructor = hasherConstructor;
      cachedHasher = ThreadSafeCached<HashAlgorithm>.Create(_lock);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected HashingSignatureProvider()
    {
      Encoding = Encoding.UTF8;
      Escape = '\\';
      Delimiter = ',';
    }

    // Deserialization

    /// <inheritdoc/>
    public void OnDeserialization(object sender)
    {
      _lock = new object();
      cachedHasher = ThreadSafeCached<HashAlgorithm>.Create(_lock);
    }
  }
}