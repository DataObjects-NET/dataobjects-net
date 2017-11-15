// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.08

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.RegistryModel1;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.RegistryModel1
{
  [Serializable]
  public class A : Entity
  {
  }

  [Serializable]
  public class B : A
  {
  }

  [Serializable]
  public class C
  {
  }

  [Serializable]
  public class D : A
  {
  }
}

namespace Xtensive.Orm.Tests.RegistryModel2
{
  [Serializable]
  public class A : Entity
  {
  }

  [Serializable]
  public class B : A
  {
  }

  [Serializable]
  public class C
  {
  }

  [Serializable]
  public class D : A
  {
  }
}

namespace Xtensive.Orm.Tests.Configuration
{
  [TestFixture]
  public class TypeRegistryTest
  {
    [Test]
    public void HierarchyTest()
    {
      var config = new DomainConfiguration();
      var typeRegistry = config.Types;
      typeRegistry.Register(typeof (A).Assembly, "Xtensive.Orm.Tests.RegistryModel1");
      Assert.IsTrue(typeRegistry.Contains(typeof (A)));
      Assert.IsTrue(typeRegistry.Contains(typeof (B)));
      Assert.IsFalse(typeRegistry.Contains(typeof (C)));
      Assert.IsTrue(typeRegistry.Contains(typeof (D)));

      config = new DomainConfiguration();
      typeRegistry = config.Types;
      typeRegistry.Register(typeof (B).Assembly, "Xtensive.Orm.Tests.RegistryModel1");
      Assert.IsTrue(typeRegistry.Contains(typeof (A)));
      Assert.IsTrue(typeRegistry.Contains(typeof (B)));
      Assert.IsFalse(typeRegistry.Contains(typeof (C)));
      Assert.IsTrue(typeRegistry.Contains(typeof (D)));
    }

    [Test]
    public void ContiniousRegistrationTest()
    {
      var config = new DomainConfiguration();
      TypeRegistry typeRegistry = config.Types;
      typeRegistry.Register(typeof (A).Assembly, "Xtensive.Orm.Tests.RegistryModel1");
      long amount = typeRegistry.Count;
      typeRegistry.Register(typeof (A).Assembly, "Xtensive.Orm.Tests.RegistryModel2");
      Assert.Less(amount, typeRegistry.Count);
    }

    [Test]
    public void CloneTest()
    {
      var config = new DomainConfiguration();
      TypeRegistry registry1 = config.Types;
      registry1.Register(typeof (A).Assembly, "Xtensive.Orm.Tests.RegistryModel1");
      var registry2 = registry1.Clone() as TypeRegistry;
      Assert.IsNotNull(registry2);
      Assert.AreEqual(registry1.Count, registry2.Count);
    }

    [Test]
    public void NamespaceFilterTest()
    {
      var config = new DomainConfiguration();
      var typeRegistry = config.Types;
      typeRegistry.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.RegistryModel2");
      Assert.IsFalse(typeRegistry.Contains(typeof (A)));
      Assert.IsFalse(typeRegistry.Contains(typeof (B)));
      Assert.IsFalse(typeRegistry.Contains(typeof (C)));
      Assert.IsFalse(typeRegistry.Contains(typeof (D)));
      Assert.IsTrue(typeRegistry.Contains(typeof (RegistryModel2.A)));
      Assert.IsTrue(typeRegistry.Contains(typeof (RegistryModel2.B)));
      Assert.IsFalse(typeRegistry.Contains(typeof (RegistryModel2.C)));
      Assert.IsTrue(typeRegistry.Contains(typeof (RegistryModel2.D)));
    }

    [Test]
    public void InvalidRegistrationTest()
    {
      var config = new DomainConfiguration();
      var types = config.Types;
      AssertEx.ThrowsArgumentNullException(() => types.Register((Assembly) null));
      AssertEx.ThrowsArgumentNullException(() => types.Register((Assembly) null, "Xtensive.Orm.Tests.RegistryModel1"));
      AssertEx.ThrowsArgumentException(() => types.Register(Assembly.GetExecutingAssembly(), ""));
    }
  }
}