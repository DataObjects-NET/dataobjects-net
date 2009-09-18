// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2009.09.17

using System;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0408_EntitySetNullReference_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  [Flags]
  public enum HistoryEntryVisibility
  {
    EndUser,
    AdministratorUser,
    Debugger,
  }

  [HierarchyRoot]
  public class HistoryEntry : Entity
  {
    public const HistoryEntryVisibility VisibilityForAll = HistoryEntryVisibility.EndUser | HistoryEntryVisibility.AdministratorUser | HistoryEntryVisibility.Debugger;

    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string What { get; set; }

    [Field]
    public DateTime When { get; set; }

    [Field]
    public HistoryEntryVisibility Visibility { get; set; }

    [Field]
    public Document Owner { get; set; }
  }

  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field, Association(PairTo = "Owner", OnOwnerRemove = OnRemoveAction.Clear)]
    public EntitySet<HistoryEntry> HistoryEntries { get; private set; }

    public void AddHistoryEntry(string what, HistoryEntryVisibility visibility)
    {
      var historyEntry = new HistoryEntry {What = what, Visibility = visibility, When = DateTime.Now};
      HistoryEntries.Add(historyEntry);
      LogTemplate<Xtensive.Storage.Log>.Debug("History entry added for {0}. {1}What : {2}", ToString(), Environment.NewLine, what);
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0408_EntitySetNullReference : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (HistoryEntry).Assembly, typeof (HistoryEntry).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      var sessionConfiguration = new SessionConfiguration {
        Options = SessionOptions.AutoTransactions
      };
      Key key;
      using (var s = Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var document = new Document();
          key = document.Key;
          t.Complete();
        }
      }
      using (var s = Session.Open(Domain, sessionConfiguration)) {
          var document = Query<Document>.Single(key);
          document.AddHistoryEntry("test", HistoryEntryVisibility.AdministratorUser);
          document.AddHistoryEntry("test2", HistoryEntryVisibility.AdministratorUser);
      }
    }
  }
}