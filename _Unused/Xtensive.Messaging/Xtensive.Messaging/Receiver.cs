// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.07

using System;
using System.Collections.Generic;
using System.IO;
using Xtensive.Core;
using Xtensive.Core.Serialization;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Core.Threading;
using Xtensive.Messaging.Diagnostics;
using Xtensive.Messaging.Providers;
using Xtensive.Messaging.Resources;

namespace Xtensive.Messaging
{
  /// <summary>
  /// Receives messages from remote hosts.
  /// </summary>
  public sealed class Receiver : IHasSyncRoot, IDisposable
  {
    // Private fields

    private readonly EndPointInfo receiverInfo;
    private readonly ISerializer serializer;
    private IListeningConnection listeningConnection;
    private bool started;

    private readonly Dictionary<IReceivingConnection, MessageReader> receiverConnections =
      new Dictionary<IReceivingConnection, MessageReader>();

    private object processorContext;
    private ProcessorFactory processorFactory;
    private bool disposed;
    private readonly IReceivingConnection externalConnection;

    // Events

    /// <summary>
    /// Raised when message is received by this provider.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> MessageReceived;

    internal event EventHandler<CollectionItemReceivedEventArgs> CollectionItemReceived;

    // Properties

    /// <summary>
    /// Gets or sets the <see cref="ISerializer"/> serializing the messages.
    /// </summary>
    public ISerializer Serializer
    {
      get { return serializer; }
    }

    /// <summary>
    /// Gets <see langword="true"/> if receiver is listening for incoming messages.
    /// </summary>
    public bool IsStarted
    {
      get { return listeningConnection!=null; }
    }

    /// <summary>
    /// Gets or sets context object for message processors.
    /// </summary>
    public object ProcessorContext
    {
      get { return processorContext; }
      set
      {
        lock (this) {
          if (IsStarted)
            throw new MessagingException(Strings.ExUnableToModifyContextWhileReceiverStarted);
          processorContext = value;
        }
      }
    }

    /// <summary>
    /// Gets count of active connections.
    /// </summary>
    public int ConnectionCount
    {
      get
      {
        lock (this)
          return receiverConnections.Count;
      }
    }

    /// <summary>
    /// Gets the URL this receiver listens at.
    /// </summary>
    public string ReceiverUrl
    {
      get { return receiverInfo==null ? null : receiverInfo.Url; }
    }

    /// <summary>
    /// Gets the <see cref="EndPointInfo"/> describing <see cref="ReceiverUrl"/>.
    /// </summary>
    public EndPointInfo ReceiverInfo
    {
      get { return receiverInfo; }
    }

    /// <summary>
    /// Gets or sets the synchronization root of the object.
    /// </summary>
    public object SyncRoot
    {
      get { return this; }
    }


    // Public methods

    /// <summary>
    /// Searches for message processors in application domain and adds them to <see cref="Receiver"/>. 
    /// </summary>
    /// <param name="processorType">Type of <see cref="IMessageProcessor"/> class to add to <see cref="Receiver"/>.</param>
    public void AddProcessor(Type processorType)
    {
      AddProcessor(processorType, false);
    }

    /// <summary>
    /// Searches for message processors in application domain and adds them to <see cref="Receiver"/>. 
    /// </summary>
    /// <param name="processorType">Type of <see cref="IMessageProcessor"/> class to add to <see cref="Receiver"/>.</param>
    /// <param name="withDescendants">If <see langword="true"/> all descending <paramref name="processorType"/> classes will be added too.</param>
    public void AddProcessor(Type processorType, bool withDescendants)
    {
      ArgumentValidator.EnsureArgumentNotNull(processorType, "processorType");
      if (IsStarted)
        throw new MessagingException(Strings.ExUnableToAddProcessorWhileReceiverStarted);
      EnsureProcessorFactory();
      processorFactory.Add(processorType, withDescendants);
    }

    /// <summary>
    /// Searches for message processors in application domain and removes them from <see cref="Receiver"/>. 
    /// </summary>
    /// <param name="processorType">Type of <see cref="IMessageProcessor"/> class to remove from <see cref="Receiver"/>.</param>
    public void RemoveProcessor(Type processorType)
    {
      RemoveProcessor(processorType, false);
    }

    /// <summary>
    /// Searches for message processors in application domain and removes them from <see cref="Receiver"/>. 
    /// </summary>
    /// <param name="processorType">Type of <see cref="IMessageProcessor"/> class to remove from <see cref="Receiver"/>.</param>
    /// <param name="withDescendants">If <see langword="true"/> all descending <paramref name="processorType"/> classes will be removed too.</param>
    public void RemoveProcessor(Type processorType, bool withDescendants)
    {
      ArgumentValidator.EnsureArgumentNotNull(processorType, "processorType");
      if (IsStarted)
        throw new MessagingException(Strings.ExUnableToRemoveProcessorWhileReceiverStarted);
      EnsureProcessorFactory();
      processorFactory.Remove(processorType, withDescendants);
    }

