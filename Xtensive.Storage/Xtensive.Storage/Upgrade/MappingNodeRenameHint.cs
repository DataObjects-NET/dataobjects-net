// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.28

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Upgrade
{
  [Serializable]
  internal class MappingNodeRenameHint
  {
    public string SourceMappingName { get; private set; }

    public MappingNode ResultMappingNode { get; private set;}

    public MappingNodeRenameHint(string sourceMappingName, MappingNode resultMappingNode)
    {
      SourceMappingName = sourceMappingName;
      ResultMappingNode = resultMappingNode;
    }
  }
}