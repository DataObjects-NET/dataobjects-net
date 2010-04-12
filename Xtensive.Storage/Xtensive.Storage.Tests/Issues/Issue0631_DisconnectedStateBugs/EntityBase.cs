// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.04.12

using System;
using System.Collections.Generic;

namespace Xtensive.Storage.Tests.Issues.Issue0631_DisconnectedStateBugs
{
  /// <summary>Базовый класс сущностей</summary>
  public abstract class EntityBase : Entity
  {
    /// <summary>The _original fields.</summary>
    private Dictionary<string, object> _originalFields;

    /// <summary>Initializes a new instance of the <see cref="EntityBase"/> class.</summary>
    /// <param name="id">Идентификатор элемента</param>
    protected EntityBase(Guid id) : base(id)
    {
    }

    /// <summary>Gets OriginalFields.</summary>
    public Dictionary<string, object> OriginalFields
    {
      get { return _originalFields ?? (_originalFields = new Dictionary<string, object>()); }
    }

    /// <summary>Идентификатор элемента</summary>
    [Field]
    [Key]
    public Guid Id { get; private set; }
  }
}