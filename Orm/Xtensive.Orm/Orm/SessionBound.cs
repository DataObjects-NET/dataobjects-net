// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Aspects;
using Xtensive.IoC;
using Xtensive.Orm.Model;
using Xtensive.Orm.Operations;


namespace Xtensive.Orm
{
  /// <summary>
  /// Base class for any object that is bound to <see cref="Session"/> instance.
  /// Methods of descendants of this interface must be processed by PostSharp 
  /// to ensure their own <see cref="Session"/> is activated inside their method bodies, 
  /// and transaction is already opened there.
  /// </summary>
  /// <remarks>
  /// Only public and protected methods and properties are processed by
  /// <see cref="TransactionalTypeAttribute"/> aspect.
  /// To override the default behavior, use <see cref="TransactionalAttribute"/> and
  /// <see cref="InfrastructureAttribute"/>.
  /// </remarks>
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
    /// <param name="session"><see cref="Orm.Session"/>, to which current instance 
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