using System;

namespace Xtensive.Orm.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  [Serializable]
  [HierarchyRoot]
  public class HistoryEntry : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    public const HistoryEntryVisibility VisibilityForAll = HistoryEntryVisibility.EndUser | HistoryEntryVisibility.AdministratorUser | HistoryEntryVisibility.Debugger;

    [Field(Length = Int32.MaxValue)]
    public string What { get; set; }

    [Field]
    public DateTime When { get; set; }

    [Field]
    public HistoryEntryVisibility Visibility { get; set; }

    [Field]
    public Document Owner { get; set; }

  }
}
