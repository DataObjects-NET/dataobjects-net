// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.20

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.FieldConverterTestModel;

namespace Xtensive.Orm.Tests.Model.FieldConverterTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    private string DateValue { get; set; }

    public DateTime? Date
    {
      get
      {
        if (string.IsNullOrEmpty(DateValue))
        if (string.IsNullOrEmpty(DateValue))
          return null;
        return DateTime.Parse(DateValue);
      }
      set { DateValue = value.ToString(); }
    }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  public class FieldConverterTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        Key key = null;
        var dateTime = new DateTime(2000, 01, 07);
        using (var t = session.OpenTransaction()) {

          var person = new Person();
          key = person.Key;
          Assert.IsNull(person.Date);
          person.Date = dateTime;

          t.Complete();
        }

        using (var t = session.OpenTransaction()) {

          var person = session.Query.Single<Person>(key);
          Assert.AreEqual(dateTime, person.Date);

          t.Complete();
        }
      }
    }
  }
}