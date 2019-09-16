// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.10

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Xtensive.Orm.Security.Cryptography
{
  /// <summary>
  /// Generic <see cref="IHashingService"/> implementation.
  /// </summary>
  public abstract class GenericHashingService : IHashingService
  {
    /// <summary>
    /// The size of salt.
    /// </summary>
    public const int SaltSize = 8;

    /// <summary>
    /// Gets the hash algorithm.
    /// </summary>
    /// <returns>Hash algorithm to use.</returns>
    protected abstract HashAlgorithm GetHashAlgorithm();

    /// <summary>
    /// Gets the salt.
    /// </summary>
    /// <returns>The salt.</returns>
    protected byte[] GetSalt()
    {
      var salt = new byte[SaltSize];
      using (var rng = new RNGCryptoServiceProvider()) {
        rng.GetBytes(salt);
        return salt;
      }
    }

    /// <summary>
    /// Computes the hash.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <param name="salt">The salt.</param>
    /// <returns>String representation of hash.</returns>
    protected string ComputeHash(byte[] password, byte[] salt)
    {
      using (var hasher = GetHashAlgorithm()) {
        var hash = hasher.ComputeHash(salt.Concat(password).ToArray());
        return Convert.ToBase64String(hash.Concat(salt).ToArray());
      }
    }

    #region IHashingService Members

    /// <inheritdoc/>
    public string ComputeHash(string password)
    {
      return ComputeHash(Encoding.UTF8.GetBytes(password), GetSalt());
    }

    /// <inheritdoc/>
    public bool VerifyHash(string password, string hash)
    {
      byte[] source;
      try {
        source = Convert.FromBase64String(hash);
      }
      catch (FormatException) {
        return false;
      }

      int hashSize;
      using (var hasher = GetHashAlgorithm())
        hashSize = hasher.HashSize / 8;

      if (source.Length < hashSize)
        return false;

      var salt = source.Skip(hashSize).ToArray();
      var currentHash = ComputeHash(Encoding.UTF8.GetBytes(password), salt);
      return StringComparer.Ordinal.Compare(hash, currentHash)==0;
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericHashingService"/> class.
    /// </summary>
    protected GenericHashingService()
    {
    }
  }
}