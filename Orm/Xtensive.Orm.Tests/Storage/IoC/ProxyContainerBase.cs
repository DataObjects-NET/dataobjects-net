// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.11

using System;
using System.Collections.Generic;
using Xtensive.IoC;

namespace Xtensive.Orm.Tests.Storage.IoC
{
  public abstract class ProxyContainerBase : ServiceContainerBase
  {
    public IServiceContainer RealContainer = null;

    protected override IEnumerable<object> HandleGetAll(Type serviceType)
    {
      return RealContainer.GetAll(serviceType);
    }

    protected override object HandleGet(Type serviceType, string name)
    {
      return RealContainer.Get(serviceType, name);
    }

    public ProxyContainerBase(object configuration, IServiceContainer parent)
      : base(parent)
    {
    }
  }
}