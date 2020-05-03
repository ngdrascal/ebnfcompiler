using Microsoft.Extensions.Logging;

namespace EbnfCompiler.Compiler
{
   public interface IDebugTracer
   {
      void BeginTrace(string message);
      void EndTrace(string message);
      void TraceLine(string message);
   }

   public class DebugTracer : IDebugTracer
   {
      private readonly ILogger _log;
      private int _traceIndent;

      public DebugTracer(ILogger log)
      {
         _log = log;
      }

      public void BeginTrace(string message)
      {
         var leader = new string(' ', _traceIndent);
         _traceIndent += 2;

         _log.LogTrace($"{leader}->{message}");
      }

      public void EndTrace(string message)
      {
         _traceIndent -= 2;
         var leader = new string(' ', _traceIndent);

         _log.LogTrace($"{leader}<-{message}");
      }

      public void TraceLine(string message)
      {
         var leader = new string(' ', _traceIndent);

         _log.LogTrace($"{leader}{message}");
      }
   }
}
