// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.19

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
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
  [Serializable]
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Company : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Emploee Head { get; set;}

    public override string ToString()
    {
      return Name;
    }

    public Company()
    {      
    }

    protected Company(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }

  [Serializable]
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Emploee : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Company Company { get; set;}

    public override string ToString()
    {
      return Name;
    }

    public Emploee()
    {
    }

    protected Emploee(SerializationInfo info, StreamingContext context) : base(info, context)
    {      
    }
  }
}

namespace Xtensive.Storage.Tests
{
  public class  SerializationTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Company).Assembly, typeof (Company).Namespace);
      return config;
    }

    private static readonly BinaryFormatter formatter = new BinaryFormatter();

    
    [Test]
    public void     SerializationByReferenceTest()
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
          AssertEx.Throws<TargetInvocationException>(
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
          Company company = (Company) Key.Create(typeof (Company), Tuple.Create(companyId)).Resolve();// Query<Company>.All.First();

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

    [Test]
    public void ComplexTest()
    {
      MemoryStream stream = new MemoryStream();

      int firstCompanyId;

      using (Domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          Company company = new Company {Name = "Transactional lines"};
          firstCompanyId = company.Id;
          transactionScope.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          Company existingCompany = (Company) Key.Create(typeof (Company), Tuple.Create(firstCompanyId)).Resolve(); //Query<Company>.All.First();

          Company company = new Company {Name = "Region mobile"};
          Emploee mike = new Emploee {Name = "Mike", Company = company};
          Emploee alex = new Emploee {Name = "Alex", Company = company};
          Emploee jef = new Emploee {Name = "Jef", Company = existingCompany};
          company.Head = alex;

          var array = new object[] { existingCompany, company, alex, jef };

          SerializationContext context =
            new SerializationContext(entity => entity==existingCompany ? SerializationKind.ByReference : SerializationKind.ByValue);

          using (context.Activate()) {
            formatter.Serialize(stream, array);
          }
          // Rollback
        }
      }

      using (Domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {

          object[] array;
          stream.Position = 0;

          var deserializationContext = new DeserializationContext();

          using (deserializationContext.Activate()) {
            array = (object [])  formatter.Deserialize(stream);
          }

          Company oldCompany = (Company) array[0];
          Company newCompany = (Company) array[1];
          Emploee alex = (Emploee) array[2];
          Emploee jef = (Emploee) array[3];

          Assert.AreEqual(firstCompanyId, oldCompany.Id);
          Assert.AreEqual("Alex", alex.Name);
          Assert.AreEqual("Jef", jef.Name);

          Assert.AreEqual(oldCompany, jef.Company);
          Assert.AreEqual(newCompany, alex.Company);
          Assert.AreEqual(alex, newCompany.Head);
        }
      }    
    }
  }
}
