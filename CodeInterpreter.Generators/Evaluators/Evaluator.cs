using Antlr4.Runtime;
using CodeInterpreter.Generators.ErrorHandlers;
using Antlr4.Runtime.Misc;

namespace CodeInterpreter.Generators.Evaluators;

public class Evaluator
{
    public static object? Unary([NotNull] ParserRuleContext context, string symbol, object? value)
    {
        if (symbol == "+")
            return value;

        if (symbol == "-")
        {
            if (value is int i)
                return -i;
            if (value is float f)
                return -f;
        }

        return ErrorHandler.HandleUnaryError(context, symbol);
    }

    public static object? Add([NotNull] ParserRuleContext context, object? left, object? right)
    {
        if (left is int i && right is int j)
            return i + j;

        if (left is float f && right is float g)
            return f + g;

        if (left is int lInt && right is float rFloat)
            return lInt + rFloat;

        if (left is float lFloat && right is int rInt)
            return lFloat + rInt;

        //concatenate
        if (left is string || right is string)
            return $"{left}{right}";

        return ErrorHandler.HandleInvalidOperatorError(context, left, right, "add");
    }

    public static object? Subtract([NotNull] ParserRuleContext context, object? left, object? right)
    {
        if (left is int i && right is int j)
            return i - j;

        if (left is float f && right is float g)
            return f - g;

        if (left is int lInt && right is float rFloat)
            return lInt - rFloat;

        if (left is float lFloat && right is int rInt)
            return lFloat - rInt;

        return ErrorHandler.HandleInvalidOperatorError(context, left, right, "subtract");
    }

    public static object? Multiply([NotNull] ParserRuleContext context, object? left, object? right)
    {
        if (left is int i && right is int j)
            return i * j;

        if (left is float f && right is float g)
            return f * g;

        if (left is int lInt && right is float rFloat)
            return lInt * rFloat;

        if (left is float lFloat && right is int rInt)
            return lFloat * rInt;

        return ErrorHandler.HandleInvalidOperatorError(context, left, right, "multiply");
    }

    public static object? Divide([NotNull] ParserRuleContext context, object? left, object? right)
    {
        if (left is int i && right is int j)
            return i / j;

        if (left is float f && right is float g)
            return f / g;

        if (left is int lInt && right is float rFloat)
            return lInt / rFloat;

        if (left is float lFloat && right is int rInt)
            return lFloat / rInt;

        return ErrorHandler.HandleInvalidOperatorError(context, left, right, "divide");
    }

    public static object? Modulo([NotNull] ParserRuleContext context, object? left, object? right)
    {
        if (left is int i && right is int j)
            return i % j;

        if (left is float f && right is float g)
            return f % g;

        if (left is int lInt && right is float rFloat)
            return lInt % rFloat;

        if (left is float lFloat && right is int rInt)
            return lFloat % rInt;

        return ErrorHandler.HandleInvalidOperatorError(context, left, right, "get modulo for the");
    }

    public static object? Relational([NotNull] ParserRuleContext context, object? left, object? right, string op)
    {
        if (op == ">")
        {
            if (left is int leftInt && right is int rightInt)
            {
                return leftInt > rightInt;
            }
            else if (left is float leftFloat && right is float rightFloat)
            {
                return leftFloat > rightFloat;
            }
            else
            {
                return ErrorHandler.HandleInvalidRelationOperatorError(context, left, right, op);
            }
        }
        else if (op == "<")
        {
            if (left is int leftInt && right is int rightInt)
            {
                return leftInt < rightInt;
            }
            else if (left is float leftFloat && right is float rightFloat)
            {
                return leftFloat < rightFloat;
            }
            else
            {
                return ErrorHandler.HandleInvalidRelationOperatorError(context, left, right, op);
            }
        }
        else if (op == ">=")
        {
            if (left is int leftInt && right is int rightInt)
            {
                return leftInt >= rightInt;
            }
            else if (left is float leftFloat && right is float rightFloat)
            {
                return leftFloat >= rightFloat;
            }
            else
            {
                return ErrorHandler.HandleInvalidRelationOperatorError(context, left, right, op);
            }
        }
        else if (op == "<=")
        {
            if (left is int leftInt && right is int rightInt)
            {
                return leftInt <= rightInt;
            }
            else if (left is float leftFloat && right is float rightFloat)
            {
                return leftFloat <= rightFloat;
            }
            else
            {
                return ErrorHandler.HandleInvalidRelationOperatorError(context, left, right, op);
            }
        }
        else if (op == "==")
        {
            return left?.Equals(right);
        }
        else if (op == "<>")
        {
            return !left?.Equals(right);
        }
        else
        {
            return ErrorHandler.HandleInvalidOperatorError(context, op, "");
        }
    }


    public static object? Negation([NotNull] ParserRuleContext context, object? op)
    {
        var not = Convert.ToBoolean(op);

        if (op is bool boolValue)
        {
            return !boolValue;
        }
        else
        {
            return ErrorHandler.HandleNegationError(context);
        }
    }

    public static object? BoolOperation([NotNull] ParserRuleContext context, object? left, object? right, string boolop)
    {
        switch (boolop)
        {
            case "AND":
                return (Convert.ToBoolean(left) && Convert.ToBoolean(right));
            case "OR":
                return (Convert.ToBoolean(left) || Convert.ToBoolean(right));
            default:
                return ErrorHandler.HandleBoolOperationError(context, boolop);
        }
    }

    public static object? Escape([NotNull] ParserRuleContext context, object? sequence)
    {
        if (sequence != null)
        {
            sequence = Convert.ToChar(sequence);
            return $"{sequence}";
        }
        else
        {
            return ErrorHandler.HandleInvalidEscapeSequenceError(context, sequence);
        }
    }
}
