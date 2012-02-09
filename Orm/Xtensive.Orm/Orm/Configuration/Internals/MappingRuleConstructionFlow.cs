// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.08

using System.Collections.Generic;
using System.Reflection;

namespace Xtensive.Orm.Configuration
{
  internal struct MappingRuleConstructionFlow : IMappingRuleConstructionFlow
  {
    private readonly ICollection<MappingRule> target;
    private readonly Assembly assembly;
    private readonly string @namespace;

    public void ToDatabase(string database)
    {
      target.Add(new MappingRule(assembly, @namespace, database, null));
    }

    public void ToSchema(string schema)
    {
      target.Add(new MappingRule(assembly, @namespace, null, schema));
    }

    public void To(string database, string schema)
    {
      target.Add(new MappingRule(assembly, @namespace, database, schema));
    }

    public MappingRuleConstructionFlow(ICollection<MappingRule> target, Assembly assembly, string @namespace)
    {
      this.target = target;
      this.assembly = assembly;
      this.@namespace = @namespace;
    }
  }
}