// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.02

using System;
using NUnit.Framework;
using System.Diagnostics;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Manual.LazyLoading
{
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Name { get; set; }

    [Field]
    public DateTime BirthDay { get; set; }

    [Field(LazyLoad = true, Length = 10000)]
    public byte[] Photo { get; set; }

    [Field]
    public Person Manager { get; private set; }

    [Field]
    [Association(PairTo = "Manager")]
    public EntitySet<Person> Employees { get; private set; }
  }

  [TestFixture]
  public class TestFixture
  {
    [Test]
    public void MainTest()
    {
      var config = new DomainConfiguration("memory://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Person));
      Domain.Build(config);
    }
  }
}