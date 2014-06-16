// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.30

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Reflection;
using System.Linq;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Default <see cref="IUpgradeHandler"/> implementation.
  /// </summary>
  public class UpgradeHandler : IUpgradeHandler
  {
    /// <summary>
    /// The ".Recycled" suffix.
    /// </summary>
    public static readonly string RecycledSuffix = ".Recycled";

    private Assembly assembly;
    private string assemblyName;
    private string assemblyVersion;

    /// <inheritdoc/>
    public virtual bool IsEnabled {
      get {
        // Must be disabled for Xtensive.Storage,
        // since there is SystemUpgradeHandler.
        return Assembly!=typeof(UpgradeHandler).Assembly;
      }
    }

    /// <inheritdoc/>
    public virtual Assembly Assembly {
      get {
        if (assembly!=null)
          return assembly;
        assembly = DetectAssembly();
        return assembly;
      }
    }

    /// <inheritdoc/>
    public virtual string AssemblyName {
      get {
        if (assemblyName!=null)
          return assemblyName;
        assemblyName = DetectAssemblyName();
        return assemblyName;
      }
    }

    /// <inheritdoc/>
    public virtual string AssemblyVersion {
      get {
        if (assemblyVersion!=null)
          return assemblyVersion;
        assemblyVersion = DetectAssemblyVersion();
        return assemblyVersion;
      }
    }

    /// <inheritdoc/>
    public UpgradeContext UpgradeContext { get; private set; }

    /// <summary>
    /// Determines whether handler is enabled autodetect of types, which moved from one namespace to another.
    /// <para>
    /// Detection is enabled by default.
    /// </para>
    /// </summary>
    public bool TypesMovementsAutoDetection
    {
      get { return UpgradeContext.TypesMovementsAutoDetection; }
      protected set { UpgradeContext.TypesMovementsAutoDetection = value; }
    }

    /// <inheritdoc/>
    public void OnConfigureUpgradeDomain()
    {
      var definitions = new List<RecycledDefinition>();
      AddRecycledDefinitions(definitions);
      ProcessRecycledDefinitions(definitions);
    }

    /// <inheritdoc/>
    public virtual void OnPrepare()
    {
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><c>context.Stage</c> is out of range.</exception>
    public virtual void OnBeforeStage()
    {
      var context = UpgradeContext;
      switch (context.Stage) {
        case UpgradeStage.Upgrading:
          AddUpgradeHints(context.Hints);
          AddAutoHints(context.Hints);
          break;
        case UpgradeStage.Final:
          break;
        default:
          throw new ArgumentOutOfRangeException("context.Stage");
      }
    }

    /// <inheritdoc/>
    public virtual void OnSchemaReady()
    {
    }

    /// <inheritdoc/>
    public virtual void OnStage()
    {
      var context = UpgradeContext;
      switch (context.Stage) {
        case UpgradeStage.Upgrading:
          OnUpgrade();
          break;
        case UpgradeStage.Final:
          break;
        default:
          throw new ArgumentOutOfRangeException("context.Stage");
      }
    }

    /// <inheritdoc/>
    public virtual void OnComplete(Domain domain)
    {
    }

    /// <inheritdoc/>
    public virtual void OnBeforeExecuteActions(UpgradeActionSequence actions)
    {
    }

    /// <inheritdoc/>
    public virtual bool CanUpgradeFrom(string oldVersion)
    {
      return oldVersion==null || oldVersion==AssemblyVersion;
    }

    /// <exception cref="ArgumentOutOfRangeException"><c>UpgradeContext.Stage</c> is out of range.</exception>
    public virtual bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      if (type.Assembly!=Assembly)
        throw new ArgumentOutOfRangeException("type");
      switch (upgradeStage) {
      case UpgradeStage.Upgrading:
        return true;
      case UpgradeStage.Final:
        return type.GetAttribute<RecycledAttribute>()==null;
      default:
        throw new ArgumentOutOfRangeException("UpgradeContext.Stage");
      }
    }

    /// <exception cref="ArgumentOutOfRangeException"><c>UpgradeContext.Stage</c> is out of range.</exception>
    public virtual bool IsFieldAvailable(PropertyInfo field, UpgradeStage upgradeStage)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      var type = field.DeclaringType;
      if (type.Assembly!=Assembly)
        throw new ArgumentOutOfRangeException("field");
      if (upgradeStage==UpgradeStage.Final)
        return field.GetAttribute<RecycledAttribute>()==null;
      return true;
    }

    /// <summary>
    /// Override this method to implement custom persistent data migration logic.
    /// </summary>
    public virtual void OnUpgrade()
    {
      // OnUpgrade is public instead of protected, WTF?
    }

    #region Protected methods

    /// <summary>
    /// Override this method to add upgrade hints to
    /// <see cref="Upgrade.UpgradeContext.Hints"/> collection.
    /// </summary>
    /// <param name="hints">A set of hints to add new hints to
    /// (a shortcut to <see cref="Upgrade.UpgradeContext.Hints"/> collection).</param>
    protected virtual void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
    {
    }

    /// <summary>
    /// Detects the assembly this handler is made for.
    /// </summary>
    /// <returns>The assembly.</returns>
    private Assembly DetectAssembly()
    {
      return GetType().Assembly;
    }

    /// <summary>
    /// Detects the name of the assembly this handler is made for.
    /// </summary>
    /// <returns>The name of the assembly.</returns>
    protected virtual string DetectAssemblyName()
    {
      var t = GetType();
      var a = Assembly;
      var ai = 
        t.GetAttributes<AssemblyInfoAttribute>().SingleOrDefault() 
        ?? Attribute.GetCustomAttributes(a, typeof(AssemblyInfoAttribute), false).SingleOrDefault() as AssemblyInfoAttribute;
      return (ai==null || ai.Name.IsNullOrEmpty()) 
        ? a.GetName().Name 
        : ai.Name;
    }

    /// <summary>
    /// Detects the version of the assembly this handler is made for.
    /// </summary>
    /// <returns>The version of the assembly.</returns>
    protected virtual string DetectAssemblyVersion()
    {
      var t = GetType();
      var a = Assembly;
      var ai = 
        t.GetAttributes<AssemblyInfoAttribute>().SingleOrDefault() 
        ?? Attribute.GetCustomAttributes(a, typeof(AssemblyInfoAttribute), false).SingleOrDefault() as AssemblyInfoAttribute;
      return (ai==null || ai.Version.IsNullOrEmpty()) 
        ? a.GetName().Version.ToString() 
        : ai.Version;
    }

    /// <summary>
    /// Adds the "auto" hints - e.g. hints for recycled types.
    /// </summary>
    /// <param name="hints">A set of hints to add new hints to
    /// (a shortcut to <see cref="Upgrade.UpgradeContext.Hints"/> collection).</param>
    protected virtual void AddAutoHints(Collections.ISet<UpgradeHint> hints)
    {
      var context = UpgradeContext;
      var types = Assembly.GetTypes();
      var registeredTypes = (
        from t in types
        where context.Configuration.Types.Contains(t)
        select t).ToArray();

      var recycledTypes =
        from t in registeredTypes
        let a = t.GetAttribute<RecycledAttribute>()
        where a!=null
        select new {Type = t, Attribute = a};

      foreach (var r in recycledTypes) {
        var oldName = r.Attribute.OriginalName;
        if (oldName.IsNullOrEmpty())
          oldName = GetOriginalName(r.Type);
        else if (!oldName.Contains(".")) {
          string ns = TryStripRecycledSuffix(r.Type.Namespace);
          oldName = ns + "." + oldName;
        }
        if (string.IsNullOrEmpty(r.Attribute.OriginalName) && !context.ExtractedTypeMap.ContainsKey(oldName))
          continue;
        var renameHint = new RenameTypeHint(oldName, r.Type);
        var similarHints =
          from h in hints
          let similarHint = h as RenameTypeHint
          where similarHint!=null
          where similarHint.NewType==renameHint.NewType
          select similarHint;
        if (!similarHints.Any())
          hints.Add(renameHint);
      }

      var recycledProperties =
        from t in registeredTypes
        from p in t.GetProperties(BindingFlags.DeclaredOnly 
          | BindingFlags.Instance
          | BindingFlags.Public
          | BindingFlags.NonPublic)
        let pa = p.GetAttribute<FieldAttribute>()
        let ra = p.GetAttribute<RecycledAttribute>()
        where pa!=null && ra!=null
        select new {Property = p, Attribute = ra};

      foreach (var r in recycledProperties) {
        var oldName = r.Attribute.OriginalName;
        if (!string.IsNullOrEmpty(oldName))
          hints.Add(new RenameFieldHint(r.Property.DeclaringType, oldName, r.Property.Name));
      }
    }

    private void ProcessRecycledDefinitions(ICollection<RecycledDefinition> definitions)
    {
      if (definitions.Count==0)
        return;
      var hints = UpgradeContext.Hints;
      var allDefinitions = UpgradeContext.RecycledDefinitions;
      var recycledFieldDefinitions = definitions.OfType<RecycledFieldDefinition>();
      foreach (var definition in recycledFieldDefinitions) {
        allDefinitions.Add(definition);
        if (!string.IsNullOrEmpty(definition.OriginalFieldName) && definition.FieldName!=definition.OriginalFieldName)
          hints.Add(new RenameFieldHint(definition.OwnerType, definition.OriginalFieldName, definition.FieldName));
      }
    }

    /// <summary>
    /// Override this method to add recycled definitions.
    /// </summary>
    /// <param name="recycledDefinitions">Collection to put recycled definitions to.</param>
    protected virtual void AddRecycledDefinitions(ICollection<RecycledDefinition> recycledDefinitions)
    {
    }

    /// <summary>
    /// Gets the original name of the recycled type.
    /// </summary>
    /// <param name="recycledType">The recycled type to get the original name for.</param>
    /// <returns>The original name of the recycled type.</returns>
    protected virtual string GetOriginalName(Type recycledType)
    {
      var nameOfType = recycledType.IsNested ? recycledType.FullName.Replace(recycledType.Namespace + ".", string.Empty) : recycledType.Name;
      return TryStripRecycledSuffix(recycledType.Namespace) + "." + nameOfType;
    }

    /// <summary>
    /// Tries to strip the ".Recycled" suffix from the namespace.
    /// </summary>
    /// <param name="nameSpace">The namespace to remove the suffix from.</param>
    /// <returns>
    /// The namespace without ".Recycled" suffix, if it was there;
    /// otherwise the same value.
    /// </returns>
    protected virtual string TryStripRecycledSuffix(string nameSpace)
    {
      return nameSpace.TryCutSuffix(RecycledSuffix);
    }

    #endregion

    
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public UpgradeHandler()
    {
      UpgradeContext = UpgradeContext.Demand();
    }

    internal UpgradeHandler(Assembly assembly)
      : this()
    {
      this.assembly = assembly;
    }
  }
}