// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System;
using Xtensive.Orm;
using Xtensive.Practices.Security.Configuration;

namespace Xtensive.Practices.Security
{
  public abstract class GenericPrincipal : Principal
  {
    [Field(Length = 128)]
    public string PasswordHash { get; protected set; }

    public virtual void SetPassword(string password)
    {
      var config = Session.GetSecurityConfiguration();
      var service = Session.Services.Get<IHashingService>(config.HashingServiceName);

      if (service == null)
        throw new InvalidOperationException(string.Format("Hashing service by name {0} is not found. Check Xtensive.Security configuration", config.HashingServiceName));

      PasswordHash = service.ComputeHash(password);
    }

    protected GenericPrincipal(Session session)
      : base(session)
    {}
  }
}