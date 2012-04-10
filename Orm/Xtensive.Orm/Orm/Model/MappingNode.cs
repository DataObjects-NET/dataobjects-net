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
  /// A <see cref="Node"/> that can be mapped to existing schema node.
  /// </summary>
  [Serializable]
  public abstract class MappingNode : Node
  {
    private string mappingName;

    /// <summary>
    /// Gets or sets mapping name of this instance.
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


    /// <summary>
    /// Updates the internal state of this instance.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be updated as well.</param>
    public override void UpdateState(bool recursive)
    {
      if (MappingName.IsNullOrEmpty())
        mappingName = Name;
      base.UpdateState(recursive);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    protected MappingNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="name">The name of this instance.</param>
    protected MappingNode(string name) 
      : base(name)
    {
    }
  }
}