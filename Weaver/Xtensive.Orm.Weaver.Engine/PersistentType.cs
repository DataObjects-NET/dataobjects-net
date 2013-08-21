// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  internal sealed class PersistentType
  {
    public PersistentTypeKind Kind { get; set; }

    public TypeDefinition Definition { get; set; }

    public IList<PropertyDefinition> Properties { get; set; }

    public PersistentType(TypeDefinition definition, PersistentTypeKind kind, IEnumerable<PropertyDefinition> fields)
    {
      Definition = definition;
      Kind = kind;
      Properties = fields.ToList();
    }
  }
}