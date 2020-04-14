using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EbnfCompiler.AST.Impl;
using NUnit.Framework;

namespace EbnfCompiler.AST.UnitTests
{
   [TestFixture, ExcludeFromCodeCoverage]
   public class TerminalSetTests
   {
      [TestCase(new string[0], true)]
      [TestCase(new[] { "T" }, false)]
      public void IsEmpty_WhenTerminalsHasValues_ReturnsExpected(string[] terminals, bool expected)
      {
         // Arrange:
         var ts = new TerminalSet();
         foreach (var terminal in terminals)
         {
            ts.Add(terminal);
         }

         // Act:
         var actual = ts.IsEmpty();

         // Assert:
         Assert.AreEqual(expected, actual);
      }

      [TestCase(new[] { "T", "U", "V" }, "U", true)]
      [TestCase(new[] { "T", "U", "V" }, "W", false)]
      [TestCase(new string[0], "W", false)]
      public void Includes_WhenTerminalsIncludeTestFor_ReturnsExpected(string[] terminals, string testFor, bool expected)
      {
         // Arrange:
         var ts = new TerminalSet();
         foreach (var terminal in terminals)
         {
            ts.Add(terminal);
         }

         // Act:
         var actual = ts.Includes(testFor);

         // Assert:
         Assert.AreEqual(expected, actual);
      }

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

      [TestCase(new[] { "A", "B", "C" }, new[] { "X", "Y", "Z" }, new[] { "A", "B", "C", "X", "Y", "Z" })]
      [TestCase(new[] { "A", "B", "C" }, new[] { "C", "D", "E" }, new[] { "A", "B", "C", "D", "E" })]
      [TestCase(new string[0], new[] { "X", "Y", "Z" }, new[] { "X", "Y", "Z" })]
      public void Union_WhenTwoSets_ReturnsUnionOfSets(string[] set1, string[] set2, string[] expected)
      {
         // Arrange:
         var ts1 = new TerminalSet();
         foreach (var terminal in set1)
            ts1.Add(terminal);

         var ts2 = new TerminalSet();
         foreach (var terminal in set2)
            ts2.Add(terminal);

         // Act:
         ts1.Union(ts2, false);

         // Assert:
         Assert.That(ts1.AsEnumerable(), Is.EquivalentTo(expected));

      }

      [TestCase(new[] { "A" }, new[] { "Z" }, false, new[] { "A", "Z" })]
      [TestCase(new[] { "A" }, new[] { "Z" }, true, new[] { "A", "Z", "$EPSILON$" })]
      public void Union_WhenIncludeEpsilon_ReturnsUnionOfSets(string[] set1, string[] set2, bool includeEpsilon, string[] expected)
      {
         // Arrange:
         var ts1 = new TerminalSet();
         foreach (var terminal in set1)
            ts1.Add(terminal);

         var ts2 = new TerminalSet();
         foreach (var terminal in set2)
            ts2.Add(terminal);
         ts2.Add(ts1.Epsilon);

         // Act:
         ts1.Union(ts2, includeEpsilon);

         // Assert:
         Assert.That(ts1.AsEnumerable(), Is.EquivalentTo(expected));

      }
   }
}
