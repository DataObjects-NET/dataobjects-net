// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.21

using System;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Configuration.Internals
{
  /// <summary>
  /// <see cref="ITypeRegistrationHandler"/> for processing <see cref="Persistent"/> 
  /// and <see cref="IEntity"/> descendants registration in 
  /// <see cref="DomainConfiguration.Types"/> registry.
  /// </summary>
  /// <remarks>This implementation provides topologically sorted list 
  /// of <see cref="Type"/>s.</remarks>
  [Serializable]
  public sealed class PersistentTypeRegistrationHandler : TypeRegistrationHandlerBase
  {
    private readonly static Type baseInterface = typeof (IEntity);
    private readonly static Type baseType = typeof (Persistent);

    /// <inheritdoc/>
    public Type BaseInterface
    {
      get { return baseInterface; }
    }

    /// <inheritdoc/>
    public override Type BaseType
    {
      get { return baseType; }
    }

    /// <inheritdoc/>
    protected override void Process(TypeRegistry registry, TypeRegistration registration, Type type)
    {
      if (registry.Contains(type))
        return;
      if (type.IsClass && type.BaseType != BaseType)
        Process(registry, registration, type.BaseType);
      Type[] interfaces = type.FindInterfaces(
        (typeObj, filterCriteria) => BaseInterface.IsAssignableFrom(typeObj), type);
      for (int index = 0; index < interfaces.Length; index++)
        Process(registry, registration, interfaces[index]);
      base.Process(registry, registration, type);
    }
  }
}