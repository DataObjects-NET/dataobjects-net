// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.01

namespace Xtensive.Integrity.Relations
{
  internal class OneToOneRelationSyncContext<TMaster, TSlave, TVariator>
  {
    public OneToOneRelationSyncStage Stage;
    public TSlave OldSlave;
    public object SceduledCallTarget;
    public object SceduledCallValue;
  }
}