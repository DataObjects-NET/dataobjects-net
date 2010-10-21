// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.19

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Serialization;
using Xtensive.Storage.Tests.SerializationTestModel;

namespace Xtensive.Storage.Tests.SerializationTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class Company : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Emploee Head { get; set;}

    public override string ToString()
    {
      return Name;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Emploee : Entity
  {
    [Field, Key] 
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Company Company { get; set;}

    public override string ToString()
    {
      return Name;
    }

    protected override void OnValidate()
    {
      if (string.IsNullOrEmpty(Name) || Company==null)
        throw new InvalidOperationException("Invalid îbject.");
    }
  }

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class Country : Entity
  {
    [Field(Length = 100), Key]
    public string Name { get; private set;}

//    [Field]
//    public City Capital { get; set; }

    public Country(string name) : 
      base(name)
    {
    }
  }

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class City : Entity
  {
    [Field, Key(0)]
    public Country Country { get; private set;}

    [Field(Length = 100), Key(1)]
    public string Name { get; private set;}

    public City(Country country, string name) 
      : base(country, name)
    {
    }
  }
    
  [Serializable]
  public class Address : Structure
  {
    [Field]
    public City City { get; set;}

    [Field]
    public string Street { get; set;}

    [Field]
    public string Number { get; set;}
  }
}

namespace Xtensive.Storage.Tests.Storage
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
    public void SerializationOfComplexReferencesTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
        }
      }
    }

    [Test]
    public void SerializationByReferenceTest()
    {
      MemoryStream stream = new MemoryStream();

      string companyName = "Xtensive LLC";
      int companyId;
      
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          Company company = new Company {Name = companyName};
          companyId = company.Id;

          using (new SerializationContext(entity => SerializationKind.ByReference).Activate()) {
            formatter.Serialize(stream, company);
          }
  
          // Can'not resolve deserialized entity - it's not commited in original session.
          AssertEx.Throws<TargetInvocationException>(
            delegate {
              using (var session2 = Domain.OpenSession()) {
                using (session2.OpenTransaction()) {
                  stream.Position = 0;
                  formatter.Deserialize(stream);
                }
              }
            });

          transactionScope.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          stream.Position = 0;
          Company company = (Company) session.Query.SingleOrDefault(Key.Create(typeof (Company), companyId));// session.Query.All<Company>().First();

          Company deserializedCompany = (Company) formatter.Deserialize(stream);

          Assert.AreSame(company, deserializedCompany);
        }
      }
      
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          stream.Position = 0;
          Company company = (Company) formatter.Deserialize(stream);

          Assert.AreEqual(companyName, company.Name);
          Assert.AreEqual(companyId, company.Id);
          Assert.AreEqual(company.Session, Session.Current);
        }
      }
    }

    [Test]
    public void ReferencedKeysTest()
    {       
      Stream stream = new MemoryStream();

      using (var session = Domain.OpenSession()) {  

        Country russia;
        
        using (var transactionScope = session.OpenTransaction()) {
          russia = new Country("Russia");
          transactionScope.Complete();
        }

        using (session.OpenTransaction()) {
          Country china = new Country("China");

          City moscow = new City(russia, "Moscow");
          City ekaterinburg = new City(russia, "Ekaterinburg");
          City hongKong = new City(china, "Hong Kong");
          City beijing = new City(china, "Beijing");
          City guangzhou = new City(china, "Guangzhou");

//          china.Capital = beijing;
//          russia.Capital = moscow;

          City[] cities = new[] {ekaterinburg, moscow, hongKong, guangzhou, beijing};          

          var serializationContext = new SerializationContext(
            entity => entity==russia ? SerializationKind.ByReference : SerializationKind.ByValue);

          using (serializationContext.Activate()) {
            formatter.Serialize(stream, cities);
          }
          // Rollback
        }
      }

      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          
          var deserializationContext = new DeserializationContext();

          City[] cities;

          using (deserializationContext.Activate()) {
            stream.Position = 0;
            cities = (City[]) formatter.Deserialize(stream);
          }

          City ekaterinburg = cities[0];
          City moscow = cities[1];
          City hongKong = cities[2];
          City guangzhou = cities[3];

          Country russia = ekaterinburg.Country;
          Country china = guangzhou.Country;

          Assert.IsNotNull(russia);
          Assert.IsNotNull(china);

          Assert.AreEqual("Russia", russia.Name);
          Assert.AreEqual("China", china.Name);

          Assert.AreEqual("Ekaterinburg", ekaterinburg.Name);
          Assert.AreEqual("Moscow", moscow.Name);
          Assert.AreEqual(russia, moscow.Country);          
          Assert.AreEqual("Hong Kong", hongKong.Name);
          Assert.AreEqual(china, hongKong.Country);
          
//          Assert.IsNull(russia.Capital); // russia was serialized by reference, not by value
//    
//          Assert.AreEqual(china.Capital.Name, "Beijing");
          
        }
      }
    }

    [Test]
    public void ComplexTest()
    {
      MemoryStream stream = new MemoryStream();

      int firstCompanyId;

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          Company company = new Company {Name = "OpenTransaction lines"};
          firstCompanyId = company.Id;
          transactionScope.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {

          object[] array;
          Company existingCompany = (Company) session.Query.SingleOrDefault(Key.Create(typeof (Company), firstCompanyId)); //session.Query.All<Company>().First();

          using (var region = Xtensive.Storage.ValidationManager.Disable()) {

            Company company = new Company {Name = "Region mobile"};
            Emploee mike = new Emploee {Name = "Mike", Company = company};
            Emploee alex = new Emploee {Name = "Alex", Company = company};
            Emploee jef = new Emploee {Name = "Jef", Company = existingCompany};
            company.Head = alex;

            array = new object[] { existingCompany, company, alex, jef };
            region.Complete();
          }
          

          SerializationContext context =
            new SerializationContext(entity => entity==existingCompany ? SerializationKind.ByReference : SerializationKind.ByValue);

          using (context.Activate()) {
            formatter.Serialize(stream, array);
          }
          // Rollback
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {

          object[] array;
          stream.Position = 0;

          var deserializationContext = new DeserializationContext();

          using (deserializationContext.Activate()) {
            array = (object []) formatter.Deserialize(stream);
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
