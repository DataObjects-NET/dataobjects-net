// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2010.10.27

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0813_AuxilaryTableMappingNameBug_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0813_AuxilaryTableMappingNameBug_Model
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

  [HierarchyRoot]
  public class PersonGroup : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Person> Persons { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
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
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {

        var person = new Person();
        var typeInfo = person.TypeInfo;
        var fieldInfo = typeInfo.Fields["Friends"];
        var associationInfo = fieldInfo.Associations[0];
        var auxiliaryType = associationInfo.AuxiliaryType;
        Assert.AreEqual("Person-Friends-Person", auxiliaryType.Name);
        Assert.AreEqual("P-F-P", auxiliaryType.MappingName);

        var personGroup = new PersonGroup();
        typeInfo = personGroup.TypeInfo;
        fieldInfo = typeInfo.Fields["Persons"];
        associationInfo = fieldInfo.Associations[0];
        auxiliaryType = associationInfo.AuxiliaryType;
        Assert.AreEqual("PersonGroup-Persons-Person", auxiliaryType.Name);
        Assert.AreEqual("PersonGroup-Persons-P", auxiliaryType.MappingName);

        ts.Complete();
      }
    }
  }
}