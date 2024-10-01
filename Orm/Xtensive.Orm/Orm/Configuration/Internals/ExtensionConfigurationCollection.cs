using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Default <see cref="IExtensionConfigurationCollection"/> implementation (<see cref="ILockable">lockable</see>).
  /// </summary>
  [Serializable]
  public sealed class ExtensionConfigurationCollection : LockableBase
    IEnumerable<ConfigurationBase>
  {
    private Dictionary<Type, ConfigurationBase> extensionConfigurations;

    /// <summary>
    /// Number of configurations in collection.
    /// </summary>
    public long Count
    {
      [DebuggerStepThrough]
      get => extensionConfigurations != null ? extensionConfigurations.Count : 0;
    }

    /// <summary>
    /// Gets configuration of certain type from collection.
    /// </summary>
    /// <typeparam name="T">Type of configuration to get.</typeparam>
    /// <returns>Found configuration or <see langword="null"/>.</returns>
    [DebuggerStepThrough]
    public T Get<T>()
      where T : ConfigurationBase
    {
      return extensionConfigurations != null && extensionConfigurations.TryGetValue(typeof(T), out var result)
        ? (T) result
        : null;
    }

    /// <summary>
    /// Adds or replace configuration of certain type
    /// </summary>
    /// <typeparam name="T">Type of configuration.</typeparam>
    /// <param name="value">Configuration to add or to replace existing one.</param>
    [DebuggerStepThrough]
    public void Set<T>(T value)
      where T : ConfigurationBase
    {
      EnsureNotLocked();
      ArgumentValidator.EnsureArgumentNotNull(value, nameof(value));

      var extensionConfigurationType = typeof(T);

      if (extensionConfigurations == null) {
        extensionConfigurations = new Dictionary<Type, ConfigurationBase>();
      }

      extensionConfigurations[extensionConfigurationType] = value;
    }

    /// <summary>
    /// Clears the collection.
    /// </summary>
    public void Clear()
    {
      EnsureNotLocked();
      extensionConfigurations = null;
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (extensionConfigurations != null)
        foreach (var pair in extensionConfigurations) {
          pair.Value.Lock(recursive);
        }
    }

    #region ICloneable methods

    /// <inheritdoc/>
    public object Clone()
    {
      return new ExtensionConfigurationCollection(this);
    }

    #endregion

    #region IEnumerable methods

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public IEnumerator<ConfigurationBase> GetEnumerator()
    {
      return extensionConfigurations != null
        ? extensionConfigurations.Values.GetEnumerator()
        : Enumerable.Empty<ConfigurationBase>().GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public ExtensionConfigurationCollection()
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="source">The source to copy into this collection.</param>
    public ExtensionConfigurationCollection(ExtensionConfigurationCollection source)
      : this()
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      if (source.Count == 0)
        return;
      extensionConfigurations = new Dictionary<Type, ConfigurationBase>(source.extensionConfigurations);
    }
  }
}
