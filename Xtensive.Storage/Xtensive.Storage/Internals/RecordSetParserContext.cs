// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal sealed class RecordSetParserContext
  {
    public Domain Domain { get; private set; }

    public Session Session { get; private set; }

    public RecordSetHeader Header { get; private set; }


    // Constructors

    public RecordSetParserContext(RecordSet source)
    {
      Session = Session.Current;
      Domain = Session.Domain;
      Header = source.Header;
    }
  }
}