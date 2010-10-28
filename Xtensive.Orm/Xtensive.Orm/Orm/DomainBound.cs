// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Orm;

namespace Xtensive.Orm
{
  /// <summary>
  /// Base class for all objects that are bound to the <see cref="Domain"/> instance.
  /// </summary>
  public abstract class DomainBound: IContextBound<Domain>
  {
    private Domain domain;

    /// <summary>
    /// Gets <see cref="Domain"/> to which current instance is bound.
    /// </summary>
    public Domain Domain
    {
      get { return domain; }
      internal set { domain = value; }
    }

    #region IContextBound<Domain> Members

    /// <inheritdoc/>
    Domain IContextBound<Domain>.Context
    {
      get { return domain; }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected DomainBound() 
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="domain"><see cref="Orm.Domain"/>, to which current instance 
    /// is bound.</param>
    /// <exception cref="ArgumentNullException"><paramref name="domain"/> is <see langword="null" />.</exception>
    protected DomainBound(Domain domain)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      this.domain = domain;
    }
  }
}