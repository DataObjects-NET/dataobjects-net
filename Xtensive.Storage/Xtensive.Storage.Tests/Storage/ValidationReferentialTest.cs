// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.08

using System.Linq;
using NUnit.Framework;
using Xtensive.Integrity.Aspects.Constraints;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.ValidationReferentialTestModel;

namespace Xtensive.Storage.Tests.Storage.ValidationReferentialTestModel
{
  [HierarchyRoot]
  public class Company : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Contact> Contacts { get; private set; }

  }

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
      using (Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          Company company;
          Contact contact;

          using (var region = InconsistentRegion.Open()) {
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