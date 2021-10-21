// Copyright (C) 2010-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.02.03

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using System.Linq;
using Xtensive.IoC;
using Xtensive.Orm.Internals;
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
    private readonly static Type iDomainServiceType = typeof(IDomainService);
    private readonly static Type iSessionServiceType = typeof(ISessionService);
    private readonly static Type iModuleType = typeof(IModule);
    private readonly static Type iUpgradeHandlerType = typeof(IUpgradeHandler);
    private readonly static Type keyGeneratorType = typeof(KeyGenerator);
    private readonly static Type ifulltextCatalogNameBuilder = typeof(IFullTextCatalogNameBuilder);
    private readonly static Type iDbConnectionAccessorType = typeof(IDbConnectionAccessor);

    private Type[] connectionAccessors;


    /// <summary>
    /// Gets all the registered persistent types.
    /// </summary>
    public IEnumerable<Type> PersistentTypes => this.Where(IsPersistentType);

    /// <summary>
    /// Gets all the registered <see cref="Domain"/>-level service types.
    /// </summary>
    public IEnumerable<Type> DomainServices => this.Where(IsDomainService);

    /// <summary>
    /// Gets all the registered <see cref="Session"/>-level service types.
    /// </summary>
    public IEnumerable<Type> SessionServices => this.Where(IsSessionService);

    public ServiceRegistration[] ServiceRegistrations =>
      serviceRegistrations ??= SessionServices.SelectMany(ServiceRegistration.CreateAll).ToArray();

    /// <summary>
    /// Gets all the registered <see cref="IModule"/> implementations.
    /// </summary>
    public IEnumerable<Type> Modules => this.Where(IsModule);

    /// <summary>
    /// Gets all the registered <see cref="IUpgradeHandler"/> implementations.
    /// </summary>
    public IEnumerable<Type> UpgradeHandlers => this.Where(IsUpgradeHandler);

    /// <summary>
    /// Gets all the registered <see cref="KeyGenerator"/>
    /// and <see cref="TemporaryKeyGenerator"/>.
    /// </summary>
    public IEnumerable<Type> KeyGenerators => this.Where(IsKeyGenerator);

    /// <summary>
    /// Gets all the registered compiler containers.
    /// </summary>
    public IEnumerable<Type> CompilerContainers => this.Where(IsCompilerContainer);

    /// <summary>
    /// Gets all the registered catalog resolvers of full-text indexes.
    /// </summary>
    public IEnumerable<Type> FullTextCatalogResolvers => this.Where(IsFullTextCatalogNameBuilder);

    /// <summary>
    /// Gets all the registered <see cref="IDbConnectionAccessor"/> implementations.
    /// </summary>
    public IEnumerable<Type> DbConnectionAccessors
    {
      get {
        // a lot of access to this property. better to have items cached;
        if (IsLocked) {
          if(connectionAccessors == null) {
            var container = new List<Type>(10);// not so many accessors expected
            foreach (var type in this.Where(IsConnectionAccessor))
              container.Add(type);
            connectionAccessors = container.Count == 0 ? Array.Empty<Type>() : container.ToArray();
          }
          return connectionAccessors;
        }
        // if instance is not locked then there is a chance of new accessors appeared
        return this.Where(IsConnectionAccessor);
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
    public static bool IsInterestingType(Type type) =>
      IsPersistentType(type) ||
      IsDomainService(type) ||
      IsSessionService(type) ||
      IsModule(type) ||
      IsUpgradeHandler(type) ||
      IsKeyGenerator(type) ||
      IsCompilerContainer(type) ||
      IsFullTextCatalogNameBuilder(type) ||
      IsConnectionAccessor(type);

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is persistent type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsPersistentType(Type type)
    {
      if (WellKnownOrmTypes.Persistent.IsAssignableFrom(type) && WellKnownOrmTypes.Persistent != type) {
        return true;
      }

      return WellKnownOrmInterfaces.Entity.IsAssignableFrom(type);
    }

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is <see cref="Domain"/>-level service.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsDomainService(Type type)
    {
      if (type.IsAbstract) {
        return false;
      }

      return iDomainServiceType.IsAssignableFrom(type) && iDomainServiceType != type;
    }

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is <see cref="Session"/>-level service.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsSessionService(Type type)
    {
      if (type.IsAbstract) {
        return false;
      }

      return iSessionServiceType.IsAssignableFrom(type) && iSessionServiceType != type;
    }

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is <see cref="Domain"/> module.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsModule(Type type)
    {
      if (type.IsAbstract) {
        return false;
      }

      return iModuleType.IsAssignableFrom(type) && iModuleType != type;
    }

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is <see cref="Domain"/> upgrade handler.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsUpgradeHandler(Type type)
    {
      if (type.IsAbstract) {
        return false;
      }

      return iUpgradeHandlerType.IsAssignableFrom(type) && iUpgradeHandlerType != type;
    }

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is key generator.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsKeyGenerator(Type type)
    {
      if (type.IsAbstract) {
        return false;
      }

      return keyGeneratorType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is compiler container.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsCompilerContainer(Type type) =>
      type.IsDefined(typeof(CompilerContainerAttribute), false);

    /// <summary>
    /// Determines whether a <paramref name="type"/>
    /// is the catalog resolver of full-text indexes.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsFullTextCatalogNameBuilder(Type type)
    {
      if (type.IsAbstract) {
        return false;
      }

      return ifulltextCatalogNameBuilder.IsAssignableFrom(type) && ifulltextCatalogNameBuilder != type;
    }

    /// <summary>
    /// Determines whether the <paramref name="type"/> is
    /// a connection accessor.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsConnectionAccessor(Type type)
    {
      if (type.IsAbstract) {
        return false;
      }

      return iDbConnectionAccessorType.IsAssignableFrom(type) && iDbConnectionAccessorType != type;
    }

    /// <summary>
    /// Determines whether the <paramref name="type"/> is
    /// a database connection accessor.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsDbConnectionAccessor(Type type)
    {
      if (type.IsAbstract) {
        return false;
      }

      return iDbConnectionAccessorType.IsAssignableFrom(type) && iDbConnectionAccessorType != type;
    }

    #endregion

    #region ICloneable members

    /// <inheritdoc/>
    public override object Clone() => new DomainTypeRegistry(this);

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
