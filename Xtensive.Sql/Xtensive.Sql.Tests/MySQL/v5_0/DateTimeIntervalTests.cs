using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Xtensive.Sql.Tests.MySQL.v5_0
{
     [TestFixture, Explicit]
    public class DateTimeIntervalTests : Tests.DateTimeIntervalTest
    {
        protected override string Url { get { return TestUrl.MySQL50; } }

    }
}
