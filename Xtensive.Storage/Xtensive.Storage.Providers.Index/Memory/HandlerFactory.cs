// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.02

namespace Xtensive.Storage.Providers.Index.Memory
{
  /// <summary>
  /// Handlers factory for memory index storage.
  /// </summary>
  [Provider(WellKnown.Protocol.Memory, "General storage provider for in-memory storages.")]
  public class HandlerFactory : Index.HandlerFactory
  {
  }
}