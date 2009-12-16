// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.15

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using Microsoft.Practices.ServiceLocation;
using NUnit.Framework;
using Xtensive.Core.IoC;
using Xtensive.Core.IoC.Configuration;
using Xtensive.Core.Testing;
using ConfigurationSection=Xtensive.Core.IoC.Configuration.ConfigurationSection;
using ServiceLocator=Xtensive.Core.IoC.ServiceLocator;

namespace Xtensive.Core.Tests.IoC
{
  public interface IPrintService
  {
    void Print(string msg);
  }

  public class ConsoleService : IPrintService
  {
    public void Print(string msg)
    {
      Console.WriteLine(msg);
    }
  }

  public class DebugService : IPrintService
  {
    public void Print(string msg)
    {
      Debug.WriteLine(msg);
    }
  }

  [TestFixture]
  public class MainTest
  {
    [Test]
    public void AmbientTest()
    {
      var ps = ServiceLocator.GetInstance<IPrintService>();
      Assert.IsNotNull(ps);

      var services = new List<IPrintService>(ServiceLocator.GetAllInstances<IPrintService>());
      Assert.AreEqual(2, services.Count);

      var singleton1 = ServiceLocator.GetInstance<IPrintService>("Console");
      var singleton2 = ServiceLocator.GetInstance<IPrintService>("Console");
      Assert.AreSame(singleton1, singleton2);
    }

    [Test]
    public void ContainerTest()
    {
      var config = (ConfigurationSection) ConfigurationManager.GetSection("Xtensive.Core.IoC");
      var container = new ServiceContainer();
      container.Configure(config.Containers["second"]);
      ServiceLocator.SetLocatorProvider(() => new ServiceLocatorAdapter(container));

      var ps = ServiceLocator.GetInstance<IPrintService>();
      Assert.IsNotNull(ps);

      var services = new List<IPrintService>(ServiceLocator.GetAllInstances<IPrintService>());
      Assert.AreEqual(2, services.Count);

      var singleton1 = ServiceLocator.GetInstance<IPrintService>("Debug");
      var singleton2 = ServiceLocator.GetInstance<IPrintService>("Debug");
      Assert.AreSame(singleton1, singleton2);

      // This is standalone container, there is no such types as "console" in it
      AssertEx.Throws<ActivationException>(() => ServiceLocator.GetInstance<IPrintService>("Console"));
    }
  }
}