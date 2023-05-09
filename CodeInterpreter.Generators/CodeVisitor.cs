using Antlr4.Runtime.Misc;
using CodeInterpreter.Generators.Content;
using CodeInterpreter.Generators.Evaluators;
using CodeInterpreter.Generators.ErrorHandlers;
using CodeInterpreter.Generators.CodeConstants;
using static CodeInterpreter.Generators.Content.CODEParser;
using static Antlr4.Runtime.Atn.SemanticContext;

namespace CodeInterpreter.Generators;

public class CodeVisitor : CODEBaseVisitor<object?>
{
    private Dictionary<string, object?> SymbolTable { get; set; } = new Dictionary<string, object?>();
    private Dictionary<string, object?> Types { get; set; } = new Dictionary<string, object?>();

    public override object? VisitProgram([NotNull] ProgramContext context)
    {
        return base.VisitProgram(context);
    }

    public override object? VisitDeclarations ([NotNull] DeclarationsContext context)
    {
        var type = Visit(context.type());
        var typestr = context.type().GetText();
        var varnames = context.IDENTIFIER();
        
        var contextstring = context.GetText().Replace(typestr, "");

        var contextParts = contextstring.Split(',');
        var exp = context.expression();
        int expctr = 0;

        for (int x = 0; x < contextParts.Length; x++)
        {
            if (SymbolTable.ContainsKey(varnames[x].GetText()))
            {
                Console.WriteLine($"Error: Redefinition of variable {varnames[x].GetText()}");
                continue;
            }
            if (contextParts[x].Contains('='))
            {
                if (expctr < exp.Count())
                {
                    if (ErrorHandler.HandleTypeError(context, Visit(exp[expctr]), (Type?)type))
                    {
                        SymbolTable[varnames[x].GetText()] = Visit(exp[expctr]);
                        Types[varnames[x].GetText()] = type;
                    }
                    expctr++;
                }
            }
            else
            {
                SymbolTable[varnames[x].GetText()] = null;
                Types[varnames[x].GetText()] = type;
            }
        }
        return null;
    }

    public override object? VisitAssignment([NotNull] AssignmentContext context)
    {
        foreach (var i in context.IDENTIFIER())
        {
            var expression = context.expression().Accept(this);

            // check type
            if (ErrorHandler.HandleTypeError(context, expression, (Type?)Types[i.GetText()]))
            {
                SymbolTable[i.GetText()] = expression;
            }
        }
        return null;
    }

    public override object VisitVariable_declaration([NotNull] Variable_declarationContext context)
    {
        var varName = context.IDENTIFIER().GetText();
        return CodeConstant.VariableDeclarations(SymbolTable, varName, context)!;
    }

    public override object? VisitConstant([NotNull] ConstantContext context)
    {
        var constant = context.GetText();
        return CodeConstant.Data(constant, context);
    }

    public override object? VisitBuiltin_display([NotNull] Builtin_displayContext context)
    {
        var exp = Visit(context.expression());
        return CodeConstant.BuiltinDisplay(exp);
    }

    public override object? VisitType([NotNull] TypeContext context)
    {
        return CodeConstant.Type(context);
    }

    public override object? VisitConcatExpression ([NotNull] ConcatExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        return CodeConstant.Append(left, right);
    }

    public override object? VisitIdentifierExpression([NotNull] IdentifierExpressionContext context)
    {
        var identifier = context.IDENTIFIER().GetText();
        return CodeConstant.Identifier(SymbolTable, identifier, context);
    }

    public override object? VisitConstantExpression([NotNull] ConstantExpressionContext context)
    {
        return CodeConstant.ConstantExpressionParser(context);
    }

    public override object? VisitVariable_assignment([NotNull] Variable_assignmentContext context)
    {
        var name = context.IDENTIFIER().GetText();

        Types[name] = Visit(context.type());
        return SymbolTable[name] = null;
    }

    public override object? VisitUnaryExpression([NotNull] UnaryExpressionContext context)
    {
        return Evaluator.Unary(context, context.unary_operator().GetText(), Visit(context.expression()));
    }

    public override object? VisitTermExpression([NotNull] TermExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        var ops = context.term_operator().GetText();

        return ops switch
        {
            "+" => Evaluator.Add(context, left, right),
            "-" => Evaluator.Subtract(context, left, right),
            _ => throw new NotImplementedException(),
        };
    }


