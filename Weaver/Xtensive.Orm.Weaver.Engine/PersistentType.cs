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

    public ISet<PropertyDefinition> KeyProperties { get; set; }

    public ISet<PropertyDefinition> AllProperties { get; set; }

    public PersistentType(TypeDefinition definition, PersistentTypeKind kind,
      IEnumerable<PropertyDefinition> keyProperties, IEnumerable<PropertyDefinition> allProperties)
    {
      if (definition==null)
        throw new ArgumentNullException("definition");
      if (allProperties==null)
        throw new ArgumentNullException("allProperties");

      Definition = definition;
      Kind = kind;
      KeyProperties = new HashSet<PropertyDefinition>(keyProperties);
      AllProperties = new HashSet<PropertyDefinition>(allProperties);
    }
  }
}