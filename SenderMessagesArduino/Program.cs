// See https://aka.ms/new-console-template for more information




using System;
using System.IO.Ports;
using Telegram.Bot;
using Twilio;
using Twilio.Rest.Api;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Telegram.Bot.Args;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Text.RegularExpressions;




// -- OBTER OUTPUT -> ARDUINO
/*SerialPort serialPort = new SerialPort("COM4", 9600);
double umidade = 0;
string situação = String.Empty;

try
{
    serialPort.Open();
    bool haveoutput = false;
    

    while (!haveoutput)
    {
         umidade = double.Parse(serialPort.ReadLine());

        if (umidade >= 75)
            situação = "DE ACORDO";
        else if (umidade >= 45)
            situação = "MEDIANA";
        else
            situação = "BAIXA";


        if (!string.IsNullOrEmpty(Convert.ToString(umidade)))
        {
            haveoutput = true;
        }
    }

    if (haveoutput)
        Console.WriteLine(umidade);

}
catch (Exception ex)
{
    Console.WriteLine($"Ocorreu um erro ao obter o resposta do Arduino: {ex.Message}");
}*/


// -- INTREGRAÇÃO AO TELEGRAM




var botClient = new TelegramBotClient("6504753779:AAEhrCoOeb8krA5aA77tZiXa3lXS-9gGgAw");

long chatId_ = 000000000;
int delay_minutos = 0;
bool conectado = false;
bool startado = false;
bool chamada = true;

//CONFIGURAçÃO DE CONEXÃO
Task PollingErrorFunction(ITelegramBotClient botClient, Exception exception, CancellationToken token)
{
    return Task.CompletedTask;
}

async Task UpdateHandlerFunction(ITelegramBotClient botClient, Update update, CancellationToken token)
{
    if(update.Message is not { } message)
        return;

    if (message.Text is not { } messageText)
        return;

    chatId_ = message.Chat.Id;
    Console.WriteLine($"Servindo a: {message.Chat.Username}");

    if (messageText == "/start")
    {
        await botClient.SendTextMessageAsync(chatId_, "Olá, seja bem vindo ao Bot Arduíno, aqui exibiremos a você \n boletins conforme queira sobre suas plantas 🤩🌱");
    } else if (message.Text.Contains("setar-"))
    {
        if (conectado)
        {
            // POSSO INICIALIZAR O BOT AQUI
            delay_minutos = int.Parse(Regex.Replace(message.Text, "[^0-9]", ""));
            if (delay_minutos != 0)
            {
                await botClient.SendTextMessageAsync(chatId_, $"Intervalo de {delay_minutos} minuto(s) definido com sucesso!");
                await botClient.SendTextMessageAsync(chatId_, "**BOT INICIALIZADO COM SUCESSO**"); //Caso for usar o modo de chamada, tirar esse aqui
                startado = true;
                //chamada = true; //PODE INICIAR AQUI
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId_, "O seguinte valor não é tolerado, inválido.");
                startado = false;
            }

        }
        else
        {
            await botClient.SendTextMessageAsync(chatId_, "Verifique a conexão com a porta antes de definir um intervalo!");
        }
    } else if (messageText == "/inicializar")
    {
        if (SerialPort.GetPortNames().Contains("COM3"))
        {
            await botClient.SendTextMessageAsync(chatId_, "Porta COM3 conectada com sucesso!");
            conectado = true;
            //chamada = true; //PODE INICIAR AQUI
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId_, "Nenhuma porta conectada, tente novamente!");
            conectado = false;
        }
    } else if (messageText == "/encerrar")
    {
        conectado = false;
        startado = false; //caso for usar o modo de chamada, tirar esse aqui e deixar somente o conectado = false
        await botClient.SendTextMessageAsync(chatId_, "O diagnóstico foi pausado");
        
    }




    //SOLICITAÇÃO
    /*if(conectado && startado && chamada)
    {
        await botClient.SendTextMessageAsync(chatId_, "**BOT INICIALIZADO COM SUCESSO**");
        chamada = false;
    }*/
}

var cancelToken = new CancellationTokenSource();
botClient.StartReceiving(
    updateHandler: UpdateHandlerFunction,
    pollingErrorHandler: PollingErrorFunction,
    receiverOptions: new ReceiverOptions
    {
        AllowedUpdates = Array.Empty<UpdateType>()
    },
    cancellationToken: cancelToken.Token
 );

var me = await botClient.GetMeAsync(cancelToken.Token);



Console.WriteLine($"Escutando: {me.Username}");

while (true)
{

    while(conectado && startado)
    {
        SerialPort serialPort = new SerialPort("COM3", 9600);
        try
        {
            serialPort.Open();

            serialPort.NewLine = "\n";

            serialPort.DataReceived += (sender, e) =>
            {
                string receivedData = serialPort.ReadLine();

                if (receivedData.StartsWith("Umidade Terra: "))
                    Console.WriteLine("Umidade Terra: " + receivedData.Replace("Umidade Terra: ", ""));

                else if (receivedData.StartsWith("Temperatura ambiente: "))
                    Console.WriteLine("Temperatura ambiente: " + receivedData.Replace("Temperatura ambiente: ", ""));
                
                else if (receivedData.StartsWith("Umidade ambiente: "))
                    Console.WriteLine("Umidade ambiente: " + receivedData.Replace("Umidade ambiente: ", ""));

                else if (receivedData.StartsWith("Ponto de Orvalho: "))
                    Console.WriteLine("Ponto de Orvalho: " + receivedData.Replace("Ponto de Orvalho: ", ""));
            };

        }
        catch (Exception ex)
        {
          
        }

            await botClient.SendTextMessageAsync(chatId_, $"Diagonóstico");
            Thread.Sleep(1000 * delay_minutos); // 60000 - 1 minuto
    }
}
