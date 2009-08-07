// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.07

namespace Xtensive.Storage.Providers.Sql.Servers.Oracle
{
  /// <summary>
  /// A <see cref="Providers.NameBuilder"/> for Oracle RDBMS.
  /// </summary>
  public class NameBuilder : Providers.NameBuilder
  {
    private const int MaxNameLength = 30;
    
    public override string ApplyNamingRules(string name)
    {
      name = base.ApplyNamingRules(name);
      if (name.Length <= MaxNameLength)
        return name;
      string hash = BuildHash(name);
      return name.Substring(0, MaxNameLength - hash.Length) + hash;
    }
  }
}