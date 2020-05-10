using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EbnfCompiler.AST.Impl;
using EbnfCompiler.Compiler;
using Moq;
using NUnit.Framework;

namespace EbnfCompiler.AST.UnitTests
{
   [TestFixture, ExcludeFromCodeCoverage]
   public class AstTraverserTests
   {
      [Test]
      public void AstTraverser_GivenANodeType_CallsProcess()
      {
         // Arrange:
         var tally = InitTally();

         var tracer = new Mock<IDebugTracer>().Object;

         var traverser = new AstTraverser(tracer);
         traverser.ProcessNode += node => tally[node.AstNodeType]++;
         var tree = BuildTree(tracer);

            // Act:
         traverser.Traverse(tree);

         // Assert:
         Assert.That(tally[AstNodeType.Syntax], Is.EqualTo(1));
         Assert.That(tally[AstNodeType.Statement], Is.EqualTo(2));
         Assert.That(tally[AstNodeType.Expression], Is.EqualTo(5));
         Assert.That(tally[AstNodeType.Term], Is.EqualTo(7));
         Assert.That(tally[AstNodeType.Factor], Is.EqualTo(8));
         Assert.That(tally[AstNodeType.ProdRef], Is.EqualTo(4));
         Assert.That(tally[AstNodeType.Terminal], Is.EqualTo(1));
         Assert.That(tally[AstNodeType.Paren], Is.EqualTo(1));
         Assert.That(tally[AstNodeType.Option], Is.EqualTo(1));
         Assert.That(tally[AstNodeType.KleeneStar], Is.EqualTo(1));
         Assert.That(tally[AstNodeType.Action], Is.EqualTo(2));
      }

      private Dictionary<AstNodeType, int> InitTally()
      {
         var tally = new Dictionary<AstNodeType, int>()
         {
            {AstNodeType.Syntax, 0},
            {AstNodeType.Statement, 0},
            {AstNodeType.Expression, 0},
            {AstNodeType.Term, 0},
            {AstNodeType.Factor, 0},
            {AstNodeType.ProdRef, 0},
            {AstNodeType.Terminal, 0},
            {AstNodeType.Paren, 0},
            {AstNodeType.Option, 0},
            {AstNodeType.KleeneStar, 0},
            {AstNodeType.Action, 0}
         };
         return tally;
      }

      private ISyntaxNode BuildTree(IDebugTracer tracer)
      {
         using var sb = new SyntaxBuilder(tracer);
         sb.Syntax(
            sb.Statement("<S>",
               sb.Expression(
                  sb.Term(
                     sb.Factor(sb.Terminal("a")),
                     sb.Factor(sb.ProdRef("<T>"))
                  ),
                  sb.Term(
                     sb.Factor("BeforeFactor", "AfterFactor",
                        sb.Paren(
                           sb.Expression(
                              sb.Term(
                                 sb.Factor(sb.ProdRef("<U>"))
                              )
                           )
                        )
                     )
                  ),
                  sb.Term(
                     sb.Factor(
                        sb.Option(
                           sb.Expression(
                              sb.Term(
                                 sb.Factor(sb.ProdRef("<V>"))
                              )
                           )
                        )
                     )
                  )
               )
            ),
            sb.Statement("<S>",
               sb.Expression(
                  sb.Term(
                     sb.Factor(
                        sb.Kleene(
                           sb.Expression(
                              sb.Term(
                                 sb.Factor(sb.ProdRef("<W>"))
                              )
                           )
                        )
                     )
                  )
               )
            )
         );

         return sb.BuildTree();
      }
   }
}
