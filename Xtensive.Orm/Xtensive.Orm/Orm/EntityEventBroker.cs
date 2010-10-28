// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.29

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Model;
using System.Linq;

namespace Xtensive.Orm
{
  /// <summary>
  /// Manages events related to <see cref="Entity"/>.
  /// </summary>
  public sealed class EntityEventBroker
  {
    private Dictionary<Triplet<Key, FieldInfo, object>, Delegate> subscribers;

    #region Event keys

    /// <summary>
    /// The key for 'Getting field' event.
    /// </summary>
    public static readonly object GettingFieldEventKey = new object();

    /// <summary>
    /// The key of 'Get field' event.
    /// </summary>
    public static readonly object GetFieldEventKey = new object();

    /// <summary>
    /// The key of 'Setting field attempt' event.
    /// </summary>
    public static readonly object SettingFieldAttemptEventKey = new object();

    /// <summary>
    /// The key of 'Setting field' event.
    /// </summary>
    public static readonly object SettingFieldEventKey = new object();

    /// <summary>
    /// The key of 'Set field' event.
    /// </summary>
    public static readonly object SetFieldEventKey = new object();

    /// <summary>
    /// The key of 'Property changed' event.
    /// </summary>
    public static readonly object PropertyChangedEventKey = new object();

    /// <summary>
    /// The key of 'Collection changed' event.
    /// </summary>
    public static readonly object CollectionChangedEventKey = new object();

    /// <summary>
    /// The key of 'Initializing persistent' event.
    /// </summary>
    public static readonly object InitializingPersistentEventKey = new object();

    /// <summary>
    /// The key of 'Initialize persistent' event.
    /// </summary>
    public static readonly object InitializePersistentEventKey = new object();
    
    /// <summary>
    /// The key of 'Error on initializing persistent' event.
    /// </summary>
    public static readonly object InitializationErrorPersistentEventKey = new object();
    
    /// <summary>
    /// The key of 'Removing entity' event.
    /// </summary>
    public static readonly object RemovingEntityEventKey = new object();

    /// <summary>
    /// The key of 'Remove entity' event.
    /// </summary>
    public static readonly object RemoveEntityEventKey = new object();

    /// <summary>
    /// The key of 'Initialize entity set' event.
    /// </summary>
    public static readonly object InitializeEntitySetEventKey = new object();

    /// <summary>
    /// The key of 'Adding entity set item' event.
    /// </summary>
    public static readonly object AddingEntitySetItemEventKey = new object();

    /// <summary>
    /// The key of 'Add entity set item' event.
    /// </summary>
    public static readonly object AddEntitySetItemEventKey = new object();

    /// <summary>
    /// The key of 'Removing entity set item' event.
    /// </summary>
    public static readonly object RemovingEntitySetItemEventKey = new object();

    /// <summary>
    /// The key of 'Remove entity set item' event.
    /// </summary>
    public static readonly object RemoveEntitySetItemEventKey = new object();

    /// <summary>
    /// The key of 'Clearing entity set' event.
    /// </summary>
    public static readonly object ClearingEntitySetEventKey = new object();

    /// <summary>
    /// The key of 'Clear entity set' event.
    /// </summary>
    public static readonly object ClearEntitySetEventKey = new object();

    #endregion

    /// <summary>
    /// Gets a value indicating whether at least a one subscriber has registered.
    /// </summary>
    public bool HasSubscribers {get { return subscribers!=null; }}

    /// <summary>
    /// Adds the subscriber.
    /// </summary>
    /// <param name="key">The key of <see cref="Entity"/> which will be watched for events.</param>
    /// <param name="fieldInfo">The <see cref="Entity"/>'s field containing a nested object 
    /// which will be watched for events.</param>
    /// <param name="eventKey">The event key.</param>
    /// <param name="subscriber">The delegate.</param>
    public void AddSubscriber(Key key, FieldInfo fieldInfo, object eventKey, Delegate subscriber)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      ArgumentValidator.EnsureArgumentNotNull(eventKey, "eventKey");
      ArgumentValidator.EnsureArgumentNotNull(subscriber, "subscriber");
      if (subscribers == null)
        subscribers = new Dictionary<Triplet<Key, FieldInfo, object>, Delegate>();
      var subscriberKey = new Triplet<Key, FieldInfo, object>(key, fieldInfo, eventKey);
      Delegate currentDelegate;
      if (subscribers.TryGetValue(subscriberKey, out currentDelegate))
        subscribers[subscriberKey] = Delegate.Combine(currentDelegate, subscriber);
      else
        subscribers.Add(subscriberKey, subscriber);
    }

