using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace CodeInterpreter.Generators.ErrorHandlers;

public class CODEErrorListener : BaseErrorListener
{
    public override void SyntaxError
        ([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol,
        int line, int col, [NotNull] string msg,
        [Nullable] RecognitionException e)
    {
        Console.Error.WriteLine($"Syntax error: {line}:{col} Unexpected token {offendingSymbol.Text.Replace("\r\n", "NEWLINE")}");
        Console.Error.WriteLine($"INFO: {msg[0].ToString().ToUpper() + msg[1..].Replace("\\r\\n", $"col: {col}")}");
        Environment.Exit(400);
        base.SyntaxError(recognizer, offendingSymbol, line, col, msg, e);
    }
}
