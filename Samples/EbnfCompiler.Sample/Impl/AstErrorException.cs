namespace EbnfCompiler.Sample.Impl
{
    public class AstErrorException : CompilerException
    {
        public AstErrorException(string message)
           : base(message, null)
        {
        }
    }
}
