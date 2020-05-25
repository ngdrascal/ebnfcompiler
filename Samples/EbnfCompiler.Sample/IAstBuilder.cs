// ReSharper disable UnusedParameter.Global

namespace EbnfCompiler.Sample
{
    public interface IAstBuilder
    {
        IRootNode RootNode { get; }

        void StmtEnd(IToken token);

        void VarStmtStart(IToken token);
        void VarStmtIdent(IToken token);
        void VarStmtType(IToken token);
        void VarStmtEnd(IToken token);

        void PrintStart(IToken token);
        void PrintExprEnd(IToken token);

        void UnaryOp(IToken token);
        void UnaryOpEnd(IToken token);

        void BinaryOp(IToken token);
        void BinaryOpEnd(IToken token);

        void NumLiteral(IToken token);
        void StrLiteral(IToken token);

        void FactIdent(IToken token);
    }
}
