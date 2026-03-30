// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.08.07

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Tuples;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Tests.Storage.BookAuthorModel;

namespace Xtensive.Orm.Tests.Rse
{
  public class HeaderParseTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.RegisterCaching(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Storage.BookAuthorModel");
      return config;
    }

    [Test]
    public void MainTest()
    {
      Key key;
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Book book = new Book {Title = "Title", Text = "Text"};
          key = book.Key;
          Session.Current.SaveChanges();
          t.Complete();
        }
      }
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          EntityState state = Session.Current.EntityStateCache[key, true];
          Assert.That(state, Is.Null);
          IndexInfo ii = Domain.Model.Types[typeof (Book)].Indexes.PrimaryIndex;

          var parameterContext = new ParameterContext();
          // Select *
          CompilableProvider rsMain = ii.GetQuery();
          UpdateCache(session, rsMain.GetRecordSetReader(session, parameterContext));
          state = Session.Current.EntityStateCache[key, true];
          Assert.That(state, Is.Not.Null);
          Assert.That(state.Tuple.GetFieldState(2).IsAvailable(), Is.True);
          Assert.That(state.Tuple.GetFieldState(3).IsAvailable(), Is.True);
          ResetState(state);

          // Select Id, TypeId, Title
          CompilableProvider rsTitle = rsMain.Select(new[] { 0, 1, 2 });
          UpdateCache(session, rsTitle.GetRecordSetReader(session, parameterContext));
          state = Session.Current.EntityStateCache[key, true];
          Assert.That(state, Is.Not.Null);
          Assert.That(state.Tuple.GetFieldState(2).IsAvailable(), Is.True);
          Assert.That(state.Tuple.GetFieldState(3).IsAvailable(), Is.False);
          ResetState(state);

          // Select Id, TypeId, Text
          CompilableProvider rsText = rsMain.Select(new[] { 0, 1, 3 });
          UpdateCache(session, rsText.GetRecordSetReader(session, parameterContext));
          state = Session.Current.EntityStateCache[key, true];
          Assert.That(state, Is.Not.Null);
          Assert.That(state.Tuple.GetFieldState(2).IsAvailable(), Is.False);
          Assert.That(state.Tuple.GetFieldState(3).IsAvailable(), Is.True);
          ResetState(state);

          // Select a.Id, a.TypeId, a.Title, b.Id, b.TypeId, b.Text
          CompilableProvider rsJoin = rsTitle.Alias("a").Join(rsText.Alias("b"), new Pair<int>(0, 0), new Pair<int>(1, 1));
          UpdateCache(session, rsJoin.GetRecordSetReader(session, parameterContext));
          state = Session.Current.EntityStateCache[key, true];
          Assert.That(state, Is.Not.Null);
          Assert.That(state.Tuple.GetFieldState(2).IsAvailable(), Is.True);
          Assert.That(state.Tuple.GetFieldState(3).IsAvailable(), Is.True);
          ResetState(state);
        }
      }
    }

    private void UpdateCache(Session session, RecordSetReader source)
    {
      var reader = Domain.EntityDataReader;
      foreach (var record in reader.Read(source.ToEnumerable(), source.Header, session)) {
        for (int i = 0; i < record.Count; i++) {
          var key = record.GetKey(i);
          if (key==null)
            continue;
          var tuple = record.GetTuple(i);
          if (tuple==null)
            continue;
          session.UpdateStateInCache(key, tuple);
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