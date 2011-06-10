// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System;
using Xtensive.IoC;

namespace Xtensive.Practices.Security.Cryptography
{
  [Service(typeof(IHashingService), Singleton = true, Name = "PLAIN")]
  public class PlainHashingService : IHashingService
  {
    public string ComputeHash(string password)
    {
      return password;
    }

    public bool VerifyHash(string password, string hash)
    {
      return StringComparer.Ordinal.Compare(password, hash) == 0;
    }
  }
}