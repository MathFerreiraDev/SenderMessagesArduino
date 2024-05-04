// See https://aka.ms/new-console-template for more information
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Insira o número o qual receberá as chamadas:");
Console.ForegroundColor = ConsoleColor.Yellow;
string number = Console.ReadLine();
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Qual o intervalo (por horas) em que deseja receber atualizações?:");
Console.ForegroundColor = ConsoleColor.Yellow;
double hours = double.Parse(Console.ReadLine());

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("---------------------------------------------------");



    try
    {
        System.Diagnostics.Process.Start("http://api.whatsapp.com/send?phone=+" + number + "&text=" + "Teste de mensagem!");
        Thread.Sleep(30000);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Mensagem enviada com sucesso para "+number);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("ERROR: " + ex.Message);
    }


//Console.ReadKey();
