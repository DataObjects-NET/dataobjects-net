// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.07

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
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
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Book book = new Book {Title = "Title", Text = "Text"};
          key = book.Key;
          Session.Current.Persist();
          t.Complete();
        }
      }
      using (Session.Open(Domain)) {
        Session session = Session.Current;
        using (Transaction.Open()) {
          EntityState state = Session.Current.EntityStateCache[key, true];
          Assert.IsNull(state);
          IndexInfo ii = Domain.Model.Types[typeof (Book)].Indexes.PrimaryIndex;

          // Select *
          RecordQuery rsMain = ii.ToRecordQuery();
          session.UpdateCache(rsMain.ToRecordSet(session));
          state = Session.Current.EntityStateCache[key, true];
          Assert.IsNotNull(state);
          Assert.IsTrue(state.Tuple.GetFieldState(2).IsAvailable());
          Assert.IsTrue(state.Tuple.GetFieldState(3).IsAvailable());
          ResetState(state);

          // Select Id, TypeId, Title
          RecordQuery rsTitle = rsMain.Select(0, 1, 2);
          session.UpdateCache(rsTitle.ToRecordSet(session));
          state = Session.Current.EntityStateCache[key, true];
          Assert.IsNotNull(state);
          Assert.IsTrue(state.Tuple.GetFieldState(2).IsAvailable());
          Assert.IsFalse(state.Tuple.GetFieldState(3).IsAvailable());
          ResetState(state);

          // Select Id, TypeId, Text
          RecordQuery rsText = rsMain.Select(0, 1, 3);
          session.UpdateCache(rsText.ToRecordSet(session));
          state = Session.Current.EntityStateCache[key, true];
          Assert.IsNotNull(state);
          Assert.IsFalse(state.Tuple.GetFieldState(2).IsAvailable());
          Assert.IsTrue(state.Tuple.GetFieldState(3).IsAvailable());
          ResetState(state);

          // Select a.Id, a.TypeId, a.Title, b.Id, b.TypeId, b.Text
          RecordQuery rsJoin = rsTitle.Alias("a").Join(rsText.Alias("b"), new Pair<int>(0, 0), new Pair<int>(1, 1));
          session.UpdateCache(rsJoin.ToRecordSet(session));
          state = Session.Current.EntityStateCache[key, true];
          Assert.IsNotNull(state);
          Assert.IsTrue(state.Tuple.GetFieldState(2).IsAvailable());
          Assert.IsTrue(state.Tuple.GetFieldState(3).IsAvailable());
          ResetState(state);
        }
      }
    }

    private static void ResetState(EntityState state)
    {
      typeof (EntityState).InvokeMember(
        "Invalidate",
        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
        null, state, new object[0]);
    }
  }
}