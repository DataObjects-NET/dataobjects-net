// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.15

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Tests;
using ConfigurationSection=Xtensive.IoC.Configuration.ConfigurationSection;

namespace Xtensive.Orm.Tests.Core.IoC
{
  public interface IPrintService
  {
    void Print(string msg);
  }

  public interface IPrinter
  {
    void Print(string msg);
  }

  public interface ISelfConsumer
  {
  }

  [Service(typeof(IPrintService), Default = true)]
  public class ConsoleService : IPrintService
  {
    public IPrinter Printer { get; private set; }

    public void Print(string msg)
    {
      Console.WriteLine(msg);
    }

    [ServiceConstructor]
    public ConsoleService(IPrinter printer)
    {
      Printer = printer;
    }
  }

  public class DebugService : IPrintService
  {
    public void Print(string msg)
    {
      Debug.WriteLine(msg);
    }
  }

  [Service(typeof(IPrintService), "AutoDebug", Singleton = false)]
  public class NamedAutoDebugService : IPrintService
  {
    public void Print(string msg)
    {
      Debug.WriteLine(msg);
    }
  }

  [Service(typeof(ISelfConsumer))]
  public class SelfConsumer : ISelfConsumer
  {
    [ServiceConstructor]
    public SelfConsumer(ISelfConsumer baseConsumer)
    {
    }
  }

  [TestFixture]
  public class MainTest : HasConfigurationAccessTest
  {
    [Test]
    public void DefaultSectionContainerTest() 
    {
      var defaultSectionContainer = ServiceContainer.Create(Configuration);
      var ps = defaultSectionContainer.Get<IPrintService>();
      Assert.IsNotNull(ps);

      var services = new List<IPrintService>(defaultSectionContainer.GetAll<IPrintService>());
      Assert.AreEqual(1, services.Count);

      var singleton1 = defaultSectionContainer.Get<IPrintService>("Console");
      var singleton2 = defaultSectionContainer.Get<IPrintService>("Console");
      Assert.AreSame(singleton1, singleton2);

      var instance1 = defaultSectionContainer.Get<IPrintService>("AutoDebug");
      var instance2 = defaultSectionContainer.Get<IPrintService>("AutoDebug");
      Assert.AreNotSame(instance1, instance2);
    }

    [Test]
    public void CustomContainerTest()
    { 
      var container = ServiceContainer.Create(Configuration, "second");

      var ps = container.Get<IPrintService>();
      Assert.IsNotNull(ps);

      var services = new List<IPrintService>(container.GetAll<IPrintService>());
      Assert.AreEqual(1, services.Count);

      var singleton1 = container.Get<IPrintService>("Debug");
      var singleton2 = container.Get<IPrintService>("Debug");
      Assert.AreSame(singleton1, singleton2);

      Assert.IsNull(container.Get<IPrintService>("Console"));
    }

    [Test]
    public void SelfConsumerTest()
    {
      var container = ServiceContainer.Create(Configuration);

      AssertEx.Throws<ActivationException>(() => {
        var s = container.Get<ISelfConsumer>();
      });
    }
  }
}