// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.07.16

using Xtensive.Core;


using Xtensive.IoC;
using Xtensive.Reflection;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// The single enumeration attempt context for the <see cref="ExecutableProvider"/>.
  /// </summary>
  [Serializable]
  public abstract class EnumerationContext
  {

    private readonly Dictionary<Pair<object, string>, object> cache = new Dictionary<Pair<object, string>, object>();

    /// <summary>
    /// Gets the options of this context.
    /// </summary>
    protected abstract EnumerationContextOptions Options { get; }

    /// <summary>
    /// Should be called before enumeration of your <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> object.</returns>
    public virtual ICompletableScope BeginEnumeration()
    {
      return null;
    }

    /// <summary>
    /// Caches the value in the current <see cref="EnumerationContext"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="name">The cache name.</param>
    /// <param name="value">The value to cache.</param>
    public void SetValue<T>(object key, string name, T value)
    {
      cache[new Pair<object, string>(key, name)] = value;
    }

    /// <summary>
    /// Gets the cached value from the current <see cref="EnumerationContext"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="name">The cache name.</param>
    /// <returns>
    /// Cached value with the specified key;
    /// <see langword="null"/>, if no cached value is found, or it is already expired.
    /// </returns>
    public T GetValue<T>(object key, string name)
    {
      object result;
      cache.TryGetValue(new Pair<object, string>(key, name), out result);
      return result==null ? default(T) : (T) result;
    }

    /// <summary>
    /// Checks whenever the specified option set is enable for this context.
    /// </summary>
    /// <param name="requiredOptions">The options to check.</param>
    /// <returns><see langword="true"/> if the speicifed options set is enable in this context;
    /// otherwise, <see langword="false"/>.</returns>
    public bool CheckOptions(EnumerationContextOptions requiredOptions)
    {
      return (Options & requiredOptions)==requiredOptions;
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    protected EnumerationContext()
    {
    }
  }
}