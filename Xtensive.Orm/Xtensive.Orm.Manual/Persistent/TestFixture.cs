// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.24


using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Manual.Persistent
{
  [Serializable]
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
    public Byte[] Photo { get; set; }

    [Field]
    public EntitySet<Person> Friends { get; private set; }

    [Field(Length = 100)]
    public string Email
    {
      get { return GetFieldValue<string>("Email"); }
      set
      {
        SetFieldValue("Email", value);
        Mailer.SendEmailChangedNotification(this);
      }
    }
  }

  public static class Mailer
  {
    public static void SendEmailChangedNotification(Person person)
    {
    }
  }

  [TestFixture]
  public class TestFixture
  {
    [Test]
    public void MainTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Person));
      Domain.Build(config);
    }
  }
}