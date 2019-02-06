// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.12.29

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests
{
  public class SessionTransactionOpener : IDisposable
  {
    private readonly bool shouldDisposeDomain;

    public Domain Domain { get; private set; }
    public Session Session { get; private set; }
    public TransactionScope TransactionScope { get; private set; }

    public QueryEndpoint Query
    {
      get { return Session.Query; }
    }

    public void Dispose()
    {
      TransactionScope.Dispose();
      Session.Dispose();
      if (shouldDisposeDomain)
        Domain.Dispose();
    }

    public SessionTransactionOpener(DomainConfiguration configuration, SessionConfiguration sessionConfiguration = null)
      : this(Orm.Domain.Build(configuration), sessionConfiguration)
    {
      shouldDisposeDomain = true;
    }

    public SessionTransactionOpener(Domain domain, SessionConfiguration sessionConfiguration = null)
    {
      if (sessionConfiguration == null)
      {
        sessionConfiguration = domain.Configuration.Sessions.Default.Clone();
        sessionConfiguration.Options |= SessionOptions.AutoActivation;
      }

      Domain = domain;
      Session = Domain.OpenSession(sessionConfiguration);
      TransactionScope = Session.OpenTransaction();
    }
  }
}