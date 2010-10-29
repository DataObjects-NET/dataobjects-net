// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.09.17

using System;
using System.ComponentModel;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Validation;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.DataErrorInfoTestModel;
using AggregateException = Xtensive.Core.AggregateException;

namespace Xtensive.Orm.Tests.Storage.DataErrorInfoTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    [NotNullConstraint(Message = "Name is empty.")]
    public string Name { get; set; }

    [Field]
    [RangeConstraint(Min = 1, Message = "Age is negative.")]
    public int Age { get; set;}
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class DataErrorInfoTest : AutoBuildTest
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
        using (var tx = session.OpenTransaction()) {
          using (var region = Xtensive.Orm.ValidationManager.Disable()) {

            var person = new Person();

            Assert.AreEqual("Name is empty.", ((IDataErrorInfo) person)["Name"]);
            Assert.AreEqual("Age is negative.", ((IDataErrorInfo) person)["Age"]);

            person.Name = "Alex";
            person.Age = 26;

            Assert.AreEqual(string.Empty, ((IDataErrorInfo) person)["Name"]);
            Assert.AreEqual(string.Empty, ((IDataErrorInfo) person)["Age"]);

            person.Age = -1;
            Assert.AreEqual("Age is negative.", ((IDataErrorInfo) person)["Age"]);

            region.Complete();
            AssertEx.Throws<AggregateException>(region.Dispose);
          } // Second .Dispose should do nothing!

          // tx.Complete(); // Rollback
        }
      }
    }
  }
}