    public override object? VisitFactorExpression([NotNull] FactorExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        var ops = context.factor_operator().GetText();

        return ops switch
        {
            "*" => Evaluator.Multiply(context, left, right),
            "/" => Evaluator.Divide(context, left, right),
            "%" => Evaluator.Modulo(context, left, right),
            _ => throw new NotImplementedException(),
        };
    }

    public override object? VisitRelationalExpression([NotNull] RelationalExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        var ops = context.relational_operator().GetText();

        return Evaluator.Relational(context, left, right, ops);
    }

    public override object? VisitParenExpression([NotNull] ParenExpressionContext context)
    {
        return Visit(context.expression());
    }

    public override object? VisitNotExpression([NotNull] NotExpressionContext context)
    {
        var expressionValue = Visit(context.expression());

        return Evaluator.Negation(context, expressionValue);
    }

    public override object? VisitBooleanExpression([NotNull] BooleanExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));
        var boolop = context.boolean_operator().GetText();

        return Evaluator.BoolOperation(context, left, right, boolop);
    }

    public override object? VisitIf_else_statement([NotNull] If_else_statementContext context)
    {
        var condition = Visit(context.expression());

        var result = ErrorHandler.HandleLogicError(context, condition);
        result = Convert.ToBoolean(result);
        if (ErrorHandler.HandleLogicError(context, condition) == true)
        {
            var lines = context.executables().ToList();
            foreach (var line in lines)
            {
                Visit(line);
            }
        }
        else
        {
            var elseIfBlocks = context.else_if_statement();
            foreach (var elseIfBlock in elseIfBlocks)
            {
                var elseIfCondition = Visit(elseIfBlock.expression());
                if (ErrorHandler.HandleLogicError(context, elseIfCondition) == true)
                {
                    var elseIfLines = elseIfBlock.executables().ToList();
                    foreach (var line in elseIfLines)
                    {
                        Visit(line);
                    }
                    return null;
                }
            }

            var elseStatement = context.else_statement();
            if (elseStatement != null)
            {
                var elseLines = elseStatement.executables().ToList();
                foreach (var line in elseLines)
                {
                    Visit(line);
                }
            }
        }
        return null;
    }

    public override object? VisitWhile_statement([NotNull] While_statementContext context)
    {
        var condition = Visit(context.expression());
        var maxIterations = 1000;
        var iterations = 0;

        while (ErrorHandler.HandleLogicError(context, condition) == true)
        {
            if (iterations >= maxIterations)
            {
                return ErrorHandler.HandleInfiniteLoopError(context);
            }
            else
            {
                var lines = context.executables().ToList();
                foreach (var line in lines)
                {
                    Visit(line);
                }
                condition = Visit(context.expression());
                iterations++;
            }
        }
        return null;
    }

    public override object? VisitDo_while_statement([NotNull] Do_while_statementContext context)
    {
        var condition = Visit(context.expression());
        var maxIterations = 1000;
        var iterations = 0;

        do
        {
            var lines = context.executables().ToList();
            foreach (var line in lines)
            {
                Visit(line);
            }
            condition = Visit(context.expression());
            iterations++;

            if (iterations >= maxIterations)
            {
                return ErrorHandler.HandleInfiniteLoopError(context);
            }

        } while (ErrorHandler.HandleLogicError(context, condition) == true);

        return null;
    }

    public override object? VisitEscapeSequenceExpression([NotNull] EscapeSequenceExpressionContext context)
    {
        var sequence = context.GetText()[1];
        return Evaluator.Escape(context, sequence) ?? ErrorHandler.HandleInvalidEscapeSequenceError(context, sequence);
    }

    public override object? VisitNewlineExpression ([NotNull] NewlineExpressionContext context)
    {
        return "\n";
    }

    public override object? VisitBuiltin_scan([NotNull] Builtin_scanContext context)
    {
        var input = Console.ReadLine();
        var inputs = input!.Split(',').Select(s => s.Trim()).ToArray();

        if (inputs.Length < 1 || inputs.Length > context.IDENTIFIER().Length)
        {
            return ErrorHandler.HandleInvalidScanInputsError(context, context.IDENTIFIER().Length, inputs.Length);
        }

        for (int i = 0; i < inputs.Length; i++)
        {
            var idName = context.IDENTIFIER(i).GetText();
            if (!SymbolTable.ContainsKey(idName))
            {
                return ErrorHandler.HandleUndeclaredVariableError(context, Types, idName);
            }
            CodeConstant.Scan(context, Types, SymbolTable, idName, inputs[i]);
        }

        return null;
    }
}
