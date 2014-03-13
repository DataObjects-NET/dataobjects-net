// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.13

using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Providers
{
  internal sealed class StorageNode
  {
    public string Id { get { return Configuration.NodeId; } }

    public NodeConfiguration Configuration { get; private set; }

    public ModelMapping Mapping { get; private set; }


    // Constructors

    public StorageNode(NodeConfiguration configuration, ModelMapping mapping)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      ArgumentValidator.EnsureArgumentNotNull(mapping, "mapping");

      Configuration = configuration;
      Mapping = mapping;
    }
  }
}