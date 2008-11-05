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
using Xtensive.Storage.Tests.Storage.BookAuthorModel;

namespace Xtensive.Storage.Tests.Rse
{
  public class HeaderParseTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.BookAuthorModel");
      return config;
    }

    [Test]
    public void MainTest()
    {
      Key key;
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Book book = new Book {Title = "Title", Text = "Text"};
          key = book.Key;
          Session.Current.Persist();
          t.Complete();
        }
      }
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          EntityState state = Session.Current.Cache[key];
          Assert.IsNull(state);
          IndexInfo ii = Domain.Model.Types[typeof (Book)].Indexes.PrimaryIndex;

          // Select *
          RecordSet rsMain = ii.ToRecordSet();
          foreach (Book book in rsMain.ToEntities<Book>()) {
            ;
          }
          state = Session.Current.Cache[key];
          Assert.IsNotNull(state);
          Assert.IsTrue(state.Data.IsAvailable(2));
          Assert.IsTrue(state.Data.IsAvailable(3));
          Session.Current.Cache.Clear();

          // Select Id, TypeId, Title
          RecordSet rsTitle = rsMain.Select(0, 1, 2);
          foreach (Book book in rsTitle.ToEntities<Book>()) {
            ;
          }
          state = Session.Current.Cache[key];
          Assert.IsNotNull(state);
          Assert.IsTrue(state.Data.IsAvailable(2));
          Assert.IsFalse(state.Data.IsAvailable(3));
          Session.Current.Cache.Clear();

          // Select Id, TypeId, Text
          RecordSet rsText = rsMain.Select(0, 1, 3);
          foreach (Book book in rsText.ToEntities<Book>()) {
            ;
          }
          state = Session.Current.Cache[key];
          Assert.IsNotNull(state);
          Assert.IsFalse(state.Data.IsAvailable(2));
          Assert.IsTrue(state.Data.IsAvailable(3));
          Session.Current.Cache.Clear();

          // Select a.Id, a.TypeId, a.Title, b.Id, b.TypeId, b.Text
          RecordSet rsJoin = rsTitle.Alias("a").Join(rsText.Alias("b"), new Pair<int>(0, 0), new Pair<int>(1, 1));
          foreach (Book book in rsJoin.ToEntities<Book>()) {
            ;
          }
          state = Session.Current.Cache[key];
          Assert.IsNotNull(state);
          Assert.IsTrue(state.Data.IsAvailable(2));
          Assert.IsTrue(state.Data.IsAvailable(3));
          Session.Current.Cache.Clear();
        }
      }
    }
  }
}