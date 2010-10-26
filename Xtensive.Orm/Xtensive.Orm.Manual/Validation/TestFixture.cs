// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.10.28

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using AggregateException = Xtensive.Core.AggregateException;

namespace Xtensive.Orm.Manual.Validation
{
  [TestFixture]
  public class TestFixture
  {
    [Test]
    public void MainTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
      var domain = Domain.Build(config);

      using (var session = domain.OpenSession()) {
        try {
          using (var transactionScope = session.OpenTransaction()) {
            using (var inconsistencyRegion = Orm.ValidationManager.Disable()) {

              var person = new Person {
                FirstName = "Mike",
                LastName = "Grooovy",
                Height = 1.5,
                BirthDay = new DateTime(1983, 03, 16),
                IsSubscribedOnNews = true,
                Email = "mike@grooovy.com"
              };

              inconsistencyRegion.Complete();
            }
            transactionScope.Complete();
          }
        }
        catch (AggregateException exception) {
          Console.WriteLine("Following validation errors were found:");
          foreach (var error in exception.GetFlatExceptions())
            Console.WriteLine(error.Message);
        }
      }
    }
  }
}
