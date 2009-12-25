// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.25

using System;
using System.Collections;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;
using Xtensive.Messaging.Resources;

namespace Xtensive.Messaging
{
  internal sealed class MessageCollection: IMessageCollection
  {
    // Private fields

    private readonly int count;
    private TimeSpan timeout = TimeSpan.FromSeconds(10);
    private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
    private Receiver receiver;
    private bool disposed;
    private MessagingException exception;
    private readonly IMessageCollectionItem[] items;
    private int lastReceivedIndex = -1;
    private readonly AutoResetEvent waitHandle = new AutoResetEvent(false);
    private bool disposeReceiver;


    ///<summary>
    ///Copies the elements of the <see cref="T:System.Collections.ICollection"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
    ///</summary>
    ///
    ///<param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing. </param>
    ///<param name="index">The zero-based index in array at which copying begins. </param>
    ///<exception cref="T:System.ArgumentNullException">array is null. </exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">index is less than zero. </exception>
    ///<exception cref="T:System.ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"></see> is greater than the available space from index to the end of the destination array. </exception>
    public void CopyTo(Array array, int index)
    {
      if (exception != null)
        throw exception;
      ArgumentValidator.EnsureArgumentNotNull(array, "array");
      if (index < 0)
        throw new ArgumentOutOfRangeException("index", index, Strings.ExCollectionArrayIndexOutOfRange);
      if (array.Rank > 1)
        throw new ArgumentException(Strings.ExCollectionArrayMultidimensial, "array");
      IEnumerator enumerator = GetEnumerator();
      while (enumerator.MoveNext() && index < array.Length) {
        array.SetValue(enumerator.Current, index);
        index++;
      }
    }

    ///<summary>
    ///Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"></see>.
    ///</summary>
    ///
    ///<returns>
    ///The number of elements contained in the <see cref="T:System.Collections.ICollection"></see>.
    ///</returns>
    ///<filterpriority>2</filterpriority>
    public int Count
    {
      get { return count; }
    }

    ///<summary>
    ///Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
    ///</summary>
    ///
    ///<returns>
    ///An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
    ///</returns>
    ///<filterpriority>2</filterpriority>
    public object SyncRoot
    {
      get { return rwLock; }
    }

    ///<summary>
    ///Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe).
    ///</summary>
    ///
    ///<returns>
    ///true if access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe); otherwise, false.
    ///</returns>
    ///<filterpriority>2</filterpriority>
    public bool IsSynchronized
    {
      get { return true; }
    }


    public TimeSpan Timeout
    {
      get { return timeout; }
      set { timeout = value; }
    }


    ///<summary>
    ///Returns an enumerator that iterates through a collection.
    ///</summary>
    ///
    ///<returns>
    ///An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
    ///</returns>
    ///<filterpriority>2</filterpriority>
    public IEnumerator GetEnumerator()
    {
      if (exception != null)
        throw exception;
      using (rwLock.ReadRegion()) {
        int index = 0;
        while (index < count) {
          if (index <= lastReceivedIndex) {
            if (exception != null)
              throw exception;
            yield return items[index].Data;
          }
          else {
            // Wait for data;
            waitHandle.Reset();
            rwLock.ExecuteSuspender(() => waitHandle.WaitOne(timeout, true));
            if (index > lastReceivedIndex)
              throw new TimeoutException(Strings.ExCollectionTimeout);
            if (exception != null)
              throw exception;
            yield return items[index].Data;
          }
          index++;
        }
      }
    }

    public bool DisposeReceiver
    {
      set { disposeReceiver = value; }
    }

    // Private members

    private void AddItem(IMessageCollectionItem item)
    {
      using (rwLock.ReadRegion()) {
        if (exception != null)
          return;
        if (item.Sequence >= count) {
          exception = new MessagingException(Strings.ExCollectionSequenceOutOfRange);
          return;
        }
        if (item.Sequence > lastReceivedIndex + 1) {
          exception = new MessagingException(Strings.ExCollectionMessageMissing);
          return;
        }
        if (item.Sequence == lastReceivedIndex + 1) {
          using (rwLock.WriteRegion()) {
            lastReceivedIndex = item.Sequence;
            items[lastReceivedIndex] = item;
            if (lastReceivedIndex == count - 1 && disposeReceiver)
              receiver.Dispose();
            waitHandle.Set();
          }
        }
      }
    }


    // Constructors

    public MessageCollection(IMessageCollectionHeadItem headItem, Receiver receiver)
    {
      ArgumentValidator.EnsureArgumentNotNull(headItem, "headItem");
      ArgumentValidator.EnsureArgumentNotNull(receiver, "receiver");
      count = headItem.Count;
      items = new IMessageCollectionItem[count];
      this.receiver = receiver;
      receiver.CollectionItemReceived += MessageReceived;
      AddItem(headItem);
    }

    private void MessageReceived(object sender, CollectionItemReceivedEventArgs e)
    {
      AddItem(e.Item);
    }


    // Dispose and finalize

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

      if (disposed)
        return;
      disposed = true;
      if (disposing) {
        rwLock.ExecuteWriter(delegate{
          if (receiver != null && disposeReceiver)
            receiver.Dispose();
          receiver = null;
        });
        rwLock.Dispose();
        waitHandle.Close();
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Dtor" copy="true"/>
    /// </summary>
    ~MessageCollection()
    {
      Dispose(false);
    }
  }
}