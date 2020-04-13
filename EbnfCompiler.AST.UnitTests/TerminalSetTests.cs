using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EbnfCompiler.AST.UnitTests
{
   [TestClass]
   public class TerminalSetTests
   {
      [TestMethod]
      public void WhenTerminalIsNotInSet_AddTerminalToSet()
      {
         // Arrange:
         var ts = new TerminalSet();
         const string terminal = "t";

         // Act:
         ts.Add(terminal);

         // Assert:
         Assert.AreEqual(1, ts.AsEnumerable().Count());
         Assert.IsTrue(ts.AsEnumerable().Contains(terminal));
      }

      [TestMethod]
      public void WhenTerminalIsAlreadyInSet_DoesNotAddDuplicate()
      {
         // Arrange:
         var ts = new TerminalSet();
         const string terminal = "t";

         // Act:
         ts.Add(terminal);
         ts.Add(terminal);

         // Assert:
         Assert.AreEqual(1, ts.AsEnumerable().Count());
         Assert.IsTrue(ts.AsEnumerable().Contains(terminal));
      }
   }
}
