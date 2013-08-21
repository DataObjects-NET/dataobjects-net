// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  internal sealed class ReferenceRegistry
  {
    public AssemblyNameReference CoreAssembly { get; set; }
    public AssemblyNameReference OrmAssembly { get; set; }

    public TypeReference Tuple { get; set; }

    public TypeReference Session { get; set; }
    public TypeReference EntityState { get; set; }
    public TypeReference FieldInfo { get; set; }
    public TypeReference Persistent { get; set; }

    public MethodReference ProcessedByWeaverAttributeConstructor { get; set; }
    public MethodReference EntityTypeAttributeConstructor { get; set; }
    public MethodReference StructureTypeAttributeConstructor { get; set; }

    public TypeReference StreamingContext { get; set; }
    public TypeReference SerializationInfo { get; set; }

    
  }
}