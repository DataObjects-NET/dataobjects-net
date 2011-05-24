// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.03.16

using System.Linq;
using System.Security.Principal;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  [Service(typeof(IPrincipalValidationService), Singleton = true)]
  public class GenericPrincipalValidationService : SessionBound, IPrincipalValidationService
  {
    public IPrincipal Validate(IIdentity identity, params object[] args)
    {
      ArgumentValidator.EnsureArgumentNotNull(identity, "identity");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(identity.Name, "identity.Name");

      return Validate(identity.Name, args);
    }

    public IPrincipal Validate(string name, params object[] args)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");

      string password = string.Empty;
      if (args != null && args.Length > 0)
        password = (string) args[0];

      return Validate(name, password);

    }

    private IPrincipal Validate(string username, string password)
    {
      var service = Session.Services.Get<IEncryptionService>();
      var encrypted = service.Encrypt(password);

      return Session.Query.All<GenericPrincipal>()
        .SingleOrDefault(u => u.Name == username && u.Password == encrypted);
    }

    [ServiceConstructor]
    public GenericPrincipalValidationService(Session session)
      : base(session)
    {
    }
  }
}