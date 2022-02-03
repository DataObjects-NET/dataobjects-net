// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.03.01

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Logging;

namespace Xtensive.Orm
{
  /// <summary>
  /// Describes a set of key-version pairs used to validate versions.
  /// </summary>
  [Serializable]
  public sealed class VersionSet : IEnumerable<KeyValuePair<Key, VersionInfo>>
  {
    private Dictionary<Ref<Entity>, VersionInfo> versions = 
      new Dictionary<Ref<Entity>, VersionInfo>();

    /// <inheritdoc/>
    public long Count {
      get { return versions.Count; }
    }

    /// <summary>
    /// Gets the <see cref="VersionInfo"/> by the specified key.
    /// If there is no such <see cref="VersionInfo"/>, it returns <see cref="VersionInfo.Void"/>.
    /// </summary>
    public VersionInfo this[Key key] {
      get {
        return Get(key);
      }
    }

    /// <summary>
    /// Gets the <see cref="VersionInfo"/> for the specified 
    /// <paramref name="entity"/>.
    /// If there is no such <see cref="VersionInfo"/>, it returns <see cref="VersionInfo.Void"/>.
    /// </summary>
    /// <param name="entity">The entity to get associated <see cref="VersionInfo"/> for.</param>
    /// <returns>Associated <see cref="VersionInfo"/>, if found;
    /// otherwise, <see cref="VersionInfo.Void"/>.</returns>
    public VersionInfo Get(Entity entity)
    {
      return Get(entity!=null ? entity.Key : null);
    }

    /// <summary>
    /// Gets the <see cref="VersionInfo"/> by the specified key.
    /// If there is no such <see cref="VersionInfo"/>, it returns <see cref="VersionInfo.Void"/>.
    /// </summary>
    /// <param name="key">The key to get associated <see cref="VersionInfo"/> for.</param>
    /// <returns>Associated <see cref="VersionInfo"/>, if found;
    /// otherwise, <see cref="VersionInfo.Void"/>.</returns>
    public VersionInfo Get(Key key)
    {
      VersionInfo result;
      if (versions.TryGetValue(key, out result))
        return result;
      else
        return VersionInfo.Void;
    }

    /// <summary>
    /// Determines whether this set contains the key of the specified entity.
    /// </summary>
    /// <param name="entity">The entity to check the key for containment.</param>
    /// <returns>Check result.</returns>
    public bool Contains(Entity entity)
    {
      if (entity==null)
        return false;
      return Contains(entity.Key);
    }

    /// <summary>
    /// Determines whether this set contains the specified key.
    /// </summary>
    /// <param name="key">The key to check for containment.</param>
    /// <returns>Check result.</returns>
    public bool Contains(Key key)
    {
      if (key==null)
        return false;
      return versions.ContainsKey(key);
    }

    #region Validate methods

    /// <summary>
    /// Validates version of the specified <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity">The entity to validate version for.</param>
    /// <returns>
    /// <see langword="True"/>, if validation passes successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Validate(Entity entity)
    {
      ArgumentValidator.EnsureArgumentNotNull(entity, "entity");
      return Validate(entity.Key, entity.VersionInfo);
    }

    /// <summary>
    /// Validates version of the specified <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity">The entity to validate version for.</param>
    /// <param name="throwOnFailure">Indicates if validation should immediately fail on failure.</param>
    /// <returns>
    /// <see langword="True"/>, if validation passes successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Validate(Entity entity, bool throwOnFailure)
    {
      ArgumentValidator.EnsureArgumentNotNull(entity, "entity");
      return Validate(entity.Key, entity.VersionInfo, throwOnFailure);
    }

    /// <summary>
    /// Validates the <paramref name="version"/>
    /// for the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to validate version for.</param>
    /// <param name="version">The version to validate.</param>
    /// <returns>
    /// <see langword="True"/>, if validation passes successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Validate(Key key, VersionInfo version)
    {
      var expectedVersion = Get(key);
      if (expectedVersion.IsVoid)
        return true;
      else
        return expectedVersion==version;
    }

