// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using Xtensive.Core.Caching;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal sealed class RecordSetHeaderParseContext
  {
    public Domain Domain { get; private set; }

    public ICache<Key, Key> KeyCache { get; private set; }

    public Session Session { get; private set; }

    public EntityCache Cache { get; private set; }

    public RecordSetHeader Header { get; private set; }


    // Constructors

    public RecordSetHeaderParseContext(Session session, RecordSetHeader header)
    {
      Session = session;
      Cache = Session.Cache;
      Domain = Session.Domain;
      KeyCache = Domain.KeyCache;
      Header = header;
    }
  }
}