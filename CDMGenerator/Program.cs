// See https://aka.ms/new-console-template for more information
using CDMGenerator;

var modelGenerator = new ModelGenerator(p => { Console.WriteLine(p); });
if (args[0].Length == 0) throw new ArgumentException("arg not set");
Console.WriteLine($"Loading {args[0]}");

await modelGenerator.Generate(args[0]);
