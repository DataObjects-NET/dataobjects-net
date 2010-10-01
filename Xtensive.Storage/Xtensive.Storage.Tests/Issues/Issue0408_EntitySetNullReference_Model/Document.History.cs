﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Storage;

namespace Xtensive.Storage.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  public partial class Document : Entity
  {
    [Field, Association(PairTo = "Owner", OnOwnerRemove = OnRemoveAction.Clear)]
    public EntitySet<HistoryEntry> HistoryEntries { get; private set; }

    public void AddHistoryEntry(string what, HistoryEntryVisibility visibility)
    {
      HistoryEntries.Add(new HistoryEntry { What = what, Visibility = visibility, When = DateTime.Now });
      Log.Debug("History entry added for {0}. {1}What : {2}", ToString(), Environment.NewLine, what);
    }
  }
}
