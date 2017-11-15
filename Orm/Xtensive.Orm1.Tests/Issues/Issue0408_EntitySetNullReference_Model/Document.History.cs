using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Orm;

namespace Xtensive.Orm.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  public partial class Document : Entity
  {
    [Field, Association(PairTo = "Owner", OnOwnerRemove = OnRemoveAction.Clear)]
    public EntitySet<HistoryEntry> HistoryEntries { get; private set; }

    public void AddHistoryEntry(string what, HistoryEntryVisibility visibility)
    {
      HistoryEntries.Add(new HistoryEntry { What = what, Visibility = visibility, When = DateTime.Now });
      TestLog.Debug("History entry added for {0}. {1}What : {2}", ToString(), Environment.NewLine, what);
    }
  }
}
