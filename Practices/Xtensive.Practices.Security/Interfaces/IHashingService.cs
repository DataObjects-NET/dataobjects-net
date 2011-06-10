// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  public interface IHashingService : IDomainService
  {
    string ComputeHash(string password);

    bool VerifyHash(string password, string hash);
  }
}