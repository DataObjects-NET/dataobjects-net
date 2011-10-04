// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0116_InterfacesCastAndIndexesModel;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0116_InterfacesCastAndIndexes : AutoBuildTest
  {
    [SetUp]
    public void SetUp()
    {
      using (Session session = Domain.OpenSession()) {
        using (TransactionScope t = session.OpenTransaction()) {
          for (int i = 0; i < 10; i++) {
            var p = new Employee(null)
                      {
                        FirstName = "Person " + i
                      };
            new Country(null)
              {
                Name = "Country " + i,
                Code = i.ToString()
              };
          }
          t.Complete();
        }
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (IRecord).Assembly, typeof (IRecord).Namespace);
      return config;
    }

    [Test]
    public void CastTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        session.Query.All(typeof (IEmployee)).Cast<IRecord>().ToList();
      }
    }

    [Test]
    public void OfTypeTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        session.Query.All(typeof (IRecord)).OfType<ICountry>().ToList();
        session.Query.All(typeof (IRecord)).OfType<Country>().ToList();
        session.Query.All(typeof (IRecord)).OfType<Employee>().ToList();
        session.Query.All(typeof (IRecord)).OfType<IEmployee>().ToList();
      }
    }
  }
}