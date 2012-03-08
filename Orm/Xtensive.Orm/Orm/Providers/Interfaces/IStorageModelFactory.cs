// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.07

using Xtensive.Orm.Upgrade.Model;

namespace Xtensive.Orm.Providers.Interfaces
{
  public interface IStorageModelFactory
  {
    StorageModel CreateEmptyStorageModel();
  }
}