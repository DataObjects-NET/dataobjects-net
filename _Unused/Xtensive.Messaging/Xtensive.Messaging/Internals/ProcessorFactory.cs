// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.17

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;
using Xtensive.Messaging.Resources;

namespace Xtensive.Messaging
{
  internal class ProcessorFactory : IFactory<object, IMessageProcessor>, IDisposable, IHasSyncRoot
  {
    // Private fields

    private readonly Dictionary<Type, IMessageProcessor> singletoneProcessors =
      new Dictionary<Type, IMessageProcessor>();

    private readonly Pool<Type, IMessageProcessor> pooledProcessors = new Pool<Type, IMessageProcessor>();

    private readonly Dictionary<Type, ProcessorActivationMode> instanceModes =
      new Dictionary<Type, ProcessorActivationMode>();

    private readonly Dictionary<Type, Type> messageProcessors = new Dictionary<Type, Type>();
    private Type defaultProcessorType;
    private bool disposed;
    private object context;

    // Properties

    public object Context
    {
      set { context = value; }
    }

    /// <summary>
    /// Gets or sets the synchronization root of the object.
    /// </summary>
    public object SyncRoot
    {
      get { return this; }
    }

    // Methods

    /// <summary>
    /// Creates the <see cref="IMessageProcessor"/> by specified message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>Newly created <see cref="IMessageProcessor"/> for <paramref name="message"/>.</returns>
    public IMessageProcessor Create(object message)
    {
      ArgumentValidator.EnsureArgumentNotNull(message, "message");
      lock (this) {
        Type processorType;
        if (messageProcessors.TryGetValue(message.GetType(), out processorType))
          return GetProcessor(processorType);
        if (defaultProcessorType!=null)
          return GetProcessor(defaultProcessorType);
        throw new MessagingException(
          string.Format(CultureInfo.CurrentCulture, Strings.ExProcessorNotFound, message.GetType()));
      }
    }

    /// <summary>
    /// Gets <see langword="true"/> if factory contains <see cref="IMessageProcessor"/> for specified <paramref name="message"/>, otherwise <see langword="false"/>.
    /// </summary>
    /// <param name="message">Message to check for.</param>
    /// <returns><see langword="True"/> if factory can create <see cref="IMessageProcessor"/> for specified message, otherwise <see langword="false"/>.</returns>
    public bool Contains(object message)
    {
      ArgumentValidator.EnsureArgumentNotNull(message, "message");
      lock (this) {
        return defaultProcessorType!=null || messageProcessors.ContainsKey(message.GetType());
      }
    }

    /// <summary>
    /// Adds <see cref="IMessageProcessor"/> classes to automatically process messages.
    /// </summary>
    /// <param name="processorType"><see cref="Type"/> of <see cref="IMessageProcessor"/> to add to <see cref="Receiver"/>.</param>
    public void Add(Type processorType)
    {
      if (processorType.GetInterface("Xtensive.Messaging.IMessageProcessor")==null)
        throw new ArgumentException(Strings.ExInvalidProcessorType);

      ConstructorInfo constructorInfo =
        processorType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any,
          new Type[0], null);
      if (constructorInfo==null)
        throw new MessagingException(
          string.Format(CultureInfo.CurrentCulture, Strings.ExProcessorDefaultConstructorMissing, processorType));
      lock (this) {
        object[] attributes = processorType.GetCustomAttributes(typeof (MessageProcessorAttribute), false);
        foreach (MessageProcessorAttribute attribute in attributes)
          if (attribute.IsDefaultProcessor) {
            if (defaultProcessorType==null) {
              defaultProcessorType = processorType;
              instanceModes.Add(processorType, attribute.ProcessorActivationMode);
            }
            else
              throw new MessagingException(
                string.Format(CultureInfo.CurrentCulture, Strings.ExDuplicateDefaultProcessor, defaultProcessorType,
                  processorType));
          }
          else if (messageProcessors.ContainsKey(attribute.MessageType))
            throw new MessagingException(
              string.Format(CultureInfo.CurrentCulture, Strings.ExDuplicateProcessor, attribute.MessageType,
                messageProcessors[attribute.MessageType], processorType));
          else {
            messageProcessors.Add(attribute.MessageType, processorType);
            instanceModes.Add(processorType, attribute.ProcessorActivationMode);
          }
      }
    }

