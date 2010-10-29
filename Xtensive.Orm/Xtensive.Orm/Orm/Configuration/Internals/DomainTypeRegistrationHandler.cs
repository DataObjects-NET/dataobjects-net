// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.21

using System;
using Xtensive.Collections;

namespace Xtensive.Orm.Configuration.Internals
{
  /// <summary>
  /// <see cref="ITypeRegistrationProcessor"/> for processing <see cref="SessionBound"/> 
  /// and <see cref="IEntity"/> descendants registration in 
  /// <see cref="DomainConfiguration.Types"/> registry.
  /// </summary>
  /// <remarks>This implementation provides topologically sorted list 
  /// of <see cref="Type"/>s.</remarks>
  [Serializable]
  public sealed class DomainTypeRegistrationHandler : TypeRegistrationProcessorBase
  {
    private readonly static Type objectType = typeof (object);

    /// <inheritdoc/>
    public override Type BaseType
    {
      get { return objectType; }
    }

    /// <inheritdoc/>
    protected override void Process(TypeRegistry registry, TypeRegistration registration, Type type)
    {
      if (type==null || registry.Contains(type))
        return;
      if (!DomainTypeRegistry.IsInterestingType(type))
        return;
      // The type is interesting; 
      // If it is a persistent type, let's register all its bases
      if (DomainTypeRegistry.IsPersistentType(type))
        Process(registry, registration, type.BaseType);
      Type[] interfaces = type.FindInterfaces(
        (_type, filterCriteria) => DomainTypeRegistry.IsPersistentType(_type), type);
      for (int index = 0; index < interfaces.Length; index++)
        Process(registry, registration, interfaces[index]);
      // Final registration
      base.Process(registry, registration, type);
    }
  }
}