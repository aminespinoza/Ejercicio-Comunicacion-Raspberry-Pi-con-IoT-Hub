using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Control_Leds_WPF
{
    public partial class MainWindow : Window
    {
        static ServiceClient serviceClient;
        static string connectionString = "HostName=hubcasatulancingo.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=kfSWlvhPZMFvWPs+oObC3bFAB5rlTUQ800Xo3lHGBoY=";

        static string connectionEventString = "HostName=comunicationhub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=OA5sFyiMLehhnlPmLWs7BTET6S9bMpLAqRWH5zj0Brk=";
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;
        EventHubReceiver eventHubReceiver;
        DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();

            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionEventString, iotHubD2cEndpoint);
            eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver("1", DateTime.UtcNow);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            EventData eventData = await eventHubReceiver.ReceiveAsync();
            string data = Encoding.UTF8.GetString(eventData.GetBytes());
            lstLog.Items.Add(data);
        }

        private void btnLuzRoja_Click(object sender, RoutedEventArgs e)
        {
            EnviarComandoParaDispositivo("1");
        }

        private void btnLuzVerde_Click(object sender, RoutedEventArgs e)
        {
            EnviarComandoParaDispositivo("2");
        }

        private void btnLuzAzul_Click(object sender, RoutedEventArgs e)
        {
            EnviarComandoParaDispositivo("3");
        }

        private void btnApagarLuz_Click(object sender, RoutedEventArgs e)
        {
            EnviarComandoParaDispositivo("4");
        }

        private void btnPolicia_Click(object sender, RoutedEventArgs e)
        {
            EnviarComandoParaDispositivo("5");
        }

        private void btnAlerta_Click(object sender, RoutedEventArgs e)
        {
            EnviarComandoParaDispositivo("6");
        }

        private static async void EnviarComandoParaDispositivo(string mensajeEnviado)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(mensajeEnviado));
            await serviceClient.SendAsync("dispositivo001", commandMessage);
        }
    }
}
