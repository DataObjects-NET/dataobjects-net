// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;


namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A collection of <see cref="HierarchyInfo"/> objects.
  /// </summary>
  [Serializable]
  public sealed class HierarchyInfoCollection : NodeCollection<HierarchyInfo>
  {

    /// <summary>
    /// Initializes a new instance of the <see cref="HierarchyInfoCollection"/> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="name">The name.</param>
    public HierarchyInfoCollection(Node owner, string name)
      : base(owner, name)
    {
    }
  }
}