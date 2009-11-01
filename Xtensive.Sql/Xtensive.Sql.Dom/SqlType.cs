// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom
{
  [Serializable]
  public abstract class SqlType : ICloneable
  {
    ///<summary>
    ///Creates a new object that is a copy of the current instance.
    ///</summary>
    ///
    ///<returns>
    ///A new object that is a copy of this instance.
    ///</returns>
    ///<filterpriority>2</filterpriority>
    public abstract object Clone();
  }
}
