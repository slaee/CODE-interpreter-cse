
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using CodeInterpreter.Generators.Content;
using CodeInterpreter.Generators.ErrorHandlers;
using System.Text.RegularExpressions;

namespace CodeInterpreter.Generators.CodeConstants;

public class CodeConstant
{
    public static object? VariableDeclarations(Dictionary<string, object?> variables, string varName, CODEParser.Variable_declarationContext context)
    {
        if (variables != null && variables.TryGetValue(varName, out object? value))
        {
            return value;
        }
        else
        {
            ErrorHandler.HandleUndefinedVariableError(context, varName);
            return null;
        }
    }

    public static object? Data(string constant, CODEParser.ConstantContext context)
    {
        if (constant.StartsWith("\"") && constant.EndsWith("\""))
        {
            return constant.Substring(1, constant.Length - 2);
        }
        else if (constant.StartsWith("'") && constant.EndsWith("'"))
        {
            return constant[1];
        }
        else if (context.BOOL() != null)
        {
            return bool.Parse(context.BOOL().GetText());
        }
        else if (context.INT() != null)
        {
            return int.Parse(context.INT().GetText());
        }
        else if (context.FLOAT() != null)
        {
            return float.Parse(context.FLOAT().GetText());
        }
        else if (context.STRING() != null)
        {
            string text = context.STRING().GetText();
            text = text.Substring(1, text.Length - 2);
            text = Regex.Replace(text, @"\\(.)", "$1");
            return text;
        }
        else if (context.CHAR() != null)
        {
            return context.CHAR().GetText()[1];
        }
        else
        {
            throw new InvalidOperationException("Unknown literal type");
        }
    }

    public static object? Type(CODEParser.TypeContext context)
    {
        switch (context.GetText())
        {
            case "INT":
                return typeof(int);
            case "FLOAT":
                return typeof(float);
            case "BOOL":
                return typeof(bool);
            case "CHAR":
                return typeof(char);
            case "STRING":
                return typeof(string);
            default:
                throw new NotImplementedException("Error: Invalid Data Type");
        }
    }

    public static object? BuiltinDisplay(object? expression)
    {
        if (expression is bool b)
            expression = b.ToString().ToUpper();

        Console.Write(expression);

        return null;

    }

    public static object? BuiltinScan([NotNull] ParserRuleContext context, Dictionary<string, object?> typeDictionary, Dictionary<string, object?> valueDictionary, string id, string input)
    {
        if (!typeDictionary.ContainsKey(id))
        {
            return ErrorHandler.HandleUndeclaredVariableError(context, typeDictionary, id);
        }

        Type? valueType = (Type?)typeDictionary[id];
        try
        {
            object? convertedValue = Convert.ChangeType(input, valueType!);
            return valueDictionary[id] = convertedValue;
        }
        catch (FormatException)
        {
            return ErrorHandler.HandleInvalidScanTypeError(context, input, valueType, "Input Scan");
            //throw new ArgumentException($"Input '{input}' is not in the expected format for data type {valueType}.");
        }
    }

    public static object? Append(object? left, object? right)
    {
        if (left is bool b)
            left = b.ToString().ToUpper();

        if (right is bool c)
            right = c.ToString().ToUpper();

        return $"{left}{right}";
    }

    public static object? Identifier(Dictionary<string, object?> dictionary, string identifier, CODEParser.IdentifierExpressionContext context)
    {
        if (dictionary.ContainsKey(identifier))
        {
            return dictionary[identifier];
        }
        else
        {
            ErrorHandler.HandleUndefinedVariableError(context, identifier);
            return null;
        }
    }

    public static object? ConstantExpressionParser(CODEParser.ConstantExpressionContext context)
    {
        if (context.constant().INT() is { } i)
            return int.Parse(i.GetText());
        else if (context.constant().FLOAT() is { } f)
            return float.Parse(f.GetText());
        else if (context.constant().CHAR() is { } g)
            return g.GetText()[1];
        else if (context.constant().BOOL() is { } b)
            return b.GetText().Equals("\"TRUE\"");
        else if (context.constant().STRING() is { } s)
            return s.GetText()[1..^1];
        throw new NotImplementedException();
    }

    public static object? Scan([NotNull] ParserRuleContext context, Dictionary<string, object?> typeDictionary, Dictionary<string, object?> valueDictionary, string id, string input)
    {
        if (!typeDictionary.ContainsKey(id))
        {
            return ErrorHandler.HandleUndeclaredVariableError(context, typeDictionary, id);
        }

        Type? valueType = (Type?)typeDictionary[id];
        try
        {
            object? convertedValue = Convert.ChangeType(input, valueType!);
            return valueDictionary[id] = convertedValue;
        }
        catch (FormatException)
        {
            return ErrorHandler.HandleInvalidScanTypeError(context, input, valueType, "Input Scan");
            //throw new ArgumentException($"Input '{input}' is not in the expected format for data type {valueType}.");
        }
    }
}
