// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  internal sealed class PersistentType
  {
    public PersistentTypeKind Kind { get; set; }

    public TypeDefinition Definition { get; set; }

    public ISet<PersistentProperty> Properties { get; set; }

    public PersistentType(TypeDefinition definition, PersistentTypeKind kind, IEnumerable<PersistentProperty> properties)
    {
      if (definition==null)
        throw new ArgumentNullException("definition");
      if (properties==null)
        throw new ArgumentNullException("properties");

      Definition = definition;
      Kind = kind;
      Properties = new HashSet<PersistentProperty>(properties);
    }
  }
}