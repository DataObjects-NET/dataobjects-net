// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.13

using System;
using Xtensive.Core;
using Xtensive.Helpers;


namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A <see cref="Node"/> that can be mapped to existing named node.
  /// </summary>
  [Serializable]
  public abstract class MappedNode : Node
  {
    private string mappingName;

    /// <summary>
    /// Gets or sets name of the database object this node maps to.
    /// </summary>
    public string MappingName
    {
      get { return mappingName; }
      set
      {
        this.EnsureNotLocked();
        ValidateName(value);
        mappingName = value;
      }
    }

    /// <inheritdoc/>
    public override void UpdateState()
    {
      if (MappingName.IsNullOrEmpty())
        mappingName = Name;
      base.UpdateState();
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    protected MappedNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="name">The name of this instance.</param>
    protected MappedNode(string name) 
      : base(name)
    {
    }
  }
}