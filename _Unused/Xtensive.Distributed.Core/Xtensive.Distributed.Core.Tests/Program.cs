using System;
using System.Collections.Generic;
using System.Text;

namespace Xtensive.Distributed.Core.Tests
{
  class Program
  {
    static void Main(string[] args)
    {
      //SimpleElectionsLocal e = new SimpleElectionsLocal();
      //e.ElectionsLocal();

      SimpleElectionsDistributed ed = new SimpleElectionsDistributed();
      ed.Init();
      ed.StartElections();
    }
  }
}