    /// <summary>
    /// Starts receive data.
    /// </summary>
    public void StartReceive()
    {
      lock (this) {
        if (started)
          return;
        started = true;
        if (externalConnection!=null)
          ConnectionAccepted(externalConnection, new EventArgs());
        else {
          listeningConnection =
            MessagingProviderManager.GetMessagingProvider(receiverInfo).CreateListeningConnection(receiverInfo);
          listeningConnection.Accepted += ConnectionAccepted;
        }
      }
    }

    /// <summary>
    /// Stops <see cref="Receiver"/> to read data from connection. 
    /// </summary>
    public void StopReceive()
    {
      StopReceiveInternal();
    }

    // Private methods

    private void EnsureProcessorFactory()
    {
      lock (this) {
        if (processorFactory==null)
          processorFactory = new ProcessorFactory();
      }
    }

    /// <summary>
    /// Called than <see cref="IReceivingConnection"/> closed.
    /// </summary>
    private void ConnectionClosed(object sender, EventArgs e)
    {
      lock (this) {
        var receivingConnection = sender as IReceivingConnection;
        if (receivingConnection!=null) {
          if (receiverConnections.ContainsKey(receivingConnection)) {
            receivingConnection.RemoveConsumer(this);
            receivingConnection.DataReceived -= DataReceived;
            receiverConnections.Remove(receivingConnection);
          }
        }
      }
    }

    /// <summary>
    /// Called than new <see cref="IReceivingConnection"/> created by <see cref="IListeningConnection"/>.
    /// </summary>
    private void ConnectionAccepted(object sender, EventArgs e)
    {
      lock (this) {
        var receivingConnection = (IReceivingConnection)sender;
        if (receiverConnections.ContainsKey(receivingConnection))
          throw new MessagingException(Strings.ExReceivingConnectionAlreadyExists);
        receivingConnection.DataReceived += DataReceived;
        receivingConnection.Closed += ConnectionClosed;
        receiverConnections.Add(receivingConnection, new MessageReader());
        receivingConnection.AddConsumer(this);
      }
    }

    private void DataReceived(object sender, DataReceivedEventArgs eventArgs)
    {
      lock (this) {
        // Merge with previous part
        MessageReader messageReader;
        if (!receiverConnections.TryGetValue((IReceivingConnection)sender, out messageReader))
          Exceptions.InternalError(Strings.ExReceiverGotDataFromUnknownConnection, Log.Instance);

        int position = 0;
        while (position < eventArgs.Data.Length)
          if (messageReader.Read(eventArgs.Data, ref position)) {
            MemoryStream messageBody = messageReader.Body;
            messageBody.Position = 0;
            try {
              object message = serializer.Deserialize(messageBody);
              if (!DebugInfo.SkipReceiveNow) {
                DebugInfo.IncreaseMessageReceivedCount();
                RaiseMessageReceivedEvent(message, eventArgs.ReplyTo);
              }
            }
            catch (Exception e) {
              DebugInfo.IncreaseReceiveErrorCount();
              Log.Error(e, Strings.LogMessageDeserializeError);
            }
            finally {
              messageReader.Clear();
            }
          }
      }
    }

    private void StopReceiveInternal()
    {
      if (!started)
        return;
      started = false;
      if (listeningConnection!=null) {
        listeningConnection.Accepted -= ConnectionAccepted;
        listeningConnection.Dispose();
        listeningConnection = null;
      }
      foreach (KeyValuePair<IReceivingConnection, MessageReader> pair in receiverConnections) {
        pair.Key.DataReceived -= DataReceived;
        pair.Key.RemoveConsumer(this);
        if (externalConnection==null)
          pair.Key.Dispose();
      }
      receiverConnections.Clear();
    }

