// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.03

using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core.Linq;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Linq;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Reflection;

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue490_GroupByWithTypeId : NorthwindDOModelTest
  {
    [Test]
    public void MainTest()
    {
      var groupQuery =
        from order in Query<Order>.All
        group order by order.Customer.Key
        into siteGroup
          select new {
            Enterprise = siteGroup.Key,
            SitesCount = siteGroup.Count()
          };
      QueryDumper.Dump(groupQuery);
    }
  }
}