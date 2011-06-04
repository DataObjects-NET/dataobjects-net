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

    protected virtual IPrincipal Validate(string username, string password)
    {
      var service = Session.Services.Get<IEncryptionService>();
      var encrypted = service.Encrypt(password);

      // GenericPrincipal is not found in the model, let's find its descendant
      var model = Session.Domain.Model;
      var rootPrincipalType = model.Hierarchies
        .Select(h => h.Root.UnderlyingType)
        .FirstOrDefault(t => typeof (GenericPrincipal).IsAssignableFrom(t));

      if (rootPrincipalType != null) {
        var gmi = GetType().GetMethod("ExecuteValidate", BindingFlags.NonPublic|BindingFlags.Instance);
        var mi = gmi.MakeGenericMethod(rootPrincipalType);
        return (IPrincipal) mi.Invoke(this, new object[] {username, encrypted});
      }

      throw new InvalidOperationException("No descendants of GenericPrincipal are found in domain model");
    }

    protected virtual IPrincipal ExecuteValidate<T>(string username, string password) where T: GenericPrincipal
    {
      return Session.Query.All<T>()
          .SingleOrDefault(u => u.Name == username && u.Password == password);
    }

    [ServiceConstructor]
    public GenericPrincipalValidationService(Session session)
      : base(session)
    {
    }
  }
}