using Antlr4.Runtime;
using CodeInterpreter.Generators;
using CodeInterpreter.Generators.Content;
using CodeInterpreter.Generators.ErrorHandlers;

bool prompt = true;


while (prompt)
{
    var filePath = Path.Combine(AppContext.BaseDirectory, "Tests", "test.code");
    var fileContent = File.ReadAllText(filePath);

    var inputStream = new AntlrInputStream(fileContent);
    var lexer = new CODELexer(inputStream);
    var commonTokenStream = new CommonTokenStream(lexer);
    var parser = new CODEParser(commonTokenStream);

    var codeErrorListener = new CODEErrorListener();
    parser.AddErrorListener(codeErrorListener);

    var context = parser.program();
    var codeVisitor = new CodeVisitor();
    codeVisitor.Visit(context);

    Console.WriteLine("\n");
    Console.Write("Do you wish to continue?(y/N): ");
    var res = Console.ReadLine()?[0];

    prompt = (res is null || res == 'Y' || res == 'y') ? true : false;
}
