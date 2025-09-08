// Copyright (C) 2016-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Groznov
// Created:    2016.08.01

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsets
{
  public class ComparisonTest : DateTimeOffsetBaseTest
  {
    [Test]
    public void EqualsTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==FirstDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==NullableDateTimeOffset);

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==WrongDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==WrongDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==null);
      });
    }

    [Test]
    public void EqualsToDateTimeOffsetWithAnotherOffset()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==FirstDateTimeOffset.ToOffset(FirstOffset));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==FirstDateTimeOffset.ToOffset(SecondOffset));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==FirstDateTimeOffset.ToOffset(TimeSpan.Zero));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==FirstDateTimeOffset.ToLocalTime());
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==FirstDateTimeOffset.ToUniversalTime());

        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.ToOffset(FirstOffset));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.ToOffset(SecondOffset));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.ToOffset(TimeSpan.Zero));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.ToLocalTime());
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.ToUniversalTime());

        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==NullableDateTimeOffset.ToOffset(FirstOffset));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==NullableDateTimeOffset.ToOffset(SecondOffset));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==NullableDateTimeOffset.ToOffset(TimeSpan.Zero));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==NullableDateTimeOffset.ToLocalTime());
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==NullableDateTimeOffset.ToUniversalTime());

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==WrongDateTimeOffset.ToOffset(FirstOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==WrongDateTimeOffset.ToOffset(SecondOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==WrongDateTimeOffset.ToOffset(TimeSpan.Zero));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==WrongDateTimeOffset.ToLocalTime());
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==WrongDateTimeOffset.ToUniversalTime());

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.ToOffset(FirstOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.ToOffset(SecondOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.ToOffset(TimeSpan.Zero));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.ToLocalTime());
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.ToUniversalTime());

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==WrongDateTimeOffset.ToOffset(FirstOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==WrongDateTimeOffset.ToOffset(SecondOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==WrongDateTimeOffset.ToOffset(TimeSpan.Zero));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==WrongDateTimeOffset.ToLocalTime());
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==WrongDateTimeOffset.ToUniversalTime());
      });
    }

    [Test]
    public void EqualsToUtcDateTime()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==FirstDateTimeOffset.UtcDateTime);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.UtcDateTime);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==NullableDateTimeOffset.UtcDateTime);

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==WrongDateTimeOffset.UtcDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.UtcDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==WrongDateTimeOffset.UtcDateTime);
      });
    }

    [Test]
    public void EqualsToLocalDateTime()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==FirstDateTimeOffset.LocalDateTime);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==FirstMillisecondDateTimeOffset.LocalDateTime);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==NullableDateTimeOffset.LocalDateTime);

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset==WrongDateTimeOffset.LocalDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset==WrongMillisecondDateTimeOffset.LocalDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset==WrongDateTimeOffset.LocalDateTime);
      });
    }

    [Test]
    public void CompareTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset > FirstDateTimeOffset.AddHours(-1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset > FirstMillisecondDateTimeOffset.AddMilliseconds(-1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset > NullableDateTimeOffset.AddYears(-1));

        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset < FirstDateTimeOffset.AddHours(1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset < FirstMillisecondDateTimeOffset.AddMilliseconds(1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset < NullableDateTimeOffset.AddYears(1));

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset > FirstDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset < FirstMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset > NullableDateTimeOffset.AddSeconds(11));
      });
    }

    [Test]
    public void CompareToDateTimeTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset > FirstDateTimeOffset.ToLocalTime().AddHours(-1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset > FirstMillisecondDateTimeOffset.ToUniversalTime().AddMilliseconds(-1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset > NullableDateTimeOffset.ToLocalTime().Date);

        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset < FirstDateTimeOffset.ToLocalTime().AddHours(1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset < FirstMillisecondDateTimeOffset.ToUniversalTime().AddMilliseconds(1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset < NullableDateTimeOffset.ToUniversalTime().AddYears(1));

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset > FirstDateTimeOffset.ToLocalTime());
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset < FirstMillisecondDateTimeOffset.ToLocalTime());
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset > NullableDateTimeOffset.AddDays(1).ToUniversalTime());
      });
    }

    [Test]
    public void CompareToNullableDateTimeOffsetToOffsetTest()
    {
      ExecuteInsideSession((s) => {
        // here, NO actual ToOffset call happens, result of operation is treaded as parameter,
        // not the original NullableDateTimeOffset
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset != NullableDateTimeOffset.ToOffset(FirstOffset));
      });
    }

    [Test]
    public void CompareToNullableDateTimeOffsetValToOffsetTest()
    {
      DateTimeOffset? nullableDateTimeOffset = NullableDateTimeOffset;
      ExecuteInsideSession((s) => {
        // here, actual ToOffset call happens and there is not translation for it at the moment
        var ex = Assert.Throws<QueryTranslationException>(() =>
          RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset != nullableDateTimeOffset.Value.ToOffset(FirstOffset)));
        Assert.That(ex.InnerException, Is.Not.Null);
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
      });
    }

    [Test]

    public void CompareToNullableDateTimeOffsetGetUtcDateTimeTest()
    {
      DateTimeOffset? nullableDateTimeOffset = NullableDateTimeOffset;
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset != NullableDateTimeOffset.UtcDateTime);

      });
    }

    [Test]
    public void CompareToNullableDateTimeOffsetValGetUtcDateTimeTest()
    {
      DateTimeOffset? nullableDateTimeOffset = NullableDateTimeOffset;
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset != nullableDateTimeOffset.Value.UtcDateTime);
      });
    }

    [Test]
    public void CompareToNullableDateTimeOffsetGetLocalDateTimeTest()
    {
      var some = NullableDateTimeOffset;
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset != some.LocalDateTime);
      });
    }

    [Test]
    public void CompareToNullableDateTimeOffsetValGetLocalDateTimeTest()
    {
      DateTimeOffset? nullableDateTimeOffset = NullableDateTimeOffset;
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset != nullableDateTimeOffset.Value.LocalDateTime);
      });
    }
  }
}
