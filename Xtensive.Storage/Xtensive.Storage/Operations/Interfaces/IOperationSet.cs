// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using System.Collections.Generic;

namespace Xtensive.Storage.Operations
{
  public interface IOperationSet : IEnumerable<IOperation>
  {
    HashSet<Key> GetKeysForRemap();
    void RegisterKeyForRemap(Key key);
    void Register(IOperation operation);
    void Register(IOperationSet source);
    KeyMapping Apply(Session session);
  }
}