// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.27

using System;
using System.Linq;
using NUnit.Framework;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using Xtensive.Orm.Rse;

namespace Xtensive.Orm.Tests.Rse
{
  [Serializable]
  [TestFixture, Category("Rse")]
  public class IncludeProviderTest: ChinookDOModelTest
  {
    [Test]
    public void SimpleTest()
    {
      var tracks = Session.Demand().Query.All<Track>().Take(10).ToList();
      var ids = tracks.Select(supplier => (Tuple)Tuple.Create(supplier.TrackId));

      var trackRs = Domain.Model.Types[typeof (Track)].Indexes.PrimaryIndex.GetQuery();
      var inRs = trackRs.Include(() => ids, "columnName", new[] {0});
      var inIndex = inRs.Header.Columns.Count-1;
      var whereRs = inRs.Filter(tuple => tuple.GetValueOrDefault<bool>(inIndex));
      var result = whereRs.GetRecordSet(Session.Current).ToList();
      Assert.AreEqual(0, whereRs.GetRecordSet(Session.Current).Select(t => t.GetValue<int>(0)).Except(tracks.Select(s => s.TrackId)).Count());
    }
  }
}