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
//bool chamada = true;

//DADOS OBTIDOS
string umidadeTerra = "";
string situacaoPlanta = "";
string temperaturaAmbiente = "";
string umidadeAmbiente = "";
string pontoOrvalho = "";

//CONFIGURAçÃO DE CONEXÃO
Task PollingErrorFunction(ITelegramBotClient botClient, Exception exception, CancellationToken token)
{
    return Task.CompletedTask;
}

async Task UpdateHandlerFunction(ITelegramBotClient botClient, Update update, CancellationToken token)
{
    if (update.Message is not { } message)
        return;

    if (message.Text is not { } messageText)
        return;

    chatId_ = message.Chat.Id;
    Console.WriteLine($"Servindo a: {message.Chat.Username}");

    if (messageText == "/start")
    {
        await botClient.SendTextMessageAsync(chatId_, "Olá, seja bem vindo ao Bot Arduíno, aqui exibiremos a você\nboletins conforme queira sobre suas plantas 🤩🌱\n\nDigite o comando /incializar para verificar se há um Arduíno conectado a porta serial! 👀");
    }
    else if (message.Text.ToLower().Replace(" ", "").Contains("setar-"))
    {
        if (conectado)
        {
            // POSSO INICIALIZAR O BOT AQUI
            delay_minutos = int.Parse(Regex.Replace(message.Text, "[^0-9]", ""));
            if (delay_minutos != 0)
            {
                await botClient.SendTextMessageAsync(chatId_, $"Intervalo de {delay_minutos} minuto(s) definido com sucesso!");
                await botClient.SendTextMessageAsync(chatId_, "𝗕𝗢𝗧 𝗜𝗡𝗜𝗖𝗜𝗔𝗟𝗜𝗭𝗔𝗗𝗢 𝗖𝗢𝗠 𝗦𝗨𝗖𝗘𝗦𝗦𝗢\n\nDigite /encerrar caso deseje interromper os diagnósticos");
                await botClient.SendTextMessageAsync(chatId_, "𝗥𝗲𝗮𝗹𝗶𝘇𝗮𝗻𝗱𝗼 𝗰𝗵𝗮𝗺𝗮𝗱𝗮 𝗱𝗲 𝗶𝗻𝗶𝗰𝗶𝗮𝗹𝗶𝘇𝗮𝗰𝗮𝗼...");//Caso for usar o modo de chamada, tirar os dois últimos aqui
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
    }
    else if (messageText == "/inicializar")
    {
        if (SerialPort.GetPortNames().Contains("COM3"))
        {
            await botClient.SendTextMessageAsync(chatId_, "Porta COM3 conectada com sucesso!");
            await botClient.SendTextMessageAsync(chatId_, "Para definir um intervalo entre os boletins, digite setar-[número em minutos desejados], verificando se não há nenhum espaço na frase!");
            conectado = true;
            //chamada = true; //PODE INICIAR AQUI
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId_, "Nenhuma porta conectada, tente novamente!");
            conectado = false;
        }
        startado = false;
    }
    else if (messageText == "/encerrar")
    {
        conectado = false;
        startado = false; //caso for usar o modo de chamada, tirar esse aqui e deixar somente o conectado = false
        await botClient.SendTextMessageAsync(chatId_, "O diagnóstico foi encerrado!");

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

    while (conectado && startado)
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
                {
                    Console.WriteLine("Umidade Terra: " + receivedData.Replace("Umidade Terra: ", ""));
                    umidadeTerra = receivedData.Replace("Umidade Terra: ", "");
                }
                else if (receivedData.StartsWith("Temperatura ambiente: "))
                {
                    Console.WriteLine("Temperatura ambiente: " + receivedData.Replace("Temperatura ambiente: ", ""));
                    temperaturaAmbiente = receivedData.Replace("Temperatura ambiente: ", "");
                }
                else if (receivedData.StartsWith("Umidade ambiente: "))
                {
                    Console.WriteLine("Umidade ambiente: " + receivedData.Replace("Umidade ambiente: ", ""));
                    umidadeAmbiente = receivedData.Replace("Umidade ambiente: ", "");
                }
                else if (receivedData.StartsWith("Ponto de Orvalho: "))
                {
                    Console.WriteLine("Ponto de Orvalho: " + receivedData.Replace("Ponto de Orvalho: ", ""));
                    pontoOrvalho = receivedData.Replace("Ponto de Orvalho: ", "");
                }
            };

        }
        catch (Exception ex)
        {

        }

        Thread.Sleep(10100);
        if (Convert.ToInt32(umidadeTerra) >= 75)
            situacaoPlanta = "𝘼 𝙥𝙡𝙖𝙣𝙩𝙖 𝙚𝙨𝙩𝙖́ 𝙗𝙚𝙢 𝙝𝙞𝙙𝙧𝙖𝙩𝙖𝙙𝙖 🤩";
        else if (Convert.ToInt32(umidadeTerra) >= 25)
            situacaoPlanta = "𝘼 𝙥𝙡𝙖𝙣𝙩𝙖 𝙩𝙚𝙢 𝙪𝙢𝙞𝙙𝙖𝙙𝙚 𝙢𝙤𝙙𝙚𝙧𝙖𝙙𝙖 🌷";
        else
            situacaoPlanta = "𝘼 𝙥𝙡𝙖𝙣𝙩𝙖 𝙣𝙚𝙘𝙚𝙨𝙨𝙞𝙩𝙖 𝙨𝙚𝙧 𝙧𝙚𝙜𝙖𝙙𝙖 🥀";

        await botClient.SendTextMessageAsync(chatId_, $"\U0001F4E2 -- 𝗕𝗢𝗟𝗘𝗧𝗜𝗠 {DateTime.Now:HH:mm}\n\n" +
                                                      $"💧 Umidade da Terra: {umidadeTerra}%\n\n" +
                                                      $"🔰 -- {situacaoPlanta} \n\n" +
                                                      $"🌡 Temperatura Ambiente: {temperaturaAmbiente}°C\n" +
                                                      $"☁ Umidade Ambiente: {umidadeAmbiente}%\n" +
                                                      $"🍃 Ponto de Orvalho: {pontoOrvalho}°C\n\n" +
                                                      $"\n𝗢 𝗛𝗜𝗦𝗧𝗢́𝗥𝗜𝗖𝗢 𝗗𝗔 𝗣𝗟𝗔𝗡𝗧𝗔 𝗦𝗘 𝗘𝗡𝗖𝗢𝗡𝗧𝗥𝗔 𝗔𝗧𝗜𝗩𝗢");

        Thread.Sleep((60000 * delay_minutos)-10200
            );

        Console.WriteLine("----------------------------");
    }
}
