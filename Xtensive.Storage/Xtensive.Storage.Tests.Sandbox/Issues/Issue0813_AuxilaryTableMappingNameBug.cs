// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2010.10.14

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0813_AuxilaryTableMappingNameBug_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0813_AuxilaryTableMappingNameBug_Model
{
  [HierarchyRoot]
  [TableMapping("P")]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [FieldMapping("F")]
    public EntitySet<Person> Friends { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0813_AuxilaryTableMappingNameBug : AutoBuildTest
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
      using (Session.Open(Domain))
      using (var ts = Transaction.Open()) {
        var person = new Person();
        var typeInfo = person.TypeInfo;
        var fieldInfo = typeInfo.Fields["Friends"];
        var associationInfo = fieldInfo.Associations[0];
        var auxiliaryType = associationInfo.AuxiliaryType;
        Assert.AreEqual("Person-Friends-Person", auxiliaryType.Name);
        Assert.AreEqual("P-F-P", auxiliaryType.MappingName);
        ts.Complete();
      }
    }
  }
}