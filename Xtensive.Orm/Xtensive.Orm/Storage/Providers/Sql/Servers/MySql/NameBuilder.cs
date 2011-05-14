// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.14

using Xtensive.Orm.Configuration;

namespace Xtensive.Storage.Providers.Sql.Servers.MySql
{
  public class NameBuilder : Providers.NameBuilder
  {
    public override string BuildIndexName(Orm.Building.Definitions.TypeDef type, Orm.Building.Definitions.IndexDef index)
    {
      if (index.IsPrimary)
        return "PRIMARY";
      return base.BuildIndexName(type, index);
    }

//    protected internal override void Initialize(NamingConvention namingConvention)
//    {
//      namingConvention.LetterCasePolicy = LetterCasePolicy.Lowercase;
//      base.Initialize(namingConvention);
//    }
  }
}