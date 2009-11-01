// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.13

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Model
{
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

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingNode"/> class.
    /// </summary>
    protected MappingNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingNode"/> class.
    /// </summary>
    /// <param name="name">The name of this instance.</param>
    protected MappingNode(string name) : base(name)
    {
    }
  }
}