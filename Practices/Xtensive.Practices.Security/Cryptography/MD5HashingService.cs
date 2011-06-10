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
  [Service(typeof (IHashingService), Singleton = true, Name = "md5")]
  public class MD5HashingService : GenericHashingService
  {
    public MD5HashingService()
      : base(new MD5CryptoServiceProvider())
    {}
  }
}