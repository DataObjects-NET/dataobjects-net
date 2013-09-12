// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.09.17

using System;
using System.ComponentModel;
using NUnit.Framework;
using Xtensive.Orm.Validation;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.DataErrorInfoTestModel;

namespace Xtensive.Orm.Tests.Storage.DataErrorInfoTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    [NotNullConstraint]
    public string Name { get; set; }

    [Field]
    [RangeConstraint(Min = 1)]
    public int Age { get; set; }

    public bool IsValid { get; set; }

    protected override void OnValidate()
    {
      if (!IsValid)
        throw new InvalidOperationException("Person is invalid.");
    }
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
          var person = new Person();
          var personErrorInfo = ((IDataErrorInfo) person);

          Assert.AreEqual("Person is invalid.", personErrorInfo.Error);
          Assert.AreEqual("Value should not be null.", personErrorInfo["Name"]);
          Assert.AreEqual("Value should not be less than 1.", personErrorInfo["Age"]);

          person.Name = "Alex";
          person.Age = 26;
          person.IsValid = true;
          session.ValidateAndGetErrors();
          Assert.AreEqual(string.Empty, personErrorInfo.Error);
          Assert.AreEqual(string.Empty, personErrorInfo["Name"]);
          Assert.AreEqual(string.Empty, personErrorInfo["Age"]);

          person.Age = -1;
          session.ValidateAndGetErrors();
          Assert.AreEqual("Value should not be less than 1.", personErrorInfo["Age"]);

          AssertEx.Throws<ValidationFailedException>(session.Validate);
        }
      }
    }
  }
}