    /// <summary>
    /// Validates the <paramref name="version"/>
    /// for the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to validate version for.</param>
    /// <param name="version">The version to validate.</param>
    /// <param name="throwOnFailure">Indicates whether <see cref="InvalidOperationException"/>
    /// must be thrown on validation failure.</param>
    /// <returns>
    /// <see langword="True"/>, if validation passes successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="VersionConflictException">Version conflict is detected.</exception>
    public bool Validate(Key key, VersionInfo version, bool throwOnFailure)
    {
      var result = Validate(key, version);
      if (throwOnFailure && !result) {
        if (OrmLog.IsLogged(LogLevel.Info))
          OrmLog.Info(Strings.LogSessionXVersionValidationFailedKeyYVersionZExpected3,
            "None (VersionSet)", key, version, Get(key));
        throw new VersionConflictException(string.Format(
          Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, key));
      }
      return result;
    }

    #endregion

    #region Add, Remove, Clear methods

    /// <summary>
    /// Adds key and <see cref="VersionInfo"/> pair
    /// of the specified <paramref name="entity"/> to this set.
    /// </summary>
    /// <param name="entity">The entity to add version of.</param>
    /// <param name="overwrite">Indicates whether to overwrite an existing
    /// key-version pair or not, if it exists.</param>
    /// <returns><see langword="True" />, if operation was successful;
    /// otherwise, <see langword="false" />.</returns>
    public bool Add(Entity entity, bool overwrite)
    {
      ArgumentValidator.EnsureArgumentNotNull(entity, "entity");
      var key = entity.Key;
      var version = entity.VersionInfo;
      return Add(key, version, overwrite);
    }

    /// <summary>
    /// Adds the specified key and <see cref="VersionInfo"/> pair to this set.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="version">The version related to this key.</param>
    /// <param name="overwrite">Indicates whether to overwrite an existing
    /// key-version pair or not, if it exists.</param>
    /// <returns><see langword="True" />, if operation was successful;
    /// otherwise, <see langword="false" />.</returns>
    public bool Add(Key key, VersionInfo version, bool overwrite)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      if (!Contains(key)) {
        if (version.IsVoid)
          return false;
        else {
          versions.Add(key, version);
          return true;
        }
      }
      else if (overwrite) {
        if (version.IsVoid)
          versions.Remove(key);
        else
          versions[key] = version;
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Removed the key and <see cref="VersionInfo"/> pair 
    /// of the specified <paramref name="entity"/> from this set.
    /// </summary>
    /// <param name="entity">The entity to remove the key-version pair of.</param>
    /// <returns><see langword="True" />, if operation was successful;
    /// otherwise, <see langword="false" />.</returns>
    public bool Remove(Entity entity)
    {
      ArgumentValidator.EnsureArgumentNotNull(entity, "entity");
      return Remove(entity.Key);
    }

    /// <summary>
    /// Removed the specified key and <see cref="VersionInfo"/> pair from this set.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><see langword="True" />, if operation was successful;
    /// otherwise, <see langword="false" />.</returns>
    public bool Remove(Key key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      return versions.Remove(key);
    }

    /// <summary>
    /// Clears this set.
    /// </summary>
    public void Clear()
    {
      versions.Clear();
    }

    #endregion

    /// <summary>
    /// Merges the current <see cref="VersionSet"/> with provided one.
    /// </summary>
    /// <param name="other">The other <see cref="VersionSet"/>.</param>
    /// <param name="session"></param>
    public void MergeWith(VersionSet other, Session session)
    {
      foreach (var pair in other.versions.Where(p => session.EntityStateCache.ContainsKey(p.Key.Key)))
        versions[pair.Key] = pair.Value;
    }

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<Key, VersionInfo>> GetEnumerator()
    {
      foreach (var pair in versions)
        yield return new KeyValuePair<Key, VersionInfo>(pair.Key, pair.Value);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public VersionSet()
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">Initial content.</param>
    public VersionSet(params Entity[] source)
    {
      foreach (var entity in source)
        Add(entity, true);
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">Initial content.</param>
    public VersionSet(IEnumerable<Entity> source)
    {
      foreach (var entity in source)
        Add(entity, true);
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">Initial content.</param>
    public VersionSet(IEnumerable<KeyValuePair<Key, VersionInfo>> source)
    {
      foreach (var pair in source)
        Add(pair.Key, pair.Value, true);
    }
  }
}