    /// <summary>
    /// Sends deserialized message to subscribers.
    /// </summary>
    private void RaiseMessageReceivedEvent(object result, ISendingConnection sendingConnection)
    {
      EventHandler<MessageReceivedEventArgs> messageReceivedEvent = MessageReceived;

      var dataMessage = result as IDataMessage;
      var responseMessage = result as IResponseMessage;
      var queryMessage = result as IQueryMessage;

      bool asyncEvent = false;
      object eventParameter = result;
      if (dataMessage!=null)
        eventParameter = dataMessage.Data;
      else {
        var messageCollectionHeadItem = result as IMessageCollectionHeadItem;
        if (messageCollectionHeadItem!=null) {
          eventParameter = new MessageCollection(messageCollectionHeadItem, this);
          asyncEvent = true;
        }
        else {
          var messageCollectionItem = result as IMessageCollectionItem;
          if (messageCollectionItem!=null) {
            EventHandler<CollectionItemReceivedEventArgs> collectionItemReceivedEvent = CollectionItemReceived;
            if (collectionItemReceivedEvent!=null) {
              collectionItemReceivedEvent(this, new CollectionItemReceivedEventArgs(messageCollectionItem));
              return;
            }
          }
        }
      }
      IMessageProcessor processor = null;
      if (processorFactory!=null && processorFactory.Contains(eventParameter)) {
        processorFactory.Context = processorContext;
        processor = processorFactory.Create(eventParameter);
      }

      if (messageReceivedEvent==null && processor==null)
        return;

      long? responseQueryId = null;
      if (responseMessage!=null)
        responseQueryId = responseMessage.ResponseQueryId;

      Sender replySender = (queryMessage!=null)
        ? new Sender(sendingConnection, queryMessage.QueryId, serializer, queryMessage.ReceiverUrl)
        : null;
      try {
        if (processor!=null) {
          try {
            processor.ProcessMessage(eventParameter, replySender);
          }
          catch (Exception ex) {
            // Send back exception.
            if (replySender!=null)
              replySender.Send(new ErrorResponseMessage(ex));
          }
        }
        if (messageReceivedEvent!=null) {
          var eventArgs = new MessageReceivedEventArgs(eventParameter, replySender, responseQueryId);
          if (asyncEvent) {
            messageReceivedEvent.BeginInvoke(this, eventArgs, null, null);
          }
          else {
            try {
              messageReceivedEvent(this, eventArgs);
            }
            catch (Exception ex) {
              if (replySender!=null)
                replySender.Send(new ErrorResponseMessage(ex));
            }
          }
        }
      }
      finally {
        if (replySender!=null)
          replySender.Dispose();
      }
    }


    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="Receiver"/> using <paramref name="receiverUrl"/>
    /// for listen for incoming connections.
    /// Uses <see cref="LegacyBinarySerializer.Instance"/> to deserialize messages.
    /// </summary>
    /// <param name="receiverUrl">Address and port where to listen on. Format must be compatible
    /// with <see cref="EndPointInfo"/>. To listen on all available IP addresses use 
    /// <see langword="$ANY"/> as host name.</param>
    public Receiver(string receiverUrl)
      : this(receiverUrl, LegacyBinarySerializer.Instance)
    {
    }

    /// <summary>
    /// Creates new instance of <see cref="Receiver"/> using <paramref name="receiverUrl"/> for listen for incoming connections.
    /// </summary>
    /// <param name="receiverUrl">Address and port where to listen on. Format must be compatible with <see cref="EndPointInfo"/>. 
    /// To listen on all available IP addresses use <see langword="$ANY"/> as host name.</param>
    /// <param name="serializer"><see cref="ISerializer"/> to deserialize messages. 
    /// By default <see cref="Receiver"/> uses <see cref="LegacyBinarySerializer.Instance"/>.</param>
    public Receiver(string receiverUrl, ISerializer serializer)
    {
      receiverInfo = new EndPointInfo(receiverUrl);
      this.serializer = serializer;
      DebugInfo.IncreaseReceiverCount();
    }

    /// <summary>
    /// Creates new instance of <see cref="Receiver"/> using <paramref name="receivingConnection"/> for read data from. No <see cref="IListeningConnection"/> creates. Usually used in <see cref="Sender"/> to get reply from remote host.
    /// </summary>
    /// <param name="receivingConnection"><see cref="IReceivingConnection"/> to read data from.</param>
    /// <param name="serializer"><see cref="ISerializer"/> to deserialize messages.</param>
    internal Receiver(IReceivingConnection receivingConnection, ISerializer serializer)
    {
      this.serializer = serializer;
      externalConnection = receivingConnection;
      DebugInfo.IncreaseReceiverCount();
    }

    // Dispose, Finalize


    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
      GC.SuppressFinalize(this);
      Dispose(true);
    }

    private void Dispose(bool disposing)
    {
      lock (SyncRoot) {
        if (disposed)
          return;
        disposed = true;
      }
      StopReceiveInternal();
      if (disposing) {
        if (processorFactory!=null) {
          processorFactory.Dispose();
          processorFactory = null;
        }
        DebugInfo.DecreaseReceiverCount();
      }
    }

    ///<summary>
    ///Allows an <see cref="T:System.Object"></see> to attempt to free resources and perform other cleanup operations before the <see cref="T:System.Object"></see> is reclaimed by garbage collection.
    ///</summary>
    ///
    ~Receiver()
    {
      Dispose(false);
    }
  }
}