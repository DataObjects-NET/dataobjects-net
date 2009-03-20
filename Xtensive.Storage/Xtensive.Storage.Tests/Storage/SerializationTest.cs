// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.19

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Storage;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Serialization;
using Xtensive.Storage.Tests.SerializationTestModel;

namespace Xtensive.Storage.Tests.SerializationTestModel
{
  [Serializable ]
  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public class Company : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Storage.Tests
{
  public class SerializationTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Company).Assembly, typeof (Company).Namespace);
      return config;
    }

    private static readonly BinaryFormatter formatter = new BinaryFormatter();

    [Test]
    public void MainTest()
    {
      MemoryStream stream = new MemoryStream();

      string companyName = "Xtensive LLC";
      int companyId;
      
      using (Domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          Company company = new Company {Name = companyName};
          companyId = company.Id;

          using (new SerializationContext(entity => SerializationKind.ByReference).Activate()) {
            formatter.Serialize(stream, company);
          }

          // Can'not resolve deserialized entity - it's not commited in original session.
          AssertEx.Throws<InvalidOperationException>(
            delegate {
              using (Domain.OpenSession()) {
                using (Transaction.Open()) {
                  stream.Position = 0;
                  formatter.Deserialize(stream);
                }
              }
            });

          transactionScope.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          stream.Position = 0;
          Company company = Query<Company>.All.First();
          Company deserializedCompany = (Company) formatter.Deserialize(stream);

          Assert.AreSame(company, deserializedCompany);
        }
      }
      
      using (Domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          stream.Position = 0;
          Company company = (Company) formatter.Deserialize(stream);

          Assert.AreEqual(companyName, company.Name);
          Assert.AreEqual(companyId, company.Id);
          Assert.AreEqual(company.Session, Session.Current);
        }
      }
    }
  }
}
