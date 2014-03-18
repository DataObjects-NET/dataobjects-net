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

namespace Xtensive.Orm.Providers
{
  internal sealed class StorageNode
  {
    public string Id { get { return Configuration.NodeId; } }

    public NodeConfiguration Configuration { get; private set; }

    public ModelMapping Mapping { get; private set; }

    public ConcurrentDictionary<object, object> InternalQueryCache { get; private set; }

    public ConcurrentDictionary<SequenceInfo, object> KeySequencesCache { get; private set; }

    public ConcurrentDictionary<PersistRequestBuilderTask, ICollection<PersistRequest>> PersistRequestCache { get; private set; }


    // Constructors

    public StorageNode(NodeConfiguration configuration, ModelMapping mapping)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      ArgumentValidator.EnsureArgumentNotNull(mapping, "mapping");

      Configuration = configuration;
      Mapping = mapping;
      KeySequencesCache = new ConcurrentDictionary<SequenceInfo, object>();
      PersistRequestCache = new ConcurrentDictionary<PersistRequestBuilderTask, ICollection<PersistRequest>>();
      InternalQueryCache = new ConcurrentDictionary<object, object>();
    }
  }
}