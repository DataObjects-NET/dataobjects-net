// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.26

using System;
using System.Runtime.Serialization;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.Issues.Issue0485_EntitySetDescendant_Model;
using System.Linq;

namespace Xtensive.Storage.Tests.Issues.Issue0485_EntitySetDescendant_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Company : Entity
  {
    [Field][Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public EntitySetDescendant<Employee> Employees{ get; private set;}
  }

  [Serializable]
  [HierarchyRoot]
  public class Employee : Entity
  {
    [Field][Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

  }

  public class EntitySetDescendant<T> : EntitySet<T>
    where T : IEntity
  {
    protected EntitySetDescendant(Entity owner, FieldInfo field)
      : base(owner, field)
    {
    }

    protected EntitySetDescendant(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0485_EntitySetDescendant : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Company).Assembly, typeof (Company).Namespace);
      return config;
    }

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var company = new Company();
          var employee1 = new Employee();
          var employee2 = new Employee();
          var employee3 = new Employee();
          company.Employees.Add(employee1);
          company.Employees.Add(employee2);
          company.Employees.Add(employee3);
          Assert.IsNotNull(company.Employees.ElementAt(0));
          // Rollback
        }
      }
    }
  }
}