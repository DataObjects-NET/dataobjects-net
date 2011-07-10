// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System.Security.Cryptography;
using System.Text;
using Xtensive.IoC;

namespace Xtensive.Practices.Security.Cryptography
{
  /// <summary>
  /// Implementation of <see cref="IHashingService"/> with MD5 algorithm.
  /// </summary>
  [Service(typeof (IHashingService), Singleton = true, Name = "md5")]
  public class MD5HashingService : GenericHashingService
  {

    /// <summary>
    /// Initializes a new instance of the <see cref="MD5HashingService"/> class.
    /// </summary>
    public MD5HashingService()
      : base(new MD5CryptoServiceProvider())
    {}
  }
}