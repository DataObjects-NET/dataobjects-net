// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.22

namespace Xtensive.Orm.Providers
{
  public interface IPersistDescriptor
  {
    PersistRequest StoreRequest { get; }

    PersistRequest ClearRequest { get; }
  }
}