// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.15

using System;
using Microsoft.Practices.Unity;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  public sealed class ServiceProvider : SessionBound
  {
    private readonly IUnityContainer servicesContainer;

    public TService Get<TService>()
     where TService : class
    {
      return servicesContainer.Resolve<TService>();
    }

    public TService Get<TService>(string name)
      where TService : class
    {
      return servicesContainer.Resolve<TService>(name);
    }

    public object Get(Type type)
    {
      return servicesContainer.Resolve(type);
    }

    public object Get(Type type, string name)
    {
      return servicesContainer.Resolve(type, name);
    }

    /// <inheritdoc/>
    public ServiceProvider(Session session)
      : base(session)
    {
      servicesContainer = session.Domain.Configuration.ServiceContainer.CreateChildContainer();
    }
  }
}