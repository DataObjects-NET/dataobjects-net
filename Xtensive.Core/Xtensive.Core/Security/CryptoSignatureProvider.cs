// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.19

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Security
{
  /// <summary>
  /// Implementation of encrypting signature provider.
  /// </summary>
  [Serializable]
  public class CryptoSignatureProvider : ISignatureProvider
  {
    #region Properties

    /// <summary>
    /// Gets or sets the encoding.
    /// </summary>
    public Encoding Encoding { get; protected set; }

    /// <summary>
    /// Gets or sets the signature provider to use.
    /// </summary>
    public ISignatureProvider SignatureProvider { get; protected set; }

    /// <summary>
    /// Gets or sets the encryptor constructor delegate.
    /// </summary>
    protected Func<ICryptoTransform> EncryptorConstructor { get; set; }

    /// <summary>
    /// Gets or sets the decryptor constructor.
    /// </summary>
    protected Func<ICryptoTransform> DecryptorConstructor { get; set; }

    #endregion

    /// <inheritdoc/>
    public string AddSignature(string token)
    {
      var signedToken = SignatureProvider.AddSignature(token);
      return Encrypt(signedToken);
    }

    /// <inheritdoc/>
    public string RemoveSignature(string signedToken)
    {
      var decrypted = Decrypt(signedToken);
      return SignatureProvider.RemoveSignature(decrypted);
    }

    /// <summary>
    /// Encrypts the specified value.
    /// </summary>
    /// <param name="value">The value to encrypt.</param>
    /// <returns>Encrypted value.</returns>
    protected virtual string Encrypt(string value)
    {
      using (var stream = new MemoryStream())
      using (var cryptoStream = new CryptoStream(stream, EncryptorConstructor(), CryptoStreamMode.Write))
      using (var writer = new StreamWriter(cryptoStream, Encoding)) {
        writer.Write(value);
        writer.Flush();
        cryptoStream.FlushFinalBlock();
        return Convert.ToBase64String(stream.ToArray());
      }
    }

    /// <summary>
    /// Decrypts the specified value.
    /// </summary>
    /// <param name="value">The value to decrypt.</param>
    /// <returns>Decrypted value.</returns>
    protected virtual string Decrypt(string value)
    {
      using (var stream = new MemoryStream(Convert.FromBase64String(value)))
      using (var cryptoStream = new CryptoStream(stream, DecryptorConstructor(), CryptoStreamMode.Read))
      using (var reader = new StreamReader(cryptoStream, Encoding)) {
        return reader.ReadToEnd();
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="encryptorConstructor">The encryptor constructor delegate.</param>
    /// <param name="decryptorConstructor">The decryptor constructor delegate.</param>
    /// <param name="signatureProvider">The signature provider to use.</param>
    public CryptoSignatureProvider(Func<ICryptoTransform> encryptorConstructor,
      Func<ICryptoTransform> decryptorConstructor, ISignatureProvider signatureProvider)
      : this()
    {
      ArgumentValidator.EnsureArgumentNotNull(encryptorConstructor, "encryptorConstructor");
      ArgumentValidator.EnsureArgumentNotNull(decryptorConstructor, "decryptorConstructor");
      ArgumentValidator.EnsureArgumentNotNull(signatureProvider, "hashingSignatureProvider");

      EncryptorConstructor = encryptorConstructor;
      DecryptorConstructor = decryptorConstructor;
      SignatureProvider = signatureProvider;
    }


    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected CryptoSignatureProvider()
    {
      Encoding = Encoding.UTF8;
    }
  }
}