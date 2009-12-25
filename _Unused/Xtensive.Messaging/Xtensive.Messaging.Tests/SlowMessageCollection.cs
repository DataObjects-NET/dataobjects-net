// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.25

using System;
using System.Collections;
using System.Threading;

namespace Xtensive.Messaging.Tests
{
  public class SlowMessageCollection : IMessageCollection
  {
    private TimeSpan timeout;
    private TimeSpan sleepTime;
    private int count = 10;

    public TimeSpan SleepTime
    {
      get { return sleepTime; }
      set { sleepTime = value; }
    }

    #region IMessageCollection Members

    public TimeSpan Timeout
    {
      get { return timeout; }
      set { timeout = value; }
    }

    #endregion

    #region ICollection Members

    ///<summary>
    ///Copies the elements of the <see cref="T:System.Collections.ICollection"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
    ///</summary>
    ///
    ///<param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing. </param>
    ///<param name="index">The zero-based index in array at which copying begins. </param>
    ///<exception cref="T:System.ArgumentNullException">array is null. </exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">index is less than zero. </exception>
    ///<exception cref="T:System.ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"></see> is greater than the available space from index to the end of the destination array. </exception>
    ///<exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.ICollection"></see> cannot be cast automatically to the type of the destination array. </exception><filterpriority>2</filterpriority>
    public void CopyTo(Array array, int index)
    {
      throw new NotImplementedException();
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
      get { return this; }
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
      get { return false; }
    }

    #endregion

    #region IEnumerable Members

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
      for (int i=0; i<count;i++) {
        Thread.Sleep(sleepTime);
        yield return i;
      }
    }

    #endregion

    #region IDisposable Members

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
      throw new NotImplementedException();
    }

    #endregion

    
    // Constructors

    public SlowMessageCollection(TimeSpan sleepTime, int count)
      : this (TimeSpan.FromSeconds(10), sleepTime, count)
    {
    }

    public SlowMessageCollection(TimeSpan timeout, TimeSpan sleepTime, int count)
    {
      this.timeout = timeout;
      this.sleepTime = sleepTime;
      this.count = count;
    }
  }
}