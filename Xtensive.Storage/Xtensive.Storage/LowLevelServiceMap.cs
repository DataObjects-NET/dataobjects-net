// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.05

using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  public sealed class LowLevelServiceMap : SessionBound
  {
    public EntityAccessor EntityAccessor { get; private set; }


    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    public LowLevelServiceMap(Session session)
      : base(session)
    {
      EntityAccessor = new EntityAccessor(session);
    }
  }
}