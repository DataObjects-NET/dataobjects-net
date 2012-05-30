// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.17

namespace Xtensive.Orm.Internals.KeyGenerators
{
  internal interface ICachingSequenceProvider<TValue>
  {
    CachingSequence<TValue> GetSequence(Session session);
  }
}