// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.10.08

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues_Issue0827_LinqDtoMaterialization;

namespace Xtensive.Storage.Tests.Issues_Issue0827_LinqDtoMaterialization
{
  [HierarchyRoot]
  public class BaseEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string SysName{ get; set; }

    [Field]
    public string Description { get; set; }
  }

  public class DocEntity : BaseEntity
  {
    [Field]
    public BaseEntity LinkedEntity { get; set; }

    [Field]
    public BaseEntity OwnerEntity { get; set; }

    [Field]
    public BaseEntity EnEntityType { get; set; }
  }
}
namespace Xtensive.Storage.Tests.Issues
{
  [Serializable]
  public class Issue0827_LinqDtoMaterialization : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (DocEntity).Assembly, typeof (DocEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var transaction = Transaction.Open(session)) {
          var interfacesDic = new Dictionary<int, Dictionary<Guid, string>>();
          var numeratorsDict = new Dictionary<int, List<NumeratorInfo>>();
          var fieldsDict = new Dictionary<int, List<DocFieldInfo>>();
          var query =
                session.Query.All<DocEntity>().Select(
                    q =>
                    new DocEntityInfo()
                        {
                            Caption = q.Name,
                            Id = q.Id,
                            Name = q.SysName,
                            Description = q.Description,
                            EntityTypeId = q.EnEntityType.Id,
                            EntityTypeName = q.EnEntityType.SysName,
                            LinkedEntitySysName = q.LinkedEntity != null ? q.LinkedEntity.SysName : null,
                            OwnerEntitySysName = q.OwnerEntity != null ? q.OwnerEntity.SysName : null,
                            FieldsInfo = fieldsDict.ContainsKey(q.Id) ? fieldsDict[q.Id] : new List<DocFieldInfo>(),
                            NumeratorInfo =
                                numeratorsDict.ContainsKey(q.Id) ? numeratorsDict[q.Id] : new List<NumeratorInfo>(),
                            Interfaces = interfacesDic.ContainsKey(q.Id) ?
                                interfacesDic[q.Id] : new Dictionary<Guid, string>()
                        });

            var result = query.ToList();
        }
      }
    }
    
  }

  public class DocEntityInfo
  {
    public Dictionary<Guid, string> Interfaces { get; set; }

    public string Caption { get; set; }

    public List<NumeratorInfo> NumeratorInfo { get; set; }

    public List<DocFieldInfo> FieldsInfo { get; set; }

    public string OwnerEntitySysName { get; set; }

    public string LinkedEntitySysName { get; set; }

    public string EntityTypeName { get; set; }

    public int EntityTypeId { get; set; }

    public string Description { get; set; }

    public string Name { get; set; }

    public int Id { get; set; }
  }
  public class DocFieldInfo
  {
    
  }

  public class NumeratorInfo
  {
    
  }
}