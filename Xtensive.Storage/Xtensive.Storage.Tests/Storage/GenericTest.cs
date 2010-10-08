// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.12

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class GenericTest
  {
    private Domain domain;

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
      if (domain != null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(PropertyVersion<decimal?>));
      domain = Domain.Build(configuration);
      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var v = new PropertyVersion<decimal?>() {PropertyValue = 123};
          t.Complete();
        }
        using (var t = Transaction.Open()) {
          var count = Query.All<PropertyVersion<decimal?>>().Count();
          Assert.AreEqual(1, count);
          var v = Query.All<PropertyVersion<decimal?>>().FirstOrDefault();
          Assert.IsNotNull(v);
          Assert.AreEqual(123, v.PropertyValue);
          t.Complete();
        }
      }

    }
  }
}