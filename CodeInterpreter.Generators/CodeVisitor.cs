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
        var typeName = context.type().GetText();
        var variables = context.IDENTIFIER();
        
        var contextLex = context.GetText().Replace(typeName, "");

        var varTokens = contextLex.Split(',');
        var exp = context.expression();
        int expctr = 0;

        for (int x = 0; x < varTokens.Length; x++)
        {
            if (SymbolTable.ContainsKey(variables[x].GetText()))
            {
                Console.WriteLine($"Error: Redefinition of variable {variables[x].GetText()}");
                continue;
            }
            if (varTokens[x].Contains('='))
            {
                if (expctr < exp.Count())
                {
                    if (ErrorHandler.HandleTypeError(context, Visit(exp[expctr]), (Type?)type))
                    {
                        SymbolTable[variables[x].GetText()] = Visit(exp[expctr]);
                        Types[variables[x].GetText()] = type;
                    }
                    expctr++;
                }
            }
            else
            {
                SymbolTable[variables[x].GetText()] = null;
                Types[variables[x].GetText()] = type;
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

        var termOperation = context.term_operator().GetText();

        return termOperation switch
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

        var factorOperation = context.factor_operator().GetText();

        return factorOperation switch
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

        var relationalOperation = context.relational_operator().GetText();

        return Evaluator.Relational(context, left, right, relationalOperation);
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
        
        var boolOperation = context.boolean_operator().GetText();

        return Evaluator.BoolOperation(context, left, right, boolOperation);
    }

    public override object? VisitIf_else_statement([NotNull] If_else_statementContext context)
    {
        var condition = Visit(context.expression());
        var result = ErrorHandler.HandleLogicError(context, condition);
        result = Convert.ToBoolean(result);
        
        if (ErrorHandler.HandleLogicError(context, condition) == true)
        {
            var executables = context.executables().ToList();
            foreach (var executable in executables)
            {
                Visit(executable);
            }
        }
        else
        {
            var elseIfStatements = context.else_if_statement();
            foreach (var elseIfStatement in elseIfStatements)
            {
                var elseIfCondition = Visit(elseIfStatement.expression());
                if (ErrorHandler.HandleLogicError(context, elseIfCondition) == true)
                {
                    var elseIfExecutables = elseIfStatement.executables().ToList();
                    foreach (var executable in elseIfExecutables)
                    {
                        Visit(executable);
                    }
                    return null;
                }
            }

            var elseStatement = context.else_statement();
            if (elseStatement != null)
            {
                var elseExecutables = elseStatement.executables().ToList();
                foreach (var elseExecutable in elseExecutables)
                {
                    Visit(elseExecutable);
                }
            }
        }
        return null;
    }

    public override object? VisitWhile_statement([NotNull] While_statementContext context)
    {
        var boolExpression = Visit(context.expression());
        var iterations = 0;

        while (ErrorHandler.HandleLogicError(context, boolExpression) == true)
        {
            if (iterations >= CodeConstant.MAX_LOOP_ITERATIONS)
            {
                return ErrorHandler.HandleInfiniteLoopError(context);
            }
            else
            {
                var executables = context.executables().ToList();
                foreach (var executable in executables)
                {
                    Visit(executable);
                }
                boolExpression = Visit(context.expression());
                iterations++;
            }
        }
        return null;
    }

    public override object? VisitDo_while_statement([NotNull] Do_while_statementContext context)
    {
        var boolExpression = Visit(context.expression());
        var iterations = 0;

        do
        {
            var executables = context.executables().ToList();
            foreach (var executable in executables)
            {
                Visit(executable);
            }
            boolExpression = Visit(context.expression());
            iterations++;

            if (iterations >= CodeConstant.MAX_LOOP_ITERATIONS)
            {
                return ErrorHandler.HandleInfiniteLoopError(context);
            }

        } while (ErrorHandler.HandleLogicError(context, boolExpression) == true);

        return null;
    }

    public override object? VisitEscapeSequenceExpression([NotNull] EscapeSequenceExpressionContext context)
    {
        var escSequence = context.GetText()[1];
        return Evaluator.Escape(context, escSequence) ?? ErrorHandler.HandleInvalidEscapeSequenceError(context, escSequence);
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
