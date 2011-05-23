// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using Xtensive.IoC;

namespace Xtensive.Practices.Security.Encryption
{
  [Service(typeof(IEncryptionService), Singleton = true)]
  public class PlainEncryptionService : IEncryptionService
  {
    public string Encrypt(string value)
    {
      return value;
    }
  }
}