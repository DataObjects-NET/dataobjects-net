// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.03.16

using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm;
using Xtensive.Practices.Security.Configuration;

namespace Xtensive.Practices.Security
{
  [Service(typeof(IPrincipalValidationService), Singleton = true, Name = "default")]
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

    protected virtual IPrincipal Validate(string username, string password)
    {
      var config = Session.GetSecurityConfiguration();
      var service = Session.Services.Get<IHashingService>(config.HashingServiceName);

      if (service == null)
        throw new InvalidOperationException(string.Format("Hashing service by name {0} is not found. Check Xtensive.Security configuration", config.HashingServiceName));

      // GenericPrincipal is not found in the model, let's find its descendant
      var model = Session.Domain.Model;
      var rootPrincipalType = model.Hierarchies
        .Select(h => h.Root.UnderlyingType)
        .FirstOrDefault(t => typeof (GenericPrincipal).IsAssignableFrom(t));

      if (rootPrincipalType == null)
        throw new InvalidOperationException("No descendants of GenericPrincipal type are found in domain model");

      var candidate = (Session.Query.All(rootPrincipalType) as IQueryable<GenericPrincipal>)
        .Where(u => u.Name == username)
        .SingleOrDefault();

      if (candidate != null && service.VerifyHash(password, candidate.PasswordHash))
        return candidate;

      return null;
    }

    [ServiceConstructor]
    public GenericPrincipalValidationService(Session session)
      : base(session)
    {
    }
  }
}