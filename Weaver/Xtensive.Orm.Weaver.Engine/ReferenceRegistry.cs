// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System.Collections.Generic;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  internal sealed class ReferenceRegistry
  {
    public AssemblyNameReference CoreAssembly { get; set; }
    public AssemblyNameReference OrmAssembly { get; set; }

    // mscorlib
    public TypeReference StreamingContext { get; set; }
    public TypeReference SerializationInfo { get; set; }

    public MethodReference CompilerGeneratedAttributeConstructor { get; set; }

    // Xtensive.Core
    public TypeReference Tuple { get; set; }

    // Xtensive.Orm
    public TypeReference Session { get; set; }
    public TypeReference EntityState { get; set; }
    public TypeReference FieldInfo { get; set; }
    public TypeReference Persistent { get; set; }

    public MethodReference ProcessedByWeaverAttributeConstructor { get; set; }
    public MethodReference EntityTypeAttributeConstructor { get; set; }
    public MethodReference EntitySetTypeAttributeConstructor { get; set; }
    public MethodReference EntityInterfaceAttributeConstructor { get; set; }
    public MethodReference StructureTypeAttributeConstructor { get; set; }

    public MethodReference PersistentGetterDefinition { get; set; }
    public MethodReference PersistentSetterDefinition { get; set; }

    public IDictionary<TypeIdentity, GenericInstanceMethod> PersistentGetters { get; set; }
    public IDictionary<TypeIdentity, GenericInstanceMethod> PersistentSetters { get; set; }

    public ReferenceRegistry()
    {
      PersistentGetters = new Dictionary<TypeIdentity, GenericInstanceMethod>();
      PersistentSetters = new Dictionary<TypeIdentity, GenericInstanceMethod>();
    }
  }
}