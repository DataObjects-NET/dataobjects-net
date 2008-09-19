// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal sealed class RecordSetHeaderParseContext
  {
    public Session Session { get; private set; }

    public Domain Domain
    {
      get { return Session.Domain; }
    }

    public RecordSetHeader Header { get; private set; }


    // Constructors

    public RecordSetHeaderParseContext(Session session, RecordSetHeader header)
    {
      Session = session;
      Header = header;
    }
  }
}