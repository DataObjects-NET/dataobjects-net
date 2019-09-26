// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System.Security.Cryptography;
using Xtensive.IoC;

namespace Xtensive.Orm.Security.Cryptography
{
  /// <summary>
  /// Implementation of <see cref="IHashingService"/> with SHA512 algorithm.
  /// </summary>
  [Service(typeof (IHashingService), Singleton = true, Name = "sha512")]
  public class SHA512HashingService : GenericHashingService
  {
    /// <inheritdoc/>
    protected override HashAlgorithm GetHashAlgorithm()
    {
      return new SHA512Managed();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SHA512HashingService"/> class.
    /// </summary>
    public SHA512HashingService()
    {
    }
  }
}