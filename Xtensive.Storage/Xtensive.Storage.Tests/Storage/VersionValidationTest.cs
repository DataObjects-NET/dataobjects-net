// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.11.11

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using System.Linq;
using Xtensive.Storage.Disconnected;

#region Model

namespace Xtensive.Storage.Tests.Storage.VersionValidationModel
{
  [Serializable]
  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Version, Field]
    public int VersionId { get; set; }

    [Field]
    public string Name { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Number { get; set; }

    [Field]
    public Customer Customer { get; set; }
  }
}

#endregion

namespace Xtensive.Storage.Tests.Storage
{
  using VersionValidationModel;

  [TestFixture]
  public class VersionValidationTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.VersionValidationModel");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    [Test]
    public void UpdateItemTest()
    {
      var customerKey = CreateCustomer();
      VersionInfo customerVersion;
      
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var customer = Query.Single<Customer>(customerKey);
          customerVersion = customer.VersionInfo;
          transactionScope.Complete();
        }
      }

      Func<Key, VersionInfo> versionGetter = key => {
        if (key==customerKey)
          return customerVersion;
        else
          throw new ArgumentException();
      };

      using (var session = Session.Open(Domain)) {
        using (VersionValidator.Attach(session, versionGetter)) {
          using (var transactionScope = Transaction.Open()) {
            var customer = Query.Single<Customer>(customerKey);
            customer.Name = "Customer3";
            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void UpdateChangedItemTest()
    {
      var customerKey = CreateCustomer();
      VersionInfo customerVersion;
      
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var customer = Query.Single<Customer>(customerKey);
          customerVersion = customer.VersionInfo;
          transactionScope.Complete();
        }
      }

      RenameCustomer(customerKey);

      Func<Key, VersionInfo> versionGetter = key => {
        if (key==customerKey)
          return customerVersion;
        else
          throw new ArgumentException();
      };

      using (var session = Session.Open(Domain)) {
        using (VersionValidator.Attach(session, versionGetter)) {
          AssertEx.Throws<InvalidOperationException>(() => {
            using (var transactionScope = Transaction.Open()) {
              var customer = Query.Single<Customer>(customerKey);
              customer.Name = "Customer3";
              transactionScope.Complete();
            }
          });
        }
      }
    }

    [Test]
    public void RemoveItemTest()
    {
      var customerKey = CreateCustomer();
      VersionInfo customerVersion;
      
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var customer = Query.Single<Customer>(customerKey);
          customerVersion = customer.VersionInfo;
          transactionScope.Complete();
        }
      }

      Func<Key, VersionInfo> versionGetter = key => {
        if (key==customerKey)
          return customerVersion;
        else
          throw new ArgumentException();
      };

      using (var session = Session.Open(Domain)) {
        using (VersionValidator.Attach(session, versionGetter)) {
          using (var transactionScope = Transaction.Open()) {
            var customer = Query.Single<Customer>(customerKey);
            customer.Remove();
            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void RemoveChangedItemTest()
    {
      var customerKey = CreateCustomer();
      VersionInfo customerVersion;
      
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var customer = Query.Single<Customer>(customerKey);
          customerVersion = customer.VersionInfo;
          transactionScope.Complete();
        }
      }

      RenameCustomer(customerKey);

      Func<Key, VersionInfo> versionGetter = key => {
        if (key==customerKey)
          return customerVersion;
        else
          throw new ArgumentException();
      };

      using (var session = Session.Open(Domain)) {
        using (VersionValidator.Attach(session, versionGetter)) {
          AssertEx.Throws<InvalidOperationException>(() => {
            using (var transactionScope = Transaction.Open()) {
              var customer = Query.Single<Customer>(customerKey);
              customer.Remove();
              transactionScope.Complete();
            }
          });
        }
      }
    }

    private void RenameCustomer(Key key)
    {
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var customer = Query.Single<Customer>(key);
          customer.Name = "Updated customer";
          transactionScope.Complete();
        }
      }
    }

    private void RemoveCustomer(Key key)
    {
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var customer = Query.Single<Customer>(key);
          customer.Remove();
          transactionScope.Complete();
        }
      }
    }

    private Key CreateCustomer()
    {
      Key result;
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var customer = new Customer();
          customer.Name = "Customer";
          result = customer.Key;
          transactionScope.Complete();
        }
      }
      return result;
    }
  }
}