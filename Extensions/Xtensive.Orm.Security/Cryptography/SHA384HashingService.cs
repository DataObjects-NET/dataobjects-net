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
  /// Implementation of <see cref="IHashingService"/> with SHA384 algorithm.
  /// </summary>
  [Service(typeof (IHashingService), Singleton = true, Name = "sha384")]
  public class SHA384HashingService : GenericHashingService
  {
    /// <inheritdoc/>
    protected override HashAlgorithm GetHashAlgorithm()
    {
      return new SHA384Managed();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SHA384HashingService"/> class.
    /// </summary>
    public SHA384HashingService()
    {
    }
  }
}