// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2009.03.19

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Serialization;
using Xtensive.Orm.Tests.SerializationTestModel;

namespace Xtensive.Orm.Tests.SerializationTestModel
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

    public override string ToString() => Name;
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

    public override string ToString() => Name;

    protected override void OnValidate()
    {
      if (string.IsNullOrEmpty(Name) || Company == null) {
        throw new InvalidOperationException("Invalid îbject.");
      }
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

namespace Xtensive.Orm.Tests.Storage
{
  public class SerializationTest : AutoBuildTest
  {
    private readonly BinaryFormatter formatter = new BinaryFormatter();

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Company).Assembly, typeof(Company).Namespace);
      return config;
    }

    


    [Test]
    public void SerializationOfComplexReferencesTest()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {

      }
    }

    [Test]
    public void SerializationByReferenceTest()
    {
      using (var stream = new MemoryStream()) {

        var companyName = "Xtensive LLC";
        int companyId;

        using (var session = Domain.OpenSession())
        using (var transactionScope = session.OpenTransaction()) {
          var company = new Company { Name = companyName };
          companyId = company.Id;

          using (new SerializationContext(entity => SerializationKind.ByReference).Activate()) {
            formatter.Serialize(stream, company);
          }

          // Can'not resolve deserialized entity - it's not commited in original session.
          var ex = Assert.Throws<System.Runtime.Serialization.SerializationException>(
            delegate {
              using (var session2 = Domain.OpenSession())
              using (session2.OpenTransaction()) {
                stream.Position = 0;
                _ = formatter.Deserialize(stream);
              }
            });
          Assert.That(ex.InnerException, Is.InstanceOf<TargetInvocationException>());

          transactionScope.Complete();
        }

        using (var session = Domain.OpenSession())
        using (var transactionScope = session.OpenTransaction()) {
          stream.Position = 0;
          var companyKey = Key.Create(Domain, typeof(Company), companyId);
          var company = (Company) session.Query.SingleOrDefault(companyKey);

          var deserializedCompany = (Company) formatter.Deserialize(stream);

          Assert.AreSame(company, deserializedCompany);
        }

        using (var session = Domain.OpenSession())
        using (var transactionScope = session.OpenTransaction()) {
          stream.Position = 0;
          var company = (Company) formatter.Deserialize(stream);

          Assert.AreEqual(companyName, company.Name);
          Assert.AreEqual(companyId, company.Id);
          Assert.AreEqual(company.Session, Session.Current);
        }
      }
    }

    [Test]
    public void ReferencedKeysTest()
    {
      using (var stream = new MemoryStream()) {

        using (var session = Domain.OpenSession()) {

          Country russia;

          using (var transactionScope = session.OpenTransaction()) {
            russia = new Country("Russia");
            transactionScope.Complete();
          }

          using (session.OpenTransaction()) {
            var china = new Country("China");

            var moscow = new City(russia, "Moscow");
            var ekaterinburg = new City(russia, "Ekaterinburg");
            var hongKong = new City(china, "Hong Kong");
            var beijing = new City(china, "Beijing");
            var guangzhou = new City(china, "Guangzhou");

            //china.Capital = beijing;
            //russia.Capital = moscow;

            var cities = new City[] { ekaterinburg, moscow, hongKong, guangzhou, beijing };

            var serializationContext = new SerializationContext(
              entity => entity == russia
                ? SerializationKind.ByReference
                : SerializationKind.ByValue);

            using (serializationContext.Activate()) {
              formatter.Serialize(stream, cities);
            }
            // Rollback
          }
        }

        using (var session = Domain.OpenSession())
        using (session.OpenTransaction()) {

          var deserializationContext = new DeserializationContext();

          City[] cities;

          using (deserializationContext.Activate()) {
            stream.Position = 0;
            cities = (City[]) formatter.Deserialize(stream);
          }

          var ekaterinburg = cities[0];
          var moscow = cities[1];
          var hongKong = cities[2];
          var guangzhou = cities[3];

          var russia = ekaterinburg.Country;
          var china = guangzhou.Country;

          Assert.IsNotNull(russia);
          Assert.IsNotNull(china);

          Assert.AreEqual("Russia", russia.Name);
          Assert.AreEqual("China", china.Name);

          Assert.AreEqual("Ekaterinburg", ekaterinburg.Name);
          Assert.AreEqual("Moscow", moscow.Name);
          Assert.AreEqual(russia, moscow.Country);
          Assert.AreEqual("Hong Kong", hongKong.Name);
          Assert.AreEqual(china, hongKong.Country);

          //Assert.IsNull(russia.Capital); // russia was serialized by reference, not by value
          //
          //Assert.AreEqual(china.Capital.Name, "Beijing");
        }
      }
    }

    [Test]
    public void ComplexTest()
    {
      using (var stream = new MemoryStream()) {

        int firstCompanyId;

        using (var session = Domain.OpenSession())
        using (var transactionScope = session.OpenTransaction()) {
          var company = new Company { Name = "OpenTransaction lines" };
          firstCompanyId = company.Id;
          transactionScope.Complete();
        }


        using (var session = Domain.OpenSession())
        using (session.OpenTransaction()) {
          var existingCompanyKey = Key.Create(Domain, typeof(Company), firstCompanyId);
          var existingCompany = (Company) session.Query.SingleOrDefault(existingCompanyKey);

          var company = new Company { Name = "Region mobile" };
          var mike = new Emploee { Name = "Mike", Company = company };
          var alex = new Emploee { Name = "Alex", Company = company };
          var jef = new Emploee { Name = "Jef", Company = existingCompany };
          company.Head = alex;

          var array = new object[] { existingCompany, company, alex, jef };
          session.Validate();

          var context = new SerializationContext(
            entity => entity == existingCompany
              ? SerializationKind.ByReference
              : SerializationKind.ByValue);

          using (context.Activate()) {
            formatter.Serialize(stream, array);
          }
          // Rollback
        }

        using (var session = Domain.OpenSession())
        using (var transactionScope = session.OpenTransaction()) {

          object[] array;
          stream.Position = 0;

          var deserializationContext = new DeserializationContext();

          using (deserializationContext.Activate()) {
            array = (object[]) formatter.Deserialize(stream);
          }

          var oldCompany = (Company) array[0];
          var newCompany = (Company) array[1];
          var alex = (Emploee) array[2];
          var jef = (Emploee) array[3];

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
