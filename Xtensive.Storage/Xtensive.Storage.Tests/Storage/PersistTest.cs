// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.01

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class PersistTest : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Customer).Assembly, typeof (Customer).Namespace);
      return configuration;
    }

    [Test]
    public void RemoveCreateTest()
    {
      using (Session.Open(Domain)) {
        Customer customer;
        using (var t = Transaction.Open()) {
          customer = new Customer("AAAAA");
          t.Complete();
        }
        using (var t = Transaction.Open()) {
          customer.Remove();
          new Customer("AAAAA");
          t.Complete();
        }
      }
    }

    [Test]
    public void UpdateRemoveTest()
    {
      using (Session.Open(Domain)) {
        Customer customer;
        using (var t = Transaction.Open()) {
          customer = new Customer("BBBBB");
          t.Complete();
        }
        using (var t = Transaction.Open()) {
          customer.CompanyName = "Xtensive";
          customer.Remove();
          t.Complete();
        }
      }
    }

    [Test]
    public void CreateRemoveTest()
    {
      using (Session.Open(Domain)) {
        Customer customer;
        using (var t = Transaction.Open()) {
          customer = new Customer("CCCCC");
          customer.Remove();
          t.Complete();
        }
      }      
    }
  }
}