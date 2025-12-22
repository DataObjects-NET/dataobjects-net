// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.12

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class GenericTest
  {
    [HierarchyRoot]
    public class PropertyVersion<T> : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public T PropertyValue { get; set; }
    }

    [Test]
    public void CombinedTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(PropertyVersion<decimal?>));
      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var v = new PropertyVersion<decimal?>() {PropertyValue = 123};
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          var count = session.Query.All<PropertyVersion<decimal?>>().Count();
          Assert.That(count, Is.EqualTo(1));
          var v = session.Query.All<PropertyVersion<decimal?>>().FirstOrDefault();
          Assert.That(v, Is.Not.Null);
          Assert.That(v.PropertyValue, Is.EqualTo(123));
          t.Complete();
        }
      }
    }
  }
}