    /// <summary>
    /// Automatically searches for all <see cref="IMessageProcessor"/> classes in application domain and adds them to this <see cref="Receiver"/>.
    /// </summary>
    /// <param name="processorType">Type of <see cref="IMessageProcessor"/> class to add to <see cref="ProcessorFactory"/>.</param>
    /// <param name="withDescendants">If <see langword="true"/> all descending <paramref name="processorType"/> classes will be added too.</param>
    public void Add(Type processorType, bool withDescendants)
    {
      if (!withDescendants)
        Add(processorType);
      ArgumentValidator.EnsureArgumentNotNull(processorType, "processorType");
      lock (this) {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
          Type[] types;
          try {
            types = assembly.GetTypes();
          }
          catch (ReflectionTypeLoadException) {
            continue;
          }
          foreach (Type type in types)
            if (type.IsSubclassOf(processorType))
              Add(type);
        }
      }
    }

    /// <summary>
    /// Removes specified message processor from <see cref="Receiver"/>/
    /// </summary>
    /// <param name="processorType"><see cref="Type"/> of <see cref="IMessageProcessor"/> to remove from <see cref="Receiver"/>.</param>
    /// <param name="withDescendants">If <see langword="true"/> all descending <paramref name="processorType"/> classes will be removed too.</param>
    public void Remove(Type processorType, bool withDescendants)
    {
      if (!withDescendants)
        Remove(processorType);
      ArgumentValidator.EnsureArgumentNotNull(processorType, "processorType");
      lock (this) {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
          Type[] types;
          try {
            types = assembly.GetTypes();
          }
          catch (ReflectionTypeLoadException) {
            continue;
          }
          foreach (Type type in types)
            if (type.IsSubclassOf(processorType))
              Remove(type);
        }
      }
    }

    /// <summary>
    /// Removes specified message processor from <see cref="Receiver"/>/
    /// </summary>
    /// <param name="processorType"><see cref="Type"/> of <see cref="IMessageProcessor"/> to remove from <see cref="Receiver"/>.</param>
    public void Remove(Type processorType)
    {
      ArgumentValidator.EnsureArgumentNotNull(processorType, "processorType");
      lock (this) {
        ProcessorActivationMode activationMode;
        if (!instanceModes.TryGetValue(processorType, out activationMode))
          // throw new MessagingException(Strings.ExProcessorWasNotAdded);
          return;
        if (defaultProcessorType==processorType)
          defaultProcessorType = null;
        var removeMessageTypes = new List<Type>();
        foreach (KeyValuePair<Type, Type> pair in messageProcessors)
          if (pair.Value==processorType)
            removeMessageTypes.Add(pair.Key);
        foreach (Type messageType in removeMessageTypes)
          messageProcessors.Remove(messageType);
        switch (activationMode) {
        case ProcessorActivationMode.PooledInstances:
          foreach (IMessageProcessor processor in pooledProcessors.RemoveKey(processorType)) {
            var pooledProcessorDisposable = processor as IDisposable;
            if (pooledProcessorDisposable!=null)
              pooledProcessorDisposable.Dispose();
          }
          break;
        case ProcessorActivationMode.Singleton:
          IMessageProcessor singletoneProcessor;
          if (singletoneProcessors.TryGetValue(processorType, out singletoneProcessor)) {
            var singletoneProcessorDisposable = singletoneProcessor as IDisposable;
            if (singletoneProcessorDisposable!=null)
              singletoneProcessorDisposable.Dispose();
            singletoneProcessors.Remove(processorType);
          }
          break;
        }
      }
    }

    // Private methods

    private IMessageProcessor GetProcessor(Type processorType)
    {
      switch (instanceModes[processorType]) {
      case ProcessorActivationMode.SingleCall:
        return CreateInstance(processorType, context);
      case ProcessorActivationMode.PooledInstances:
        return pooledProcessors.Consume(processorType, () => CreateInstance(processorType, context));
      case ProcessorActivationMode.Singleton:
        IMessageProcessor result;
        if (!singletoneProcessors.TryGetValue(processorType, out result)) {
          result = CreateInstance(processorType, context);
          singletoneProcessors.Add(processorType, result);
        }
        return result;
      default:
        throw new MessagingException(Strings.ExUnknownProcessorType);
      }
    }

    private static IMessageProcessor CreateInstance(Type processorType, object context)
    {
      IMessageProcessor result;
      try {
        result =
          (IMessageProcessor)
            processorType.InvokeMember("", BindingFlags.CreateInstance, null, null, null, CultureInfo.CurrentCulture);
        result.SetContext(context);
      }
      catch (Exception ex) {
        throw new MessagingException(
          string.Format(CultureInfo.CurrentCulture, Strings.ExProcessorCreationError, processorType), ex);
      }
      return result;
    }

    private static void PooledProcessorExpired(object sender, ItemRemovedEventArgs<IMessageProcessor> e)
    {
      if (e.Item is IDisposable)
        ((IDisposable)e.Item).Dispose();
    }

    // Constructors

    public ProcessorFactory()
    {
      pooledProcessors.ItemRemoved += PooledProcessorExpired;
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dtor"/>
    /// </summary>
    public void Dispose()
    {
      GC.SuppressFinalize(this);
      Dispose(true);
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dtor"/>
    /// </summary>
    ~ProcessorFactory()
    {
      Dispose(false);
    }

    protected void Dispose(bool disposing)
    {
      lock (this) {
        if (disposed)
          return;
        disposed = true;
        if (disposing) {
          pooledProcessors.Dispose();
        }
        foreach (KeyValuePair<Type, IMessageProcessor> pair in singletoneProcessors)
          if (pair.Value is IDisposable)
            ((IDisposable)pair.Value).Dispose();
      }
    }
  }
}