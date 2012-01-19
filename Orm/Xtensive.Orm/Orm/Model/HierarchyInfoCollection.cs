// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A collection of <see cref="HierarchyInfo"/> objects.
  /// </summary>
  [Serializable]
  public sealed class HierarchyInfoCollection : NodeCollection<HierarchyInfo>
  {
    /// <inheritdoc/>
    public HierarchyInfoCollection(Node owner, string name)
      : base(owner, name)
    {
    }
  }
}