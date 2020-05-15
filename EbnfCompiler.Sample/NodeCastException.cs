using System;

namespace EbnfCompiler.Sample
{
   public class NodeCastException : Exception
   {
      public NodeCastException()
      {
      }

      public NodeCastException(string message)
         : base(message)
      {
      }

      public NodeCastException(string message, Exception innerException)
         : base(message, innerException)
      {
      }
   }
}