    /// <summary>
    /// Adds the subscriber.
    /// </summary>
    /// <param name="key">The key of <see cref="Entity"/> which will be watched for events.</param>
    /// <param name="eventKey">The event key.</param>
    /// <param name="subscriber">The delegate.</param>
    public void AddSubscriber(Key key, object eventKey, Delegate subscriber)
    {
      AddSubscriber(key, null, eventKey, subscriber);
    }

    /// <summary>
    /// Removes the subscriber.
    /// </summary>
    /// <param name="key">The key of <see cref="Entity"/> which will be watched for events.</param>
    /// <param name="fieldInfo">The <see cref="Entity"/>'s field containing a nested object 
    /// which will be watched for events.</param>
    /// <param name="eventKey">The event key.</param>
    /// <param name="subscriber">The delegate.</param>
    public void RemoveSubscriber(Key key, FieldInfo fieldInfo, object eventKey, Delegate subscriber)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      ArgumentValidator.EnsureArgumentNotNull(eventKey, "eventKey");
      ArgumentValidator.EnsureArgumentNotNull(subscriber, "subscriber");
      if (subscribers == null)
        return;
      var subscriberKey = new Triplet<Key, FieldInfo, object>(key, fieldInfo, eventKey);
      Delegate currentDelegate;
      if (subscribers.TryGetValue(subscriberKey, out currentDelegate))
        subscribers[subscriberKey] = Delegate.Remove(currentDelegate, subscriber);
    }

    /// <summary>
    /// Removes the subscriber.
    /// </summary>
    /// <param name="key">The key of <see cref="Entity"/> which will be watched for events.</param>
    /// <param name="eventKey">The event key.</param>
    /// <param name="subscriber">The delegate.</param>
    public void RemoveSubscriber(Key key, object eventKey, Delegate subscriber)
    {
      RemoveSubscriber(key, null, eventKey, subscriber);
    }

    /// <summary>
    /// Gets the subscriber.
    /// </summary>
    /// <param name="key">The key of <see cref="Entity"/> which will be watched for events.</param>
    /// <param name="fieldInfo">The <see cref="Entity"/>'s field containing a nested object 
    /// which will be watched for events.</param>
    /// <param name="eventKey">The event key.</param>
    /// <returns>A delegate registered for the event or <see langword="null" />.</returns>
    public Delegate GetSubscriber(Key key, FieldInfo fieldInfo, object eventKey)
    {
      if (subscribers == null)
        return null;
      var subscriberKey = new Triplet<Key, FieldInfo, object>(key, fieldInfo, eventKey);
      Delegate subscriber;
      if (subscribers.TryGetValue(subscriberKey, out subscriber))
        return subscriber;
      return null;
    }

    /// <summary>
    /// Gets the subscriber.
    /// </summary>
    /// <param name="key">The key of <see cref="Entity"/> which will be watched for events.</param>
    /// <param name="eventKey">The event key.</param>
    /// <returns>A delegate registered for the event or <see langword="null" />.</returns>
    public Delegate GetSubscriber(Key key, object eventKey)
    {
      return GetSubscriber(key, null, eventKey);
    }

    /// <summary>
    /// Gets all the subscribers for the specified <paramref name="eventKey"/>.
    /// </summary>
    /// <param name="eventKey">The event key.</param>
    /// <returns>
    /// The sequence of subscribers for the specified <paramref name="eventKey"/>.
    /// </returns>
    public IEnumerable<Triplet<Key, FieldInfo, Delegate>> GetSubscribers(object eventKey)
    {
      if (subscribers==null)
        yield break;

      foreach (var keyValuePair in subscribers) {
        var triplet = keyValuePair.Key;
        if (triplet.Third==eventKey)
          yield return new Triplet<Key, FieldInfo, Delegate>(triplet.First, triplet.Second, keyValuePair.Value);
      }
    }

    /// <summary>
    /// Remaps the event keys in accordance with specified <paramref name="keyMapping"/>.
    /// </summary>
    /// <param name="keyMapping">The key mapping.</param>
    public void RemapKeys(KeyMapping keyMapping)
    {
      if (subscribers==null || subscribers.Count==0)
        return;
      var copy = new Dictionary<Triplet<Key, FieldInfo, object>, Delegate>(subscribers);
      subscribers.Clear();
      foreach (var kvp in copy) {
        var triplet = kvp.Key;
        var subscriber = kvp.Value;
        if (subscriber==null) // Strange, but there is report is can be null: http://goo.gl/W6xo 
          continue;
        var key = keyMapping.TryRemapKey(triplet.First);
        AddSubscriber(key, triplet.Second, triplet.Third, kvp.Value);
      }
    }
  }
}