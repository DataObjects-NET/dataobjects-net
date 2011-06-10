// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.10

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Xtensive.Practices.Security.Cryptography
{
  public abstract class GenericHashingService : IHashingService
  {
    public const int SaltSize = 8;

    public HashAlgorithm HashAlgorithm { get; private set; }

    protected byte[] GetSalt()
    {
      var salt = new byte[SaltSize];
      var rng = new RNGCryptoServiceProvider();
      rng.GetNonZeroBytes(salt);
      return salt;
    }

    protected string ComputeHash(byte[] password, byte[] salt)
    {
      var hash = HashAlgorithm.ComputeHash(salt.Concat(password).ToArray());
      return Convert.ToBase64String(hash.Concat(salt).ToArray());
    }

    #region IHashingService Members

    public string ComputeHash(string password)
    {
      return ComputeHash(Encoding.UTF8.GetBytes(password), GetSalt());
    }

    public bool VerifyHash(string password, string hash)
    {
      byte[] source;
      try {
        source = Convert.FromBase64String(hash);
      }
      catch (FormatException) {
        return false;
      }

      int hashSize = HashAlgorithm.HashSize/8;

      if (source.Length < hashSize)
        return false;

      var salt = source.Skip(hashSize).ToArray();
      return StringComparer.Ordinal.Compare(hash, ComputeHash(Encoding.UTF8.GetBytes(password), salt)) == 0;
    }

    #endregion

    protected GenericHashingService(HashAlgorithm hashAlgorithm)
    {
      HashAlgorithm = hashAlgorithm;
    }
  }
}