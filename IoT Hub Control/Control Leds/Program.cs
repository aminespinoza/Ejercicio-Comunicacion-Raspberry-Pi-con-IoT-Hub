using Microsoft.Azure.Devices;
using System.Drawing;
using System.Text;
using Console = Colorful.Console;

namespace Control_Leds
{
    class Program
    {
        static ServiceClient serviceClient;
        static string connectionString = "HostName=testingHub2017.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=VNKQRKIGFSA2VA3nP97PBs6fdgIUbbRYwo9ACwQQj5M=";

        static void Main(string[] args)
        {
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

            Console.WriteAscii("IOT HUB CONTROL", Color.White);
            Console.ForegroundColor = Color.WhiteSmoke;
            Console.WriteLine("----------Control de luces----------");
            Console.WriteLine("----------Menú principal------------");
            Console.WriteLine("1 Encender luz roja");
            Console.WriteLine("2 Encender luz verde");
            Console.WriteLine("3 Encender luz azul");
            Console.WriteLine("4 Apagar luces");
            Console.WriteLine("5 Modo policía");
            Console.WriteLine("6 Modo alerta");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Selecciona tu opción");

            string valorRecibido = Console.ReadLine();

            EnviarComandoParaDispositivo(valorRecibido);
            Console.Clear();
            Main(null);
        }

        
        private async static void EnviarComandoParaDispositivo(string mensajeEnviado)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(mensajeEnviado));
            await serviceClient.SendAsync("maquina001", commandMessage);
        }
    }
}
