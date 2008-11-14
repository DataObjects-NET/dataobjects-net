// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.13

using System;
using Microsoft.Practices.Unity;

namespace Xtensive.Storage
{
  public partial class Session
  {
    private readonly IUnityContainer servicesContainer;

    public TService GetService<TService>()
      where TService : class
    {
      return servicesContainer.Resolve<TService>();
    }

    public TService GetService<TService>(string name)
      where TService : class
    {
      return servicesContainer.Resolve<TService>(name);
    }

    public object GetService(Type type)
    {
      return servicesContainer.Resolve(type);
    }

    public object GetService(Type type, string name)
    {
      return servicesContainer.Resolve(type, name);
    }
  }
}