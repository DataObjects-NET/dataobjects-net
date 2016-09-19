// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.08.01

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public class JoinResult<T>
  {
    public long LeftId { get; set; }
    public long RightId { get; set; }
    public T LeftDateTime { get; set; }
    public T RightDateTime { get; set; }

    public override bool Equals(object obj)
    {
      var equalTo = obj as JoinResult<T>;
      if (equalTo==null)
        return false;
      return LeftId==equalTo.LeftId &&
             RightId==equalTo.RightId &&
             LeftDateTime.Equals(equalTo.LeftDateTime) && RightDateTime.Equals(equalTo.RightDateTime);
    }
  }
}
