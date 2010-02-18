// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.18

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Linq.Dto;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  namespace Dto
  {
    [HierarchyRoot]
    public class Person
     : Entity
    {
      [Field, Key]
      public int ID { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    public class PersonDto
    {
      public string Name { get; set; }
    }
  }

  [Serializable]
  public class SelectDtoTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Person));
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var alex = new Person() { Name = "Alex" };
        var query = Query.All<Person>()
          .Select(p => new PersonDto {Name = p.Name})
          .Where(personDto => personDto.Name == "Alex");
        var result = query.ToList();
        var firstPerson = query.SingleOrDefault();
        Assert.IsTrue(firstPerson.Name == "Alex");
        t.Complete();
      }
    }
  }
}