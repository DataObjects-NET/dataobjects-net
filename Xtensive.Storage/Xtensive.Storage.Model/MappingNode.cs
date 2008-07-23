// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.13

using System;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Model
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
        Validate(value);
        mappingName = value;
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      if (MappingName.IsNullOrEmpty())
        mappingName = Name;
      base.Lock(recursive);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected MappingNode()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The name of this instance.</param>
    protected MappingNode(string name) : base(name)
    {
    }
  }
}