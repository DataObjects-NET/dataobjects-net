// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.03

using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using System.Reflection;

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0490_GroupByWithTypeId : NorthwindDOModelTest
  {
    [Test]
    public void MainTest()
    {
      var groupQuery =
        from order in Session.Query.All<Order>()
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