// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.23

using System;
using System.Collections.Generic;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal static class AssociationBrowser
  {
    public static IEnumerable<Entity> FindReferencingObjects(AssociationInfo association, Entity referencedEntity)
    {
      throw new NotImplementedException();
    }
  }
}