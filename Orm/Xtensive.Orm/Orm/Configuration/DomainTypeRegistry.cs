// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.03

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using System.Linq;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// An implementation of <see cref=" TypeRegistry"/>
  /// used by the <see cref="DomainConfiguration"/>.
  /// </summary>
  [Serializable]
  public class DomainTypeRegistry : TypeRegistry
  {
    internal readonly static Type iEntityType = typeof (IEntity);
    internal readonly static Type persistentType = typeof (Persistent);
    internal readonly static Type iDomainServiceType = typeof (IDomainService);
    internal readonly static Type iSessionServiceType = typeof (ISessionService);
    internal readonly static Type iModuleType = typeof (IModule);
    internal readonly static Type iUpgradeHandlerType = typeof (IUpgradeHandler);
    internal readonly static Type keyGeneratorType = typeof (KeyGenerator);

    /// <summary>
    /// Gets all the registered persistent types.
    /// </summary>
    public IEnumerable<Type> PersistentTypes { 
      get {
        return this.Where(IsPersistentType);
      }
    }

    /// <summary>
    /// Gets all the registered <see cref="Domain"/>-level service types.
    /// </summary>
    public IEnumerable<Type> DomainServices { 
      get {
        return this.Where(IsDomainService);
      }
    }

    /// <summary>
    /// Gets all the registered <see cref="Session"/>-level service types.
    /// </summary>
    public IEnumerable<Type> SessionServices { 
      get {
        return this.Where(IsSessionService);
      }
    }

    /// <summary>
    /// Gets all the registered <see cref="IModule"/> implementations.
    /// </summary>
    public IEnumerable<Type> Modules { 
      get {
        return this.Where(IsModule);
      }
    }

    /// <summary>
    /// Gets all the registered <see cref="IUpgradeHandler"/> implementations.
    /// </summary>
    public IEnumerable<Type> UpgradeHandlers { 
      get {
        return this.Where(IsUpgradeHandler);
      }
    }

    /// <summary>
    /// Gets all the registered <see cref="KeyGenerator"/>
    /// and <see cref="TemporaryKeyGenerator"/>.
    /// </summary>
    public IEnumerable<Type> KeyGenerators { 
      get {
        return this.Where(IsKeyGenerator);
      }
    }

    /// <summary>
    /// Gets all the registered compiler containers.
    /// </summary>
    public IEnumerable<Type> CompilerContainers { 
      get {
        return this.Where(IsCompilerContainer);
      }
    }

    #region IsXxx method group

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is any of types <see cref="DomainTypeRegistry"/> 
    /// is interested in.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsInterestingType(Type type)
    {
      return 
        IsPersistentType(type) ||
        IsDomainService(type) ||
        IsSessionService(type) ||
        IsModule(type) ||
        IsUpgradeHandler(type) ||
        IsKeyGenerator(type) ||
        IsCompilerContainer(type);
    }

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is persistent type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsPersistentType(Type type)
    {
      if (persistentType.IsAssignableFrom(type) && persistentType!=type)
        return true;
      if (iEntityType.IsAssignableFrom(type))
        return true;
      return false;
    }

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is <see cref="Domain"/>-level service.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsDomainService(Type type)
    {
      if (type.IsAbstract)
        return false;
      if (iDomainServiceType.IsAssignableFrom(type) && iDomainServiceType!=type)
        return true;
      return false;
    }

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is <see cref="Session"/>-level service.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsSessionService(Type type)
    {
      if (type.IsAbstract)
        return false;
      if (iSessionServiceType.IsAssignableFrom(type) && iSessionServiceType!=type)
        return true;
      return false;
    }

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is <see cref="Domain"/> module.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsModule(Type type)
    {
      if (type.IsAbstract)
        return false;
      if (iModuleType.IsAssignableFrom(type) && iModuleType!=type)
        return true;
      return false;
    }

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is <see cref="Domain"/> upgrade handler.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsUpgradeHandler(Type type)
    {
      if (type.IsAbstract)
        return false;
      if (iUpgradeHandlerType.IsAssignableFrom(type) && iUpgradeHandlerType!=type)
        return true;
      return false;
    }

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is key generator.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsKeyGenerator(Type type)
    {
      if (type.IsAbstract)
        return false;
      if (keyGeneratorType.IsAssignableFrom(type))
        return true;
      return false;
    }

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is compiler container.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsCompilerContainer(Type type)
    {
      return type.IsDefined(typeof (CompilerContainerAttribute), false);
    }

    #endregion

    #region ICloneable members

    /// <inheritdoc/>
    public override object Clone()
    {
      return new DomainTypeRegistry(this);
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="processor">The registry action processor.</param>
    public DomainTypeRegistry(ITypeRegistrationProcessor processor)
      : base(processor)
    {
    }

    /// <inheritdoc/>
    protected DomainTypeRegistry(TypeRegistry source)
      : base(source)
    {
    }
  }
}