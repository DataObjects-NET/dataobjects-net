// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Aspects;
using Xtensive.Core.IoC;
using Xtensive.Storage.Model;
using Xtensive.Storage.Operations;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base class for all objects that are bound to the <see cref="Session"/> instance.
  /// Methods of any descendant of this typer must be processed by PostSharp 
  /// to ensure its <see cref="Session"/> is active inside method bodies.
  /// </summary>
  [Infrastructure]
  public abstract class SessionBound : ISessionBound
  {
    private Session session;

    /// <summary>
    /// Gets <see cref="Session"/> which current instance is bound to.
    /// </summary>
    public Session Session {
      [DebuggerStepThrough]
      get { return session; }
      [DebuggerStepThrough]
      internal set { session = value; }
    }

    /// <summary>
    /// Opens the non-intermediate operation context.
    /// </summary>
    /// <returns>
    /// The instance of <see cref="IOperationContext"/> implementor.
    /// </returns>
    protected IOperationContext OpenOperationContext()
    {
      return OpenOperationContext(false);
    }

    /// <summary>
    /// Opens the operation context.
    /// </summary>
    /// <param name="isIntermediate">If set to <see langword="true" />,
    /// opened operation context will be an 
    /// <see cref="IOperationContext.IsIntermediate">intermediate</see> context.</param>
    /// <returns>
    /// The instance of <see cref="IOperationContext"/> implementor.
    /// </returns>
    protected IOperationContext OpenOperationContext(bool isIntermediate)
    {
      if (!Session.IsOperationLoggingEnabled)
        return Session.BlockingOperationContext;
      if (Session.CurrentOperationContext==null)
        return new OperationContext(Session, isIntermediate);
      if (!Session.CurrentOperationContext.IsIntermediate)
        return Session.BlockingOperationContext;
      return new OperationContext(Session, isIntermediate);
    }

    /// <summary>
    /// Ensures <see cref="Session"/> of <paramref name="other"/> is the same 
    /// as <see cref="Session"/> of this instance.
    /// </summary>
    /// <param name="other">The <see cref="SessionBound"/> object to check the session of.</param>
    /// <exception cref="ArgumentException">Session of <paramref name="other"/>
    /// 	<see cref="SessionBound"/> differs from this <see cref="Session"/>.</exception>
    protected void EnsureTheSameSession(SessionBound other)
    {
      if (Session!=other.Session)
        throw new ArgumentException(Strings.ExSessionOfAnotherSessionBoundMustBeTheSame, "other");
    }

    #region IContextBound<Session> Members

    Session IContextBound<Session>.Context {
      [DebuggerStepThrough]
      get { return session; }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected SessionBound() 
      : this(Session.Demand())
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/> 
    /// </summary>
    /// <param name="session"><see cref="Xtensive.Storage.Session"/>, to which current instance 
    /// is bound.</param>
    /// <exception cref="ArgumentNullException"><paramref name="session"/> is <see langword="null" />.</exception>
    protected SessionBound(Session session)
    {
      if (session==null)
        throw new InvalidOperationException(
          Strings.ExSessionIsNotOpen);

      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      this.session = session;
    }
  }
}