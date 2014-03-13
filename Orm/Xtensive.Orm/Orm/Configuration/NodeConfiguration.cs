﻿// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.13

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Storage node configuration.
  /// </summary>
  [Serializable]
  public class NodeConfiguration : LockableBase, ICloneable
  {
    private string nodeId;
    private string connectionInitializationSql;
    private ConnectionInfo connectionInfo;
    private IDictionary<string, string> schemaMapping;
    private IDictionary<string, string> databaseMapping;

    /// <summary>
    /// Gets or sets node identifier.
    /// </summary>
    public string NodeId
    {
      get { return nodeId; }
      set
      {
        this.EnsureNotLocked();
        nodeId = value;
      }
    }

    /// <summary>
    /// Gets or sets connection information.
    /// </summary>
    public ConnectionInfo ConnectionInfo
    {
      get { return connectionInfo; }
      set
      {
        this.EnsureNotLocked();
        connectionInfo = value;
      }
    }

    /// <summary>
    /// Gets or sets connection initialization SQL code.
    /// </summary>
    public string ConnectionInitializationSql
    {
      get { return connectionInitializationSql; }
      set
      {
        this.EnsureNotLocked();
        connectionInitializationSql = value;
      }
    }

    /// <summary>
    /// Gets schema mapping.
    /// </summary>
    public IDictionary<string, string> SchemaMapping
    {
      get { return schemaMapping; }
    }

    /// <summary>
    /// Gets database mapping.
    /// </summary>
    public IDictionary<string, string> DatabaseMapping
    {
      get { return databaseMapping; }
    }

    public override void Lock(bool recursive)
    {
      base.Lock(recursive);

      schemaMapping = new ReadOnlyDictionary<string, string>(schemaMapping);
      databaseMapping = new ReadOnlyDictionary<string, string>(databaseMapping);
    }

    /// <summary>
    /// Creates clone of this instance.
    /// </summary>
    /// <returns>Clone of this instance.</returns>
    public object Clone()
    {
      var clone = new NodeConfiguration {
        nodeId = nodeId,
        connectionInfo = connectionInfo,
        connectionInitializationSql = connectionInitializationSql,
        databaseMapping = new Dictionary<string, string>(databaseMapping),
        schemaMapping = new Dictionary<string, string>(schemaMapping),
      };
      return clone;
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public NodeConfiguration()
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="nodeId">Node identifier.</param>
    public NodeConfiguration(string nodeId)
    {
      this.nodeId = nodeId;
    }
  }
}