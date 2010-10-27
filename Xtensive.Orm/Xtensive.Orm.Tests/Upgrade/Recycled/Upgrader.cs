// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Tests.Upgrade.Recycled.Model.Version2;

namespace Xtensive.Orm.Tests.Upgrade.Recycled
{
  [Serializable]
  public class Upgrader : UpgradeHandler
  {
    private static bool isEnabled = false;
    private static string runningVersion;

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable Enable(string version)
    {
      if (isEnabled)
        throw new InvalidOperationException();
      isEnabled = true;
      runningVersion = version;
      return new Disposable(_ => {
        isEnabled = false;
        runningVersion = null;
      });
    }

    public override bool IsEnabled {
      get {
        return isEnabled;
      }
    }
    
    protected override string DetectAssemblyVersion()
    {
      return runningVersion;
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    protected override void AddUpgradeHints(Xtensive.Collections.ISet<UpgradeHint> hints)
    {
      if (runningVersion=="2")
        hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Upgrade.Recycled.Model.Version1.Order", typeof(Order)));
    }

    public override void OnUpgrade()
    {
      var customerMap = new Dictionary<int, Customer>();
      var employeeMap = new Dictionary<int, Employee>();
      var rcCustomers = Session.Demand().Query.All<RcCustomer>();
      var rcEmployees = Session.Demand().Query.All<RcEmployee>();
      var orders = Session.Demand().Query.All<Order>();
      foreach (var rcCustomer in rcCustomers) {
        var customer = new Customer() {
           Address = rcCustomer.Address,
           Phone = rcCustomer.Phone,
           Name = rcCustomer.Name
         };
        customerMap.Add(rcCustomer.Id, customer);
      }
      foreach (var rcEmployee in rcEmployees) {
        var employee = new Employee() {
          CompanyName = rcEmployee.CompanyName,
          Name = rcEmployee.Name
        };
        employeeMap.Add(rcEmployee.Id, employee);
      }

      var log = new List<string>();
      foreach (var order in orders) {
#pragma warning disable 612,618
        order.Customer = customerMap[order.RcCustomer.Id];
        order.Employee = employeeMap[order.RcEmployee.Id];
        log.Add(order.ToString());
#pragma warning restore 612,618
      }
      foreach (var order in orders)
        log.Add(order.ToString());
      Log.Info("Orders: {0}", Environment.NewLine + 
        string.Join(Environment.NewLine, log.ToArray()));
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      string suffix = ".Version" + runningVersion;
      var originalNamespace = type.Namespace;
      var nameSpace = originalNamespace.TryCutSuffix(suffix);
      return nameSpace!=originalNamespace 
        && base.IsTypeAvailable(type, upgradeStage);
    }

    
  }
}