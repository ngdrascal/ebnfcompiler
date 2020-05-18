namespace EbnfCompiler.Sample.Impl
{
   public class SemanticErrorException : CompilerException
   {
      public SemanticErrorException(string message)
         : base(message, null)
      {
      }

      public SemanticErrorException(string message, IAstNode astNode)
         : base(message, astNode.Location)
      {
      }

      public SemanticErrorException(string message, IToken token)
         : base(message, token.Location)
      {
      }
   }
}
