// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.08

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Configuration.TypeRegistry;
using Xtensive.Storage.Tests.RegistryModel1;

namespace Xtensive.Storage.Tests.RegistryModel1
{
  public class A : Entity
  {
  }

  public class B : A
  {
  }

  public class C
  {
  }

  public class D : A
  {
  }
}

namespace Xtensive.Storage.Tests.RegistryModel2
{
  public class A : Entity
  {
  }

  public class B : A
  {
  }

  public class C
  {
  }

  public class D : A
  {
  }
}

namespace Xtensive.Storage.Tests.Configuration
{
  [TestFixture]
  public class RegistryTest
  {
    [Test]
    public void HierarchyTest()
    {
      DomainConfiguration config = new DomainConfiguration();
      Registry registry = config.Types;
      registry.Register(typeof (A).Assembly, "Xtensive.Storage.Tests.RegistryModel1");
      Assert.IsTrue(registry.Contains(typeof (A)));
      Assert.IsTrue(registry.Contains(typeof (B)));
      Assert.IsFalse(registry.Contains(typeof (C)));
      Assert.IsTrue(registry.Contains(typeof (D)));

      config = new DomainConfiguration();
      registry = config.Types;
      registry.Register(typeof (B).Assembly, "Xtensive.Storage.Tests.RegistryModel1");
      Assert.IsTrue(registry.Contains(typeof (A)));
      Assert.IsTrue(registry.Contains(typeof (B)));
      Assert.IsFalse(registry.Contains(typeof (C)));
      Assert.IsTrue(registry.Contains(typeof (D)));
    }

    [Test]
    public void ContiniousRegistrationTest()
    {
      DomainConfiguration config = new DomainConfiguration();
      Registry registry = config.Types;
      registry.Register(typeof (A).Assembly, "Xtensive.Storage.Tests.RegistryModel1");
      long amount = registry.Count;
      registry.Register(typeof (A).Assembly, "Xtensive.Storage.Tests.RegistryModel2");
      Assert.Less(amount, registry.Count);
    }

    [Test]
    public void CloneTest()
    {
      DomainConfiguration config = new DomainConfiguration();
      Registry registry1 = config.Types;
      registry1.Register(typeof (A).Assembly, "Xtensive.Storage.Tests.RegistryModel1");
      Registry registry2 = registry1.Clone() as Registry;
      Assert.IsNotNull(registry2);
      Assert.AreEqual(registry1.Count, registry2.Count);
    }

    [Test]
    public void NamespaceFilterTest()
    {
      DomainConfiguration config = new DomainConfiguration();
      Registry registry = config.Types;
      registry.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.RegistryModel2");
      Assert.IsFalse(registry.Contains(typeof (A)));
      Assert.IsFalse(registry.Contains(typeof (B)));
      Assert.IsFalse(registry.Contains(typeof (C)));
      Assert.IsFalse(registry.Contains(typeof (D)));
      Assert.IsTrue(registry.Contains(typeof (RegistryModel2.A)));
      Assert.IsTrue(registry.Contains(typeof (RegistryModel2.B)));
      Assert.IsFalse(registry.Contains(typeof (RegistryModel2.C)));
      Assert.IsTrue(registry.Contains(typeof (RegistryModel2.D)));
    }

    [Test]
    public void InvalidRegistrationTest()
    {
      DomainConfiguration config = new DomainConfiguration();
      Registry registry = config.Types;
      AssertEx.ThrowsArgumentNullException(() => registry.Register(null));
      AssertEx.ThrowsArgumentNullException(() => registry.Register(null, "Xtensive.Storage.Tests.RegistryModel1"));
      AssertEx.ThrowsArgumentException(() => registry.Register(Assembly.GetExecutingAssembly(), ""));
    }
  }
}