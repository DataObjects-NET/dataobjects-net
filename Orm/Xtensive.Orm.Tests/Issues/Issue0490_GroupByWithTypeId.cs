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
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using System.Reflection;

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0490_GroupByWithTypeId : ChinookDOModelTest
  {
    [Test]
    public void MainTest()
    {
      var groupQuery =
        from invoice in Session.Query.All<Invoice>()
        group invoice by invoice.Customer.Key
        into siteGroup
          select new {
            Enterprise = siteGroup.Key,
            SitesCount = siteGroup.Count()
          };
      QueryDumper.Dump(groupQuery);
    }
  }
}