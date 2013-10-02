// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.16

namespace Xtensive.Orm.Tests.Storage.Multimapping.CrossRenameModel.Version2.Namespace1
{
  [HierarchyRoot]
  public class Renamed2 : Entity
  {
    [Key, Field]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }
  }
}