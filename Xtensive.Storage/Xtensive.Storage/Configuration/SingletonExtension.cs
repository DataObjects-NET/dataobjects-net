// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.14

using System;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

namespace Xtensive.Storage.Configuration
{
  internal sealed class SingletonExtension : UnityContainerExtension
  {
    private sealed class ContainerLifetimeManager : LifetimeManager
    {
      private object value;
      public override object GetValue()
      {
        return value;
      }

      public override void RemoveValue()
      {}

      public override void SetValue(object newValue)
      {
        value = newValue;
      }
    }

    protected override void Initialize()
    {
      Context.Registering+=OnRegister;
    }

    private void OnRegister(object sender, RegisterEventArgs e)
    {
      if (e.LifetimeManager==null) {
        Type lifetimeType = e.TypeTo ?? e.TypeFrom;
        LifetimeManager lifetimeManager = new ContainerLifetimeManager();
        var typeBuildKey = new NamedTypeBuildKey(lifetimeType, e.Name);
        if (lifetimeType.IsGenericTypeDefinition) {
          var factory = new LifetimeManagerFactory(Context, typeof(ContainerLifetimeManager));
          Context.Policies.Set<ILifetimeFactoryPolicy>(factory, typeBuildKey);
        }
        else
          Context.Policies.Set<ILifetimePolicy>(lifetimeManager, typeBuildKey);
      }
    }
  }
}