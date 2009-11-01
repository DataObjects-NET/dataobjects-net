// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.28

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.KeyProviders;
using Xtensive.Storage.Tests.Storage.Internals;

namespace Xtensive.Storage.Tests.Storage.EntityRef
{
  [TestFixture]
  public class EntityRefTests
  {
    [HierarchyRoot(typeof (GuidLongGuidKeyProvider), "ID", "Sequence", "Guid")]
    public class Company : Entity
    {
      [Field]
      public Guid ID { get; set; }

      [Field]
      public long Sequence { get; set; }

      [Field]
      public Guid Guid { get; set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot(typeof (GuidProvider), "ID")]
    public class Department : Entity
    {
      [Field]
      public Guid ID { get; set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public Company Company { get; set; }
    }

    [Test]
    public void TestEntityRef()
    {
      DomainConfiguration config = new DomainConfiguration("memory://localhost/EntityRef");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.EntityRef");
      Domain domain = Domain.Build(config);
      using (domain.OpenSession()) {
        // Fill company1
        Company company1 = new Company();
        company1.Name = "Company 1";
        Department department11 = new Department();
        department11.Name = "Department 1.1";
        department11.Company = company1;
        Department department12 = new Department();
        department12.Name = "Department 1.2";
        department12.Company = company1;

        Assert.IsTrue(ReferenceEquals(department11.Company, department12.Company));

        // Fill company2
        Company company2 = new Company();
        company2.Name = "Company 2";
        Department department21 = new Department();
        department21.Name = "Department 2.1";
        department21.Company = company2;
      }
    }
  }
}