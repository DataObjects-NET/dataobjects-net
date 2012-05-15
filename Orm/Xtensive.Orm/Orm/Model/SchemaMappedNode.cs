// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.10

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A <see cref="Node"/> that is mapped to existing database schema node.
  /// </summary>
  [Serializable]
  public abstract class SchemaMappedNode : MappedNode
  {
    private string mappingDatabase;
    private string mappingSchema;

    /// <summary>
    /// Gets or sets database this node is mapped to.
    /// </summary>
    public string MappingDatabase
    {
      get { return mappingDatabase; }
      set
      {
        this.EnsureNotLocked();
        mappingDatabase = value;
      }
    }

    /// <summary>
    /// Gets or sets schema this node is mapped to.
    /// </summary>
    public string MappingSchema
    {
      get { return mappingSchema; }
      set
      {
        this.EnsureNotLocked();
        mappingSchema = value;
      }
    }


    // Constructors

    /// <summary>
    /// Creates new instance of this class.
    /// </summary>
    protected SchemaMappedNode()
    {
    }

    /// <summary>
    /// Creates new instance of this class.
    /// </summary>
    /// <param name="name">Node name</param>
    protected SchemaMappedNode(string name)
      : base(name)
    {
    }
  }
}