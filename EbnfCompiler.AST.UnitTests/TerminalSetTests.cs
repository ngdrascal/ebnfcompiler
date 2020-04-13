using System.Linq;
using NUnit.Framework;

namespace EbnfCompiler.AST.UnitTests
{
   [TestFixture]
   public class TerminalSetTests
   {
      [Test]
      public void Add_WhenTerminalIsNotInSet_AddTerminalToSet()
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

      [Test]
      public void Add_WhenTerminalIsAlreadyInSet_DoesNotAddDuplicate()
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
