// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System.Security.Cryptography;
using System.Text;
using Xtensive.IoC;

namespace Xtensive.Practices.Security.Encryption
{
  [Service(typeof (IEncryptionService), Singleton = true)]
  public class Md5EncryptionService : IEncryptionService
  {
    public string Encrypt(string value)
    {
      var md5Hasher = new MD5CryptoServiceProvider();
      var data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(value));
      var sBuilder = new StringBuilder();

      for (int i = 0; i < data.Length; i++)
        sBuilder.Append(data[i].ToString("x2"));

      return sBuilder.ToString();
    }
  }
}