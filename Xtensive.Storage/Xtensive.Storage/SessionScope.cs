// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.29

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.IoC;

namespace Xtensive.Storage
{
  /// <summary>
  /// <see cref="Session"/> activation scope. 
  /// </summary>
  public sealed class SessionScope : Scope<Session>
  {
    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public static Session CurrentSession
    {
      get { return CurrentContext; }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public Session Session
    {
      get { return Context; }
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session to activate.</param>
    public SessionScope(Session session)
      : base(session)
    {
    }
  }
}