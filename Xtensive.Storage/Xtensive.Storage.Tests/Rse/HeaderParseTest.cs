// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.07

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.Storage.LazyLoadModel;

namespace Xtensive.Storage.Tests.Rse
{
  public class HeaderParseTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.LazyLoadModel");
      return config;
    }

    [Test]
    public void MainTest()
    {
      Key key;
      using (Domain.OpenSession()) {
        Book book = new Book();
        book.Title = "Title";
        book.Text = "Text";
        key = book.Key;
      }
      using (Domain.OpenSession()) {
        EntityData data = Session.Current.DataCache[key];
        Assert.IsNull(data);
        IndexInfo ii = Domain.Model.Types[typeof (Book)].Indexes.PrimaryIndex;

        // Select *
        RecordSet rsMain = Session.Current.Handler.Select(ii);
        rsMain.Parse();
        data = Session.Current.DataCache[key];
        Assert.IsNotNull(data);
        Assert.IsTrue(data.Tuple.IsAvailable(2));
        Assert.IsTrue(data.Tuple.IsAvailable(3));
        Session.Current.DataCache.Clear();

        // Select Id, TypeId, Title
        RecordSet rsTitle = rsMain.Select(0, 1, 2);
        rsTitle.Parse();
        data = Session.Current.DataCache[key];
        Assert.IsNotNull(data);
        Assert.IsTrue(data.Tuple.IsAvailable(2));
        Assert.IsFalse(data.Tuple.IsAvailable(3));
        Session.Current.DataCache.Clear();

        // Select Id, TypeId, Text
        RecordSet rsText = rsMain.Select(0, 1, 3);
        rsText.Parse();
        data = Session.Current.DataCache[key];
        Assert.IsNotNull(data);
        Assert.IsFalse(data.Tuple.IsAvailable(2));
        Assert.IsTrue(data.Tuple.IsAvailable(3));
        Session.Current.DataCache.Clear();

        // Select a.Id, a.TypeId, a.Title, b.Id, b.TypeId, b.Text
        RecordSet rsJoin = rsTitle.Alias("a").Join(rsText.Alias("b"), new Pair<int>(0, 0), new Pair<int>(1,1));
        rsJoin.Parse();
        data = Session.Current.DataCache[key];
        Assert.IsNotNull(data);
        Assert.IsTrue(data.Tuple.IsAvailable(2));
        Assert.IsTrue(data.Tuple.IsAvailable(3));
        Session.Current.DataCache.Clear();
      }
    }
  }
}