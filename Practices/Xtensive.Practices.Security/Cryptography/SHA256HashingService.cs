// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System.Security.Cryptography;
using Xtensive.IoC;

namespace Xtensive.Practices.Security.Cryptography
{
  /// <summary>
  /// Implementation of <see cref="IHashingService"/> with SHA256 algorithm.
  /// </summary>
  [Service(typeof (IHashingService), Singleton = true, Name = "sha256")]
  public class SHA256HashingService : GenericHashingService
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SHA256HashingService"/> class.
    /// </summary>
    public SHA256HashingService()
      : base(new SHA256Managed())
    {}
  }
}