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
using Xtensive.IoC;
using Xtensive.Testing;
using ConfigurationSection=Xtensive.IoC.Configuration.ConfigurationSection;

namespace Xtensive.Tests.IoC
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
  public class MainTest
  {
    [Test]
    public void DefaultContainerTest()
    {
      var ps = ServiceContainer.Default.Get<IPrintService>();
      Assert.IsNotNull(ps);

      var services = new List<IPrintService>(ServiceContainer.Default.GetAll<IPrintService>());
      Assert.AreEqual(1, services.Count);

      var singleton1 = ServiceContainer.Default.Get<IPrintService>("Console");
      var singleton2 = ServiceContainer.Default.Get<IPrintService>("Console");
      Assert.AreSame(singleton1, singleton2);

      var instance1 = ServiceContainer.Default.Get<IPrintService>("AutoDebug");
      var instance2 = ServiceContainer.Default.Get<IPrintService>("AutoDebug");
      Assert.AreNotSame(instance1, instance2);
    }

    [Test]
    public void CustomContainerTest()
    {
      var config = (Xtensive.IoC.Configuration.ConfigurationSection) ConfigurationManager.GetSection("Xtensive.IoC");
      var container = ServiceContainer.Create("second");

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
      AssertEx.Throws<ActivationException>(() => {
        var s = ServiceContainer.Default.Get<ISelfConsumer>();
      });
    }
  }
}