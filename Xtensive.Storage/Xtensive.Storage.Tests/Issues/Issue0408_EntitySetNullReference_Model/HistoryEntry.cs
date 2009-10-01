﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Storage;

namespace Xtensive.Storage.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
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
