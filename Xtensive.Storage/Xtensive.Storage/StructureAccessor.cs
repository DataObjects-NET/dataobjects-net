// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.01

namespace Xtensive.Storage
{
  public sealed class StructureAccessor : PersistentAccessor
  {
    protected internal override void OnGettingField(Persistent obj, Model.FieldInfo field)
    {
      base.OnGettingField(obj, field);
      Structure structure = (Structure) obj;
      if (structure.Owner != null)
        Session.GetAccessor(obj).OnGettingField(structure.Owner, structure.Field);
    }

    protected internal override void OnGetField(Persistent obj, Model.FieldInfo field)
    {
      Structure structure = (Structure) obj;
      if (structure.Owner != null)
        Session.GetAccessor(obj).OnGetField(structure.Owner, structure.Field);
      base.OnGetField(obj, field);
    }

    protected internal override void OnSettingField(Persistent obj, Model.FieldInfo field)
    {
      base.OnSettingField(obj, field);
      Structure structure = (Structure) obj;
      if (structure.Owner != null)
        Session.GetAccessor(obj).OnSettingField(structure.Owner, structure.Field);
    }

    protected internal override void OnSetField(Persistent obj, Model.FieldInfo field)
    {
      Structure structure = (Structure) obj;
      if (structure.Owner != null)
        Session.GetAccessor(obj).OnSetField(structure.Owner, structure.Field);
      base.OnSetField(obj, field);
    }

    public StructureAccessor(Session session)
      : base(session)
    {
    }
  }
}