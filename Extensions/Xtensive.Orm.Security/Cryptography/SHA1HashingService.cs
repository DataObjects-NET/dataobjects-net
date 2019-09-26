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
  /// Implementation of <see cref="IHashingService"/> with SHA1 algorithm.
  /// </summary>
  [Service(typeof (IHashingService), Singleton = true, Name = "sha1")]
  public class SHA1HashingService : GenericHashingService
  {
    /// <inheritdoc/>
    protected override HashAlgorithm GetHashAlgorithm()
    {
      return new SHA1Managed();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SHA1HashingService"/> class.
    /// </summary>
    public SHA1HashingService()
    {
    }
  }
}