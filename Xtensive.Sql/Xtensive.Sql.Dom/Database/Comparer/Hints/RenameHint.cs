// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer.Hints
{
  [Serializable]
  public class RenameHint : ComparisonHintBase
  {
    private readonly string oldName;

    /// <summary>
    /// Gets old node name.
    /// </summary>
    public string OldName
    {
      get { return oldName; }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RenameHint(Type type, string name, string oldName)
      : base(type, name)
    {
      this.oldName = oldName;
    }
  }
}