// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.10.07

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues_Issue0825_StrangeLinqBehavior;

namespace Xtensive.Orm.Tests.Issues_Issue0825_StrangeLinqBehavior
{
  [HierarchyRoot]
  public class SevRGas2EPassportCustomerMatchGuess : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int SevRGasCustomerID { get; set; }

    [Field]
    public int EPassportCustomerID { get; set; }

    [Field]
    public int Rank { get; set; }
  }

  [HierarchyRoot]
  public class SevRGasCustomer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string FullName { get; set; }

    [Field]
    public int Tin { get; set; }

    [Field]
    public string Code { get; set; }
  }

  [HierarchyRoot]
  public class EPassportAgent : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [HierarchyRoot]
  public class DataUnitCustomer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public SevRGasCustomer SevRGasCustomer { get; set; }

    [Field]
    public EPassportAgent EPassportAgent { get; set; }
  }

}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0825_StrangeLinqBehavior : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (SevRGas2EPassportCustomerMatchGuess).Assembly, typeof (SevRGas2EPassportCustomerMatchGuess).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transactionn = session.OpenTransaction()) {

          var sevRGasCustomer2 = new SevRGasCustomer() {
            Code = "Code",
            FullName = "Customer FullName",
            Name = "Customer Name",
            Tin = 44
          };
          // var ePassportAgent2 = new EPassportAgent();
          var sevRGas2EPassportCustomerMatchGuess2 = new SevRGas2EPassportCustomerMatchGuess {
            SevRGasCustomerID = sevRGasCustomer2.Id,
            EPassportCustomerID = 12345,
            Rank = 12345
          };
          var dataUnitCustomer2 = new DataUnitCustomer {
            SevRGasCustomer = sevRGasCustomer2,
            EPassportAgent = null
          };

          var sevRGasCustomer1 = new SevRGasCustomer() {
            Code = "Code",
            FullName = "Customer FullName",
            Name = "Customer Name",
            Tin = 12
          };
          var ePassportAgent1 = new EPassportAgent();
          var sevRGas2EPassportCustomerMatchGuess1 = new SevRGas2EPassportCustomerMatchGuess {
            SevRGasCustomerID = sevRGasCustomer1.Id,
            EPassportCustomerID = ePassportAgent1.Id,
            Rank = 12345
          };
          var dataUnitCustomer1 = new DataUnitCustomer {
            SevRGasCustomer = sevRGasCustomer1,
            EPassportAgent = ePassportAgent1
          };

          var query =
            from customer in session.Query.All<SevRGasCustomer>()
            let matchGuess = session.Query.All<SevRGas2EPassportCustomerMatchGuess>().Where(
              g => g.SevRGasCustomerID==customer.Id).FirstOrDefault()
            let matchGuessObject = session.Query.All<EPassportAgent>().Where(
              go => go.Id==matchGuess.EPassportCustomerID).FirstOrDefault()
            let unifiedCustomer = session.Query.All<DataUnitCustomer>().Where(
              ua => ua.SevRGasCustomer==customer && ua.EPassportAgent!=null).FirstOrDefault()
            select new SevRGas2EPassportCustomerMatchListItem {
              LeftID = customer.Id,
              LeftName = customer.Name ?? customer.FullName,
              LeftUniqueTaxpayerNumber = customer.Tin,
              LeftCode = customer.Code,
              Rank = matchGuess.Rank,
              Status = unifiedCustomer==null ? MatchStatus.NotConfirmed : MatchStatus.Confirmed,
              Right = GetRight(unifiedCustomer, matchGuess, matchGuessObject)
            };
          var result = query.ToList();
        }
      }
    }

    private EPassportAgent GetRight(DataUnitCustomer unifiedCustomer, SevRGas2EPassportCustomerMatchGuess matchGuess, EPassportAgent matchGuessObject)
    {
      return matchGuessObject;
    }

  }

  public class SevRGas2EPassportCustomerMatchListItem
  {
    public int LeftID { get; set; }

    public string LeftName { get; set; }

    public int LeftUniqueTaxpayerNumber { get; set; }

    public string LeftCode { get; set; }

    public int Rank { get; set; }

    public MatchStatus Status { get; set; }

    public EPassportAgent Right { get; set; }
  }
}