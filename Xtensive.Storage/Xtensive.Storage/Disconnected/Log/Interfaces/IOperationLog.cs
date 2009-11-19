// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using System.Collections.Generic;

namespace Xtensive.Storage.Disconnected.Log
{
  public interface IOperationLog : IEnumerable<IOperation>
  {
    HashSet<Key> GetKeysForRemap();
    void RegisterKeyForRemap(Key key);
    void Register(IOperation operation);
    void Append(IOperationLog source);
    KeyMapping Apply(Session session);
  }
}