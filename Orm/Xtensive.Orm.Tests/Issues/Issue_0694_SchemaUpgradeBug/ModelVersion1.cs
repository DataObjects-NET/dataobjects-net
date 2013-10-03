// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Issues.Issue_0694_SchemaUpgradeBug.Model.Version1
{
  [Serializable]
  [HierarchyRoot]
  public sealed class Status : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    [Association(PairTo = "Statuses")]
    public EntitySet<Content> AssociatedContent { get; private set; }

    public override string ToString()
    {
      return Title;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Content : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public EntitySet<Status> Statuses { get; private set; }

    public override string ToString()
    {
      return string.Format("{0} (Statuses: {1})", Title, Statuses.ToCommaDelimitedString());
    }
  }

  [Serializable]
  public class Media : Content
  {
    [Field]
    public string Data { get; set; }
  }
}