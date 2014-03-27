// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.13

using System.Collections.Concurrent;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm
{
  /// <summary>
  /// Storage node.
  /// </summary>
  public sealed class StorageNode
  {
    /// <summary>
    /// Gets node identifier.
    /// </summary>
    public string Id { get { return Configuration.NodeId; } }

    /// <summary>
    /// Gets node configuration.
    /// </summary>
    public NodeConfiguration Configuration { get; private set; }

    /// <summary>
    /// Gets model mapping.
    /// </summary>
    public ModelMapping Mapping { get; private set; }

    /// <summary>
    /// Gets type identifier registry.
    /// </summary>
    public TypeIdRegistry TypeIdRegistry { get; private set; }

    internal ConcurrentDictionary<object, object> InternalQueryCache { get; private set; }

    internal ConcurrentDictionary<SequenceInfo, object> KeySequencesCache { get; private set; }

    internal ConcurrentDictionary<PersistRequestBuilderTask, ICollection<PersistRequest>> PersistRequestCache { get; private set; }


    // Constructors

    internal StorageNode(NodeConfiguration configuration, ModelMapping mapping, TypeIdRegistry typeIdRegistry)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      ArgumentValidator.EnsureArgumentNotNull(mapping, "mapping");
      ArgumentValidator.EnsureArgumentNotNull(typeIdRegistry, "typeIdRegistry");

      Configuration = configuration;
      Mapping = mapping;
      TypeIdRegistry = typeIdRegistry;

      KeySequencesCache = new ConcurrentDictionary<SequenceInfo, object>();
      PersistRequestCache = new ConcurrentDictionary<PersistRequestBuilderTask, ICollection<PersistRequest>>();
      InternalQueryCache = new ConcurrentDictionary<object, object>();
    }
  }
}