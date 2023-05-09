using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace CodeInterpreter.Generators.ErrorHandlers;

public class ErrorHandler
{
    public static void HandleUndefinedVariableError([NotNull] ParserRuleContext context, object? variableName)
    {
        var line = context.Start.Line;
        var col = context.Start.Column;
        Console.WriteLine($"Error: {line}:{col} -> variable '{variableName}' is not defined.\n");
        Environment.Exit(400);
    }
    
    public static bool? HandleLogicError([NotNull] ParserRuleContext context, object? value)
    {
        if (value is bool b)
        {
            return b;
        }
        else
        {
            var line = context.Start.Line;
            var col = context.Start.Column;
            Console.WriteLine($"Error: {line}:{col} -> cannot convert {value} to boolean. ");
            Environment.Exit(400);
            return false;
        }
    }

    public static bool HandleTypeError([NotNull] ParserRuleContext context, object? obj, Type? type)
    {
        if (obj is int || obj is float || obj is bool || obj is char || obj is string)
        {
            if (obj.GetType() == type)
            {
                return true;
            }
            else
            {
                var line = context.Start.Line;
                var col = context.Start.Column;
                Console.WriteLine($"Error: {line}:{col} -> cannot convert {obj?.GetType().Name.ToUpper()} to {type?.Name.ToUpper()}.");
                Environment.Exit(400);
                return false;
            }
        }
        else
        {
            var line = context.Start.Line;
            var col = context.Start.Column;
            Console.WriteLine($"Error: {line}:{col} -> cannot convert {obj?.GetType().Name.ToUpper()} to {type?.Name.ToUpper()}.");
            Environment.Exit(400);
            return false;
        }
    }

    public static object? HandleBoolOperationError([NotNull] ParserRuleContext context, string boolop)
    {
        var line = context.Start.Line;
        var col = context.Start.Column;
        Console.WriteLine($"Error:{line}:{col} -> invalid boolean operator: {boolop}");
        Environment.Exit(400);
        return null;
    }


    public static bool HandleUndeclaredVariableError([NotNull] ParserRuleContext context, Dictionary<string, object?> dictionary, string keyId)
    {
        if (dictionary.ContainsKey(keyId))
        {
            return true;
        }
        else
        {
            var line = context.Start.Line;
            var col = context.Start.Column;
            Console.WriteLine($"Error: {line}:{col} -> variable '{keyId}' has not been declared.");
            Environment.Exit(400);
            return false;
        }
    }

    public static object? HandleInvalidScanInputsError([NotNull] ParserRuleContext context, int length, int inputs)
    {
        var line = context.Start.Line;
        var col = context.Start.Column;
        Console.WriteLine($"Error: {line}:{col} -> invalid number of inputs. Expected between 1 and {length}, but got {inputs}.");
        Environment.Exit(400);
        return null;
    }

    public static object? HandleInvalidScanTypeError([NotNull] ParserRuleContext context, string input, Type? type, string location)
    {
        var line = context.Start.Line;
        var col = context.Start.Column;
        Console.WriteLine($"Error: in {location}, in line {line} -> input '{input}' is not in the expected format for data type {type?.Name.ToUpper()}.");
        Environment.Exit(400);
        return null;
    }

    public static object? HandleInvalidOperatorError([NotNull] ParserRuleContext context, object? left, object? right, string op)
    {
        var line = context.Start.Line;
        var col = context.Start.Column;
        Console.WriteLine($"Error: {line}:{col} -> cannot {op} values of types {left?.GetType().Name.ToUpper()} and {right?.GetType().Name.ToUpper()}");
        Environment.Exit(400);
        return null;
    }

    public static object? HandleInvalidRelationOperatorError([NotNull] ParserRuleContext context, object? left, object? right, string op)
    {
        var line = context.Start.Line;
        var col = context.Start.Column;
        Console.WriteLine($"Error: {line}:{col} -> cannot compare values of types {left?.GetType().Name.ToUpper()} and {right?.GetType().Name.ToUpper()} with '{op}' operator");
        Environment.Exit(400);
        return null;
    }

    public static object? HandleInvalidOperatorError([NotNull] ParserRuleContext context, string op, string specifier)
    {
        var line = context.Start.Line;
        var col = context.Start.Column;
        Console.WriteLine($"Error: {line}:{col} -> invalid {specifier} operator: {op}");
        Environment.Exit(400);
        return null;
    }

    public static object? HandleNegationError([NotNull] ParserRuleContext context)
    {
        var line = context.Start.Line;
        var col = context.Start.Column;
        Console.WriteLine($"Error: {line}:{col} -> argument must be of boolean value.");
        Environment.Exit(400);
        return null;
    }

    public static object? HandleUnaryError([NotNull] ParserRuleContext context, string symbol)
    {
        var line = context.Start.Line;
        var col = context.Start.Column;
        Console.WriteLine($"Error: {line}:{col} -> cannot get unary value for symbol {symbol}");
        Environment.Exit(400);
        return null;
    }

    public static object? HandleInvalidEscapeSequenceError([NotNull] ParserRuleContext context, object? sequence)
    {
        var line = context.Start.Line;
        var col = context.Start.Column;
        Console.WriteLine($"Error: {line}:{col} -> invalid escape sequence character: {sequence}");
        Environment.Exit(400);
        return null;
    }

    public static object? HandleInfiniteLoopError([NotNull] ParserRuleContext context)
    {
        var line = context.Start.Line;
        var col = context.Start.Column;
        Console.WriteLine($"Error: {line}:{col} -> infinite loop detected.");
        Environment.Exit(400);
        return null;
    }
}
