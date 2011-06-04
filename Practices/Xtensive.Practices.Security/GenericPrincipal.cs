// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  public abstract class GenericPrincipal : Principal
  {
    [Field(Length = 50)]
    public string Password { get; protected set; }

    public virtual void SetPassword(string password)
    {
      var service = Session.Services.Get<IEncryptionService>();
      Password = service.Encrypt(password);
    }

    protected GenericPrincipal(Session session)
      : base(session)
    {}
  }
}