// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using Xtensive.Storage.Model;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal abstract class ActionProcessor
  {
    public abstract void Process(RemovalContext context, AssociationInfo association, Entity referencingObject, Entity referencedObject);
  }
}