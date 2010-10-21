// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.08

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.ValidationReferentialTestModel;
using Xtensive.Storage.Validation;

namespace Xtensive.Storage.Tests.Storage.ValidationReferentialTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class Company : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Contact> Contacts { get; private set; }

  }

  [Serializable]
  [HierarchyRoot]
  public class Contact : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, Association(PairTo = "Contacts", OnTargetRemove = OnRemoveAction.Cascade)]
    [NotNullConstraint]
    public Company Company { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class ValidationReferentialTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Company).Assembly, typeof (Company).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          Company company;
          Contact contact;

          using (var region = Xtensive.Storage.ValidationManager.Disable()) {
            company = new Company();
            contact = new Contact {Company = company};
            region.Complete();
          }

          Assert.AreEqual(1, company.Contacts.Count);
          Assert.AreEqual(company.Contacts.First(), contact);

          contact.Remove();

          Assert.AreEqual(0, company.Contacts.Count);
          transactionScope.Complete();
        }
      }
    }
  }
}