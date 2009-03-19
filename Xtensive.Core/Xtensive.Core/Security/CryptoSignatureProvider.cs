// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.19

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Security
{
  /// <summary>
  /// An abstract base class for crypto signature providers.
  /// </summary>
  [Serializable]
  public class CryptoSignatureProvider : ISignatureProvider
  {
    private readonly ISignatureProvider signatureProvider;
    private readonly Func<ICryptoTransform> encryptorGenerator;
    private readonly Func<ICryptoTransform> decryptorGenerator;
    private Encoding encoding;

    /// <summary>
    /// Gets or sets the encoding.
    /// </summary>
    protected Encoding Encoding
    {
      get { return encoding; }
      set { encoding = value; }
    }

    /// <inheritdoc/>
    public string AddSignature(string token)
    {
      var signedToken = signatureProvider.AddSignature(token);
      return Encrypt(signedToken);
    }

    /// <inheritdoc/>
    public string RemoveSignature(string signedToken)
    {
      var decrypted = Decrypt(signedToken);
      return signatureProvider.RemoveSignature(decrypted);
    }

    #region Private methods

    private string Encrypt(string value)
    {
      using (var stream = new MemoryStream())
      using (var cryptoStream = new CryptoStream(stream, encryptorGenerator(), CryptoStreamMode.Write))
      using (var writer = new StreamWriter(cryptoStream, Encoding)) {
        writer.Write(value);
        writer.Flush();
        cryptoStream.FlushFinalBlock();
        return Convert.ToBase64String(stream.ToArray());
      }
    }

    private string Decrypt(string value)
    {
      using (var stream = new MemoryStream(Convert.FromBase64String(value)))
      using (var cryptoStream = new CryptoStream(stream, decryptorGenerator(), CryptoStreamMode.Read))
      using (var reader = new StreamReader(cryptoStream, Encoding)) {
        return reader.ReadToEnd();
      }
    }

    #endregion

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="encryptorGenerator">The encryptor generator.</param>
    /// <param name="decryptorGenerator">The decryptor generator.</param>
    /// <param name="signatureProvider">The hashing signature provider.</param>
    public CryptoSignatureProvider(Func<ICryptoTransform> encryptorGenerator,
      Func<ICryptoTransform> decryptorGenerator, ISignatureProvider signatureProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(encryptorGenerator, "encryptorGenerator");
      ArgumentValidator.EnsureArgumentNotNull(decryptorGenerator, "decryptorGenerator");
      ArgumentValidator.EnsureArgumentNotNull(signatureProvider, "hashingSignatureProvider");

      this.encryptorGenerator = encryptorGenerator;
      this.decryptorGenerator = decryptorGenerator;
      this.signatureProvider = signatureProvider;
      Encoding = Encoding.UTF8;
    }

    
  }
}