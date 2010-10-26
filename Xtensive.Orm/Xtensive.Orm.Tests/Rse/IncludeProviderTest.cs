// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.27

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using Xtensive.Storage.Rse;

namespace Xtensive.Orm.Tests.Rse
{
  [Serializable]
  [TestFixture, Category("Rse")]
  public class IncludeProviderTest: NorthwindDOModelTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Supplier).Assembly, typeof(Supplier).Namespace);
      return config;
    }

    [Test]
    public void SimpleTest()
    {
      var suppliers = Session.Demand().Query.All<Supplier>().Take(10).ToList();
      var ids = suppliers.Select(supplier => (Tuple)Tuple.Create(supplier.Id));

      var supplierRs = Domain.Model.Types[typeof (Supplier)].Indexes.PrimaryIndex.ToRecordQuery();
      var inRs = supplierRs.Include(() => ids, "columnName", new[] {0});
      var inIndex = inRs.Header.Columns.Count-1;
      var whereRs = inRs.Filter(tuple => tuple.GetValueOrDefault<bool>(inIndex));
      var result = whereRs.ToRecordSet(Session.Current).ToList();
      Assert.AreEqual(0, whereRs.ToRecordSet(Session.Current).Select(t => t.GetValue<int>(0)).Except(suppliers.Select(s => s.Id)).Count());
    }
  }
}