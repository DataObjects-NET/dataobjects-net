// Copyright (C) 2003-2007 Xtensive LLC, INLINE GmbH.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Threading
{
  /// <summary>
  /// Lock utility - a helper class providing more convenient ways for dealing
  /// with various locks (<see cref="Monitor"/>, <see cref="ReaderWriterLockSlim"/>).
  /// </summary>
  public static class Locker
  {
    #region Nested types: Controllers (IDisposable)

    private class ReadController
      : IDisposable
    {
      private readonly ReaderWriterLockSlim rwLock;      


      // Constructors

      /// <summary>
      /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
      /// </summary>
      /// <param name="rwLock">The <see cref="ReaderWriterLockSlim"/> to lock.</param>
      public ReadController(ReaderWriterLockSlim rwLock)
      {
        this.rwLock = rwLock;
        AcquireReadLock(this.rwLock);
      }

      // Destructor

      /// <summary>
      /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
      /// </summary>
      public void Dispose()
      {
        ReleaseReadLock(rwLock);
      }
    }

    private class ReadLockController
      : IDisposable
    {
      private readonly object toLock;      


      // Constructors

      /// <summary>
      /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
      /// </summary>
      /// <param name="toLock">To object to lock.</param>
      public ReadLockController(object toLock)
      {
        this.toLock = toLock;
        AcquireReadLock(this.toLock);
      }

      // Destructor

      /// <summary>
      /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
      /// </summary>
      public void Dispose()
      {
        ReleaseReadLock(toLock);
      }
    }

    private class WriteController
      : IDisposable
    {
      private readonly ReaderWriterLockSlim rwLock;
      private readonly LockCookie? lockCookie;


      // Constructors

      /// <summary>
      /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
      /// </summary>
      /// <param name="rwLock">The <see cref="ReaderWriterLockSlim"/> to lock.</param>
      public WriteController(ReaderWriterLockSlim rwLock)
      {
        this.rwLock = rwLock;
        lockCookie = AcquireWriteLock(rwLock);
      }

      // Destructor

      /// <summary>
      /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
      /// </summary>
      public void Dispose()
      {
        ReleaseWriteLock(rwLock, lockCookie);
      }
    }

    private class WriteLockController
      : IDisposable
    {
      private readonly object toLock;
      private readonly LockCookie? lockCookie;


      // Constructors

      /// <summary>
      ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
      /// </summary>
      /// <param name="toLock">The object to lock.</param>
      public WriteLockController(object toLock)
      {
        this.toLock = toLock;
        lockCookie = AcquireWriteLock(toLock);
      }

      // Destructor

      /// <summary>
      /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
      /// </summary>
      public void Dispose()
      {
        ReleaseWriteLock(toLock, lockCookie);
      }
    }

    private class SuspendController
      : IDisposable
    {
      private readonly LockCookie lockCookie;
      private readonly ReaderWriterLockSlim rwLock;

      public void Dispose()
      {
        RestoreLock(rwLock, lockCookie);
      }

      public SuspendController(ReaderWriterLockSlim rwLock)
      {
        this.rwLock = rwLock;
        lockCookie = ReleaseWriteLock(this.rwLock);
      }
    }

    #endregion

    #region XxxRegion methods

    public static IDisposable LockRegion(this LockType lockType, object toLock)
    {
      if (toLock == null)
        return null;
      switch (lockType) {
      case LockType.Read:
        return ReadRegion(toLock);
      case LockType.Write:
        return WriteRegion(toLock);
      case LockType.Suspend:
        return SuspendRegion((ReaderWriterLockSlim)toLock);
      default:
        throw Exceptions.InternalError("Unknown LockType.", Log.Instance);
      }
    }

    public static IDisposable ReadRegion(this ReaderWriterLockSlim rwLock)
    {
      return new ReadController(rwLock);
    }

    public static IDisposable ReadRegion(object toLock)
    {
      return new ReadLockController(toLock);
    }

    public static IDisposable WriteRegion(this ReaderWriterLockSlim rwLock)
    {
      return new WriteController(rwLock);
    }

    public static IDisposable WriteRegion(object toLock)
    {
      return new WriteLockController(toLock);
    }

    public static IDisposable SuspendRegion(this ReaderWriterLockSlim rwLock)
    {
      return new SuspendController(rwLock);
    }

    #endregion

    #region ExecuteXxx methods

    public static void Execute(this LockType lockType, object toLock, Action proc)
    {
      if (toLock == null) {
        proc();
        return;
      }
      switch (lockType) {
      case LockType.Read:
        ExecuteReader(toLock, proc);
        return;
      case LockType.Write:
        ExecuteWriter(toLock, proc);
        return;
      case LockType.Suspend:
        ExecuteSuspender((ReaderWriterLockSlim)toLock, proc);
        return;
      default:
        Debug.Fail("Unknown LockType.");
        return;
      }
    }

    public static T Execute<T>(this LockType lockType, object toLock, Func<T> func)
    {
      if (toLock == null)
        return func();
      switch (lockType) {
      case LockType.Read:
        return ExecuteReader<T>(toLock, func);
      case LockType.Write:
        return ExecuteWriter<T>(toLock, func);
      case LockType.Suspend:
        return ExecuteSuspender<T>((ReaderWriterLockSlim)toLock, func);
      default:
        Debug.Fail("Unknown LockType.");
        return default(T);
      }
    }

    public static void Execute(object objLock, Action reader)
    {
      AcquireWriteLock(objLock);
      try {
        reader();
      }
      finally {
        ReleaseWriteLock(objLock);
      }
    }

    public static T Execute<T>(object objLock, Func<T> reader)
    {
      AcquireWriteLock(objLock);
      try {
        return reader();
      }
      finally {
        ReleaseWriteLock(objLock);
      }
    }

    public static void ExecuteReader(this ReaderWriterLockSlim rwLock, Action reader)
    {
      AcquireReadLock(rwLock);
      try {
        reader();
      }
      finally {
        ReleaseReadLock(rwLock);
      }
    }

    /// <summary>
    /// Executes a <paramref name="reader"/> delegate
    /// with lock on <paramref name="toLock"/> object.
    /// </summary>
    /// <param name="toLock">An object to lock.</param>
    /// <param name="reader">A reader delegate.</param>
    public static void ExecuteReader(object toLock, Action reader)
    {
      AcquireReadLock(toLock);
      try {
        reader();
      }
      finally {
        ReleaseReadLock(toLock);
      }
    }

    public static T ExecuteReader<T>(this ReaderWriterLockSlim rwLock, Func<T> reader)
    {
      AcquireReadLock(rwLock);
      try {
        return reader();
      }
      finally {
        ReleaseReadLock(rwLock);
      }
    }

    /// <summary>
    /// Executes a <paramref name="reader"/> delegate
    /// with lock on <paramref name="toLock"/> object.
    /// </summary>
    /// <typeparam name="T">A delegate return type.</typeparam>
    /// <param name="toLock">An object to lock.</param>
    /// <param name="reader">A reader delegate.</param>
    /// <returns>An object that <paramref name="reader"/> returns.
    /// </returns>
    public static T ExecuteReader<T>(object toLock, Func<T> reader)
    {
      AcquireReadLock(toLock);
      try {
        return reader();
      }
      finally {
        ReleaseReadLock(toLock);
      }
    }

    public static void ExecuteWriter(this ReaderWriterLockSlim rwLock, Action writer)
    {
      LockCookie? lockCookieHolder = AcquireWriteLock(rwLock);
      try {
        writer();
      }
      finally {
        ReleaseWriteLock(rwLock, lockCookieHolder);
      }
    }

    /// <summary>
    /// Executes a <paramref name="writer"/> delegate
    /// with lock on <paramref name="toLock"/> object.
    /// </summary>
    /// <param name="toLock">An object to lock.</param>
    /// <param name="writer">A writer delegate.</param>
    public static void ExecuteWriter(object toLock, Action writer)
    {
      LockCookie? lockCookieHolder = AcquireWriteLock(toLock);
      try {
        writer();
      }
      finally {
        ReleaseWriteLock(toLock, lockCookieHolder);
      }
    }

    public static T ExecuteWriter<T>(this ReaderWriterLockSlim rwLock, Func<T> writer)
    {
      LockCookie? lockCookieHolder = AcquireWriteLock(rwLock);
      try {
        return writer();
      }
      finally {
        ReleaseWriteLock(rwLock, lockCookieHolder);
      }
    }

    /// <summary>
    /// Executes a <paramref name="writer"/> delegate
    /// with lock on <paramref name="toLock"/> object.
    /// </summary>
    /// <typeparam name="T">A delegate return type.</typeparam>
    /// <param name="toLock">An object to lock.</param>
    /// <param name="writer">A writer delegate.</param>
    /// <returns>An object that <paramref name="writer"/> returns.
    /// </returns>
    public static T ExecuteWriter<T>(object toLock, Func<T> writer)
    {
      LockCookie? lockCookieHolder;
      lockCookieHolder = AcquireWriteLock(toLock);
      try {
        return writer();
      }
      finally {
        ReleaseWriteLock(toLock, lockCookieHolder);
      }
    }

    public static void ExecuteSuspender(this ReaderWriterLockSlim rwLock, Action suspender)
    {
      // LockCookie lockCookie = 
      bool isWriter = rwLock.IsWriteLockHeld;
      if (isWriter)
        rwLock.ExitWriteLock();
      else 
        rwLock.ExitReadLock();
      try {
        suspender();
      }
      finally {
        if (isWriter)
          rwLock.EnterWriteLock();
        else
          rwLock.EnterReadLock();
      }
    }

    public static T ExecuteSuspender<T>(this ReaderWriterLockSlim rwLock, Func<T> suspender)
    {
      LockCookie lockCookie = ReleaseWriteLock(rwLock);
      try {
        return suspender();
      }
      finally {
        RestoreLock(rwLock, lockCookie);
      }
    }

    #endregion

    #region BeginXxx \ EndXxx methods

    /// <summary>
    /// Attempts to acquire the lock in read mode.
    /// </summary>
    /// <param name="toLock">The object to acquire the lock on.</param>
    public static void BeginRead(object toLock)
    {
      if (toLock == null)
        return;
      var rwLock = toLock as ReaderWriterLockSlim;
      if (rwLock == null)
        Monitor.Enter(toLock);
      else
        AcquireReadLock(rwLock);
    }

    /// <summary>
    /// Releases the read lock.
    /// </summary>
    /// <param name="rwLock">The object to release the lock from.</param>
    public static void EndRead(this ReaderWriterLockSlim rwLock)
    {
      if (rwLock == null)
        return;

      ReleaseReadLock(rwLock);
    }

    /// <summary>
    /// Attempts to acquire the lock in read mode.
    /// </summary>
    /// <param name="rwLock">The object to acquire the lock on.</param>
    public static void BeginRead(this ReaderWriterLockSlim rwLock)
    {
      if (rwLock == null)
        return;

      AcquireReadLock(rwLock);
    }

    /// <summary>
    /// Releases the read lock.
    /// </summary>
    /// <param name="toLock">The object to release the lock from.</param>
    public static void EndRead(object toLock)
    {
      if (toLock == null)
        return;
      var rwLock = toLock as ReaderWriterLockSlim;
      if (rwLock == null)
        Monitor.Exit(toLock);
      else
        ReleaseReadLock(rwLock);
    }

    /// <summary>
    /// Attempts to acquire the lock in write mode.
    /// </summary>
    /// <param name="toLock">The object to acquire the lock on.</param>
    public static LockCookie? BeginWrite(object toLock)
    {
      if (toLock == null)
        return null;
      var rwLock = toLock as ReaderWriterLockSlim;
      if (rwLock == null)
        Monitor.Enter(toLock);
      else
        return AcquireWriteLock(rwLock);
      return null;
    }

    /// <summary>
    /// Releases the write lock.
    /// </summary>
    /// <param name="toLock">The object to release the lock from.</param>
    public static void EndWrite(object toLock, LockCookie? lockCookieHolder)
    {
      if (toLock == null)
        return;
      var rwLock = toLock as ReaderWriterLockSlim;
      if (rwLock == null)
        Monitor.Exit(toLock);
      else
        ReleaseWriteLock(rwLock, lockCookieHolder);
    }

    /// <summary>
    /// Attempts to acquire the lock in write mode.
    /// </summary>
    /// <param name="rwLock">The object to acquire the lock on.</param>
    public static LockCookie? BeginWrite(this ReaderWriterLockSlim rwLock)
    {
      if (rwLock == null)
        return null;
      
      return AcquireWriteLock(rwLock);
    }

    /// <summary>
    /// Releases the write lock.
    /// </summary>
    /// <param name="rwLock">The object to release the lock from.</param>
    public static void EndWrite(this ReaderWriterLockSlim rwLock, LockCookie? lockCookieHolder)
    {
      if (rwLock == null)
        return;

      ReleaseWriteLock(rwLock, lockCookieHolder);
    }

    #endregion

    #region Private methods: AcquireXxxLock \ ReleaseXxxLock \ RestoreXxxLock

    // Untyped methods

    private static void AcquireReadLock(object toLock)
    {
      if (toLock == null)
        return;
      var rwLock = toLock as ReaderWriterLockSlim;
      if (rwLock != null)
        AcquireReadLock(rwLock);
      else
        Monitor.Enter(toLock);
    }

    private static void ReleaseReadLock(object toLock)
    {
      if (toLock == null)
        return;
      var rwLock = toLock as ReaderWriterLockSlim;
      if (rwLock != null)
        ReleaseWriteLock(rwLock);
      else
        Monitor.Exit(toLock);
    }

    private static LockCookie? AcquireWriteLock(object toLock)
    {
      if (toLock == null)
        return null;
      var rwLock = toLock as ReaderWriterLockSlim;
      if (rwLock != null)
        return AcquireWriteLock(rwLock);
      else
        Monitor.Enter(toLock);
      return null;
    }

    private static void ReleaseWriteLock(object toLock)
    {
      if (toLock == null)
        return;
      ReaderWriterLockSlim rwLock = toLock as ReaderWriterLockSlim;
      if (rwLock != null)
        ReleaseWriteLock(rwLock);
      else
        Monitor.Exit(toLock);
    }

    private static void ReleaseWriteLock(object toLock, LockCookie? lockCookieHolder)
    {
      if (toLock == null)
        return;
      var rwLock = toLock as ReaderWriterLockSlim;
      if (rwLock != null)
        ReleaseWriteLock(rwLock, lockCookieHolder);
      else
        Monitor.Exit(toLock);
    }

    // ReaderWriterLockSlim methods

    private static void AcquireReadLock(ReaderWriterLockSlim rwLock)
    {
      if (rwLock == null)
        return;
      rwLock.EnterReadLock();
    }

    private static void ReleaseReadLock(ReaderWriterLockSlim rwLock)
    {
      if (rwLock == null)
        return;
      if (rwLock.IsWriteLockHeld)
        rwLock.ExitWriteLock();
      else if (rwLock.IsReadLockHeld)
        rwLock.ExitReadLock();
    }

    private static LockCookie? AcquireWriteLock(ReaderWriterLockSlim rwLock)
    {
      if (rwLock == null)
        return null;
      LockCookie? lockCookieHolder = null;
      if (rwLock.IsReadLockHeld) {
        rwLock.ExitReadLock();
        rwLock.EnterWriteLock();
      }
      else if (!rwLock.IsWriteLockHeld)
        rwLock.EnterWriteLock();
      return lockCookieHolder;
    }

    // TODO: Fix (rwLock.ReleaseWriterLock is called despite of rwLock.IsWriterLockHeld == false)
    private static void ReleaseWriteLock(ReaderWriterLockSlim rwLock, LockCookie? lockCookieHolder)
    {
      if (rwLock == null)
        return;
      if (rwLock.IsWriteLockHeld)
        rwLock.ExitWriteLock();
      else if (rwLock.IsReadLockHeld)
        rwLock.ExitReadLock();
    }

    private static LockCookie ReleaseWriteLock(ReaderWriterLockSlim rwLock)
    {
      if (rwLock == null)
        return new LockCookie();
      if (rwLock.IsWriteLockHeld)
        rwLock.ExitWriteLock();
      else if (rwLock.IsReadLockHeld)
        rwLock.ExitReadLock();
      return new LockCookie();
    }

    private static void RestoreLock(ReaderWriterLockSlim rwLock, LockCookie lockCookie)
    {
      if (rwLock == null)
        return;
      if (rwLock.IsWriteLockHeld)
        rwLock.EnterWriteLock();
      else if (rwLock.IsReadLockHeld)
        rwLock.ExitReadLock();
    }

    #endregion
  }
}
