using Mono.Cecil.Cil;

namespace EbnfCompiler.Sample.Impl
{
    internal class StackElementBase
    {
    }

    internal class VarStmtElement : StackElementBase
    {
        public VariableDefinition VariableDef { get; set; }
    }

    internal class PrintStmtElement : StackElementBase
    {
    }

    internal class PrintExprElement : StackElementBase
    {
    }

    internal class UnaryOperatorElement : StackElementBase
    {
        public UnaryOperators Operator { get; set; }
    }

    internal class BinaryOperatorElement : StackElementBase
    {
        public BinaryOperators Operator { get; set; }

        public string TypeName { get; set; }
    }

    internal class NumberLiteralElement : StackElementBase
    {
    }

    internal class StringLiteralElement : StackElementBase
    {
    }

    internal class VarReferenceElement : StackElementBase
    {
        public string TypeName { get; set; }
    }
}
