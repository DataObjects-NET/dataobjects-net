// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System.Security.Cryptography;
using Xtensive.IoC;

namespace Xtensive.Practices.Security.Cryptography
{
  [Service(typeof (IHashingService), Singleton = true, Name = "sha256")]
  public class SHA256HashingService : GenericHashingService
  {
    public SHA256HashingService()
      : base(new SHA256Managed())
    {}
  }
}