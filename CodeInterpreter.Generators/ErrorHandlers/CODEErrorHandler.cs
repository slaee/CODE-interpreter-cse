namespace CodeInterpreter.Generators.ErrorHandlers;

public class CODEErrorHandler
{
    public static object? ThrowError(int line, int col, string message)
    {
        Console.WriteLine($"Error: {line}:{col} -> {message}");
        Environment.Exit(400);
        return null;
    }
}
