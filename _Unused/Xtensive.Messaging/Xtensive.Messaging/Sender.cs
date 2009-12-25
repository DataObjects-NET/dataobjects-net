// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.07

using System;
using System.Collections;
using System.IO;
using System.Threading;
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
  /// Sends messages to the <see cref="Receiver"/> listening
  /// on specified <see cref="ReceiverUrl"/>.
  /// </summary>
  public sealed class Sender : IDisposable, IHasSyncRoot
  {
    // Static members
    private static long queryId = 1;

    // Private fields
    private int retryCount = 3;
    private ISerializer serializer;
    private ISendingConnection sendingConnection;
    private readonly MemoryStream sendingStream = new MemoryStream(512);
    private long sentQueryId;
    private TimeSpan responseTimeout = TimeSpan.FromSeconds(3);
    private readonly EndPointInfo receiverUrl;
    private Receiver responseReceiver;
    private long? responseQueryId;
    private EventWaitHandle responseReceivedWaitHandle;
    private object receivedMessage;
    private readonly bool isExternalConnection;
    private bool disposed;

    // Properties

    /// <summary>
    /// Gets or sets the synchronization root of the object.
    /// </summary>
    public object SyncRoot
    {
      get { return this; }
    }

    /// <summary>
    /// Gets or sets timeout to wait response from remote host.
    /// </summary>
    public TimeSpan ResponseTimeout
    {
      get { return responseTimeout; }
      set { responseTimeout = value; }
    }


    /// <summary>
    /// Gets the <see cref="ReceiverUrl"/> describing <see cref="Sender.ReceiverUrl"/>.
    /// </summary>
    public EndPointInfo ReceiverUrl
    {
      get { return receiverUrl; }
    }

    /// <summary>
    /// Gets or sets the <see cref="ISerializer"/> serializing the messages.
    /// </summary>
    public ISerializer Serializer
    {
      get { return serializer; }
    }

    /// <summary>
    /// Gets or sets the <see cref="Receiver"/> to which <see cref="Sender.Ask(object)"/> responses
    /// are delivered.
    /// </summary>
    public Receiver ResponseReceiver
    {
      get { return responseReceiver; }
      set {
        if (responseReceiver != null)
          Exceptions.AlreadyInitialized("ResponseReceiver");
        responseReceiver = value;
        responseReceiver.MessageReceived += ResponseReceived;
      }
    }

    /// <summary>
    /// Gets or sets count of retries than error occurred while sending message.
    /// </summary>
    public int RetryCount
    {
      get { return retryCount; }
      set { retryCount = value; }
    }


    // Public methods

    /// <summary>
    /// Prepares message to send to multiple receivers, serializes it's data.
    /// </summary>
    /// <param name="message"><see cref="IMessage"/> to prepare.</param>
    /// <returns><see cref="ISerializedMessage"/> with serialized data of <paramref name="message"/> within.</returns>
    public ISerializedMessage Prepare(IMessage message)
    {
      return new SerializedMessage(message, serializer);
    }

    /// <summary>
    /// Sends the <paramref name="message"/> to remote host.
    /// </summary>
    /// <param name="message">Message to send.</param>
    public void Send(object message)
    {
      ArgumentValidator.EnsureArgumentNotNull(message, "message");
      var responseMessage = message as IResponseMessage;
      if (responseMessage!=null)
        Send(responseMessage);
      else {
        var messageCollection = message as IMessageCollection;
        if (messageCollection!=null)
          Send(messageCollection);
        else {
          if (responseQueryId!=null)
            message = new DataResponseMessage(message);
          if (!(message is IMessage))
            message = new DataQueryMessage(message);
          var queryMessage = message as IQueryMessage;
          if (queryMessage!=null && queryMessage.ReceiverUrl==null && responseReceiver!=null)
            queryMessage.ReceiverUrl = responseReceiver.ReceiverUrl;
          SendInternal(message);
        }
      }
    }

    /// <summary>
    /// Sends collection part-by-part using <see cref="IEnumerable"/> interface to retrieve parts to send.
    /// </summary>
    /// <param name="collection">Collection to send.</param>
    public void Send(IMessageCollection collection)
    {
      ArgumentValidator.EnsureArgumentNotNull(collection, "collection");
      int index = 0;
      foreach (object collectionItem in collection) {
        IMessageCollectionItem item = (index==0)
          ? new MessageCollectionHeadItem(collectionItem, collection.Count)
          : new MessageCollectionItem(collectionItem, index);
        SendInternal(item);
        index++;
      }
    }


    /// <summary>
    /// Sends the response <paramref name="message"/>.
    /// </summary>
    /// <param name="message">Message to send.</param>
    public void Send(IResponseMessage message)
    {
      SendInternal(message);
    }

    private void SendInternal(object message)
    {
      lock (this) {
        int retry = 0;
        while (retry <= RetryCount) {
          bool messageSent = false;
          sendingConnection.AddConsumer(this);
          try {
            SendToSendingConnection(message);
            messageSent = true;
          }
          catch (Exception ex) {
            if (++retry >= RetryCount)
              throw new MessagingException(Strings.ExSendError, ex);
          }
          finally {
            sendingConnection.RemoveConsumer(this);
          }
          if (messageSent)
            break;
          // Recreate connection
          if (ReceiverUrl!=null) {
            try {
              InitializeSendingConnection();
            }
            catch (Exception exRecreate) {
              throw new MessagingException(Strings.ExProviderRecreateError, exRecreate);
            }
          }
        }
      }
    }

    /// <summary>
    /// Sends the query <paramref name="message"/> and waits for response to it.
    /// </summary>
    /// <param name="message">Message to send.</param>
    /// <returns>Response to <paramref name="message"/>.</returns>
    public object Ask(object message)
    {
      ArgumentValidator.EnsureArgumentNotNull(message, "message");
      var queryMessage = message as IQueryMessage;
      if (queryMessage!=null)
        return Ask(queryMessage);
      var serializedMessage = message as ISerializedMessage;
      if (serializedMessage!=null)
        return Ask(serializedMessage);
      queryMessage = new DataQueryMessage(message);
      lock (this) {
        TimeSpan? calculatedTimeout = queryMessage.CalculateTimeout();
        queryMessage.Timeout = calculatedTimeout.HasValue ? calculatedTimeout.Value : responseTimeout;
        queryMessage.ReceiverUrl = responseReceiver==null ? null : responseReceiver.ReceiverUrl;
        return Ask(queryMessage);
      }
    }

    /// <summary>
    /// Sends the query <paramref name="message"/> and waits for response to it.
    /// </summary>
    /// <param name="message">Message to send.</param>
    /// <returns>Response to <paramref name="message"/>.</returns>
    public object Ask(IMessage message)
    {
      ArgumentValidator.EnsureArgumentNotNull(message, "message");
      return AskInternal(message);
    }


    // Private Methods

    private object AskInternal(IMessage message)
    {
      ArgumentValidator.EnsureArgumentNotNull(message, "message");
      lock (this) {
        receivedMessage = null;
        int currentTry = 0;
        Receiver internalReceiver = null;
        while (currentTry <= RetryCount) {
          bool messageReceived = false;
          internalReceiver = null;
          sendingConnection.AddConsumer(this);
          try {
            // Initialize internal receiver if needed
            if (sendingConnection is IBidirectionalConnection) {
              try {
                internalReceiver = new Receiver((IReceivingConnection)sendingConnection, serializer);
                internalReceiver.MessageReceived += ResponseReceived;
                internalReceiver.StartReceive();
              }
              catch (Exception ex) {
                throw new MessagingException(Strings.ExUnableToCreateInternalReceiver, ex);
              }
            }
            if (internalReceiver==null && responseReceiver==null)
              throw new MessagingException(Strings.ExResponseReceiverIsNotInitialized);

            if (responseReceivedWaitHandle==null)
              responseReceivedWaitHandle = new AutoResetEvent(false);

            // Ask remote host.

            SendToSendingConnection(message);
            responseReceivedWaitHandle.WaitOne(responseTimeout, true);
            messageReceived = receivedMessage!=null;
          }
          catch (Exception) {
            if (currentTry >= (RetryCount - 1))
              throw;
          }
          finally {
            sendingConnection.RemoveConsumer(this);
            if (internalReceiver!=null && !(receivedMessage is IMessageCollectionHeadItem)) {
              internalReceiver.MessageReceived -= ResponseReceived;
              internalReceiver.Dispose();
            }
          }
          if (messageReceived)
            break;
          // Recreate connection
          if (++currentTry >= RetryCount)
            break;
          if (ReceiverUrl!=null) {
            try {
              InitializeSendingConnection();
            }
            catch (Exception exRecreate) {
              throw new MessagingException(Strings.ExProviderRecreateError, exRecreate);
            }
          }
        }
        if (receivedMessage!=null) {
          if (receivedMessage is IMessageCollectionHeadItem) {
            var collection = new MessageCollection((IMessageCollectionHeadItem)receivedMessage, internalReceiver);
            collection.DisposeReceiver = true;
            return new DataResponseMessage(collection);
          }
          if (receivedMessage is IHasError && ((IHasError)receivedMessage).Error!=null)
            throw ((IHasError)receivedMessage).Error;
          object result = receivedMessage;
          receivedMessage = null;
          return result;
        }
        if (internalReceiver!=null)
          internalReceiver.Dispose();
        throw new TimeoutException(Strings.ExAskTimeout);
      }
    }

    /// <summary>
    /// Serializes and sends object to <see cref="ISendingConnection"/>.
    /// </summary>
    /// <param name="objectToSend"></param>
    private void SendToSendingConnection(object objectToSend)
    {
      var serializedMessage = objectToSend as ISerializedMessage;
      var responseMessage = objectToSend as IResponseMessage;
      if (serializedMessage!=null)
        sentQueryId = serializedMessage.QueryId;
      else {
        var message = objectToSend as IMessage;
        if (message!=null)
          sentQueryId = message.QueryId = Interlocked.Increment(ref queryId);
      }
      if (responseMessage!=null && responseQueryId!=null)
        responseMessage.ResponseQueryId = responseQueryId.Value;
      lock (this) {
        byte[] messageData;
        int length;
        if (serializedMessage!=null) {
          messageData = serializedMessage.Data;
          length = messageData.Length;
        }
        else {
          sendingStream.Position = MessageHeader.HeaderLength;
          serializer.Serialize(sendingStream, objectToSend);
          messageData = sendingStream.GetBuffer();
          var header = new MessageHeader((int)sendingStream.Position - MessageHeader.HeaderLength, 0);
          header.ToBytes(messageData, 0);
          length = header.Length + MessageHeader.HeaderLength;
        }
        try {
          if (DebugInfo.DropConnectionNow)
            sendingConnection.Close();
          if (!DebugInfo.SkipSendNow) {
            sendingConnection.Send(messageData, 0, length);
          }
          DebugInfo.IncreaseMessageSentCount();
        }
        catch (Exception ex) {
          DebugInfo.IncreaseSendErrorCount();
          // Log.Error(ex, Strings.ExProviderDataSending);
          throw new MessagingException(Strings.ExProviderDataSending, ex);
        }
      }
    }

    /// <summary>
    /// Event handler, raises than response to <see cref="Ask(object)"/> received.
    /// </summary>
    private void ResponseReceived(object sender, MessageReceivedEventArgs e)
    {
      if (e.ResponseQueryId!=null && e.ResponseQueryId==sentQueryId) {
        receivedMessage = e.Message;
        if (responseReceivedWaitHandle!=null)
          responseReceivedWaitHandle.Set();
      }
    }

    private void InitializeSendingConnection()
    {
      lock (this) {
        if (sendingConnection!=null)
          sendingConnection.Dispose();
        sendingConnection =
          MessagingProviderManager.GetMessagingProvider(ReceiverUrl).CreateSendingConnection(ReceiverUrl);
      }
    }


    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="Sender"/> using existing <see cref="ISendingConnection"/>. Usually used to send reply back to remote host.
    /// </summary>
    /// <param name="sendingConnection"><see cref="ISendingConnection"/> to send reply to.</param>
    /// <param name="queryId">Query ID to fill <see cref="IResponseMessage"/>.</param>
    /// <param name="serializer"><see cref="ISerializer"/> to serialize message.</param>
    /// <param name="receiverUrl">URL of receiver to send reply to.</param>
    internal Sender(ISendingConnection sendingConnection, long queryId, ISerializer serializer, string receiverUrl)
    {
      ArgumentValidator.EnsureArgumentNotNull(sendingConnection, "sendingConnection");
      ArgumentValidator.EnsureArgumentNotNull(serializer, "serializer");
      this.serializer = serializer;
      responseQueryId = queryId;
      if (receiverUrl==null) {
        this.sendingConnection = sendingConnection;
        isExternalConnection = true;
      }
      else {
        this.receiverUrl = new EndPointInfo(receiverUrl);
        InitializeSendingConnection();
      }
      DebugInfo.IncreaseSenderCount();
    }

    /// <summary>
    /// Creates new instance of <see cref="Sender"/> using remote host's <see cref="EndPointInfo"/>.
    /// </summary>
    /// <param name="receiverUrl">Remote host url to send messages to.</param>
    /// <param name="serializer"><see cref="ISerializer"/> to serialize messages before send to remote host.</param>
    /// <param name="responseReceiver"><see cref="Receiver"/> to get responses from remote host. If no <paramref name="responseReceiver"/> provided, default <see cref="Receiver"/> will be created automatically.</param>
    public Sender(string receiverUrl, ISerializer serializer, Receiver responseReceiver)
      : this(new EndPointInfo(receiverUrl), serializer, responseReceiver)
    {
    }

    /// <summary>
    /// Creates new instance of <see cref="Sender"/> using remote host's <see cref="EndPointInfo"/>.
    /// </summary>
    /// <param name="receiverUrl">Remote host <see cref="EndPointInfo"/> to send messages to.</param>
    /// <param name="serializer"><see cref="ISerializer"/> to serialize messages before send to remote host.</param>
    /// <param name="responseReceiver"><see cref="Receiver"/> to get responses from remote host. If no <paramref name="responseReceiver"/> provided, default <see cref="Receiver"/> will be created automatically.</param>
    public Sender(EndPointInfo receiverUrl, ISerializer serializer, Receiver responseReceiver)
    {
      ArgumentValidator.EnsureArgumentNotNull(receiverUrl, "receiverUrl");
      ArgumentValidator.EnsureArgumentNotNull(serializer, "serializer");
      this.receiverUrl = receiverUrl;
      this.serializer = serializer;
      this.responseReceiver = responseReceiver;
      InitializeSendingConnection();
      DebugInfo.IncreaseSenderCount();
    }

    /// <summary>
    /// Creates new instance of <see cref="Sender"/> using remote host's <see cref="EndPointInfo"/>.
    /// </summary>
    /// <param name="receiverUrl">Remote host <see cref="EndPointInfo"/> to send messages to.</param>
    public Sender(string receiverUrl)
      : this(new EndPointInfo(receiverUrl))
    {
    }


    /// <summary>
    /// Creates new instance of <see cref="Sender"/> using remote host's <see cref="EndPointInfo"/>.
    /// </summary>
    /// <param name="receiverUrl">Remote host url to send messages to.</param>
    public Sender(EndPointInfo receiverUrl)
      : this(receiverUrl, LegacyBinarySerializer.Instance)
    {
    }

    /// <summary>
    /// Creates new instance of <see cref="Sender"/> using remote host's <see cref="EndPointInfo"/>.
    /// </summary>
    /// <param name="receiverUrl">Remote host url to send messages to.</param>
    /// <param name="serializer"><see cref="ISerializer"/> to serialize messages before send to remote host.</param>
    public Sender(string receiverUrl, ISerializer serializer)
      : this(new EndPointInfo(receiverUrl), serializer)
    {
    }

    /// <summary>
    /// Creates new instance of <see cref="Sender"/> using remote host's <see cref="EndPointInfo"/>.
    /// </summary>
    /// <param name="receiverUrl">Remote host <see cref="EndPointInfo"/> to send messages to.</param>
    /// <param name="serializer"><see cref="ISerializer"/> to serialize messages before send to remote host.</param>
    public Sender(EndPointInfo receiverUrl, ISerializer serializer)
      : this(receiverUrl, serializer, null)
    {
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
      if (!isExternalConnection && sendingConnection!=null)
        sendingConnection.Dispose();
      if (disposing) {
        if (responseReceiver!=null)
          responseReceiver.MessageReceived -= ResponseReceived;
        if (responseReceivedWaitHandle!=null)
          responseReceivedWaitHandle.Close();
        if (sendingStream!=null)
          sendingStream.Dispose();
      }
      DebugInfo.DecreaseSenderCount();
    }

    ///<summary>
    ///Allows an <see cref="T:System.Object"></see> to attempt to free resources and perform other cleanup operations before the <see cref="T:System.Object"></see> is reclaimed by garbage collection.
    ///</summary>
    ///
    ~Sender()
    {
      Dispose(false);
    }
  }
}