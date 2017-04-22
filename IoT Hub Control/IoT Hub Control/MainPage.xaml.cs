using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using GHIElectronics.UWP.Shields;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IoT_Hub_Control
{
    public sealed partial class MainPage : Page
    {
        FEZHAT shield;
        private DispatcherTimer timer;
        bool isSendingInformation = false;

        static DeviceClient deviceClient;
        static string iotHubUri = "comunicationhub.azure-devices.net";
        static string deviceKey = "anq2K+XZsT5uQOtWct6whsJPMlErNMZGyT4TMjS4s3o=";
        static string deviceId = "dispositivo001";

        double valorNivelLuz = 0;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            deviceClient = DeviceClient.Create(iotHubUri, AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Http1);
            RecibirInformacionDelHub();

            shield = await FEZHAT.CreateAsync();
            shield.S1.SetLimits(500, 2400, 0, 180);
            shield.S1.Position = 0;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            double x, y, z;
            this.shield.GetAcceleration(out x, out y, out z);

            valorNivelLuz = shield.GetLightLevel();

            txtLuz.Text = "Luminosidad: " + valorNivelLuz.ToString("P2");
            txtTemperatura.Text ="Temperatura: " + shield.GetTemperature().ToString("N2") + "°C";
            txtAcelerometro.Text = "Acelerómetro: x:" 
                + x.ToString("N2") + " y:" 
                + y.ToString("N2") + " z:" 
                + z.ToString("N2");

            if (isSendingInformation)
            {
                EnviarInformaciónAlHub(valorNivelLuz.ToString("N2"), x.ToString(), y.ToString(), z.ToString());
            }
        }

        private void btnInformacion_Click(object sender, RoutedEventArgs e)
        {
            isSendingInformation = true;
        }

        private async void EnviarInformaciónAlHub(string nivelLuz, string ejeX, string ejeY, string ejeZ)
        {
            var telemetryDataPoint = new
            {
                deviceId = deviceId,
                lightLevel = nivelLuz,
                ejeX = ejeX,
                ejeY = ejeY,
                ejeZ = ejeZ,
                date = DateTime.Now
            };

            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            Debug.WriteLine(messageString);
            await deviceClient.SendEventAsync(message);
        }

        private async void RecibirInformacionDelHub()
        {
            while (true)
            {
                var receivedMessage = await deviceClient.ReceiveAsync();

                if (receivedMessage == null) continue;

                if (receivedMessage != null)
                {
                    var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    ProcesarComando(messageData);
                }

                await deviceClient.CompleteAsync(receivedMessage);
            }
        }

        private void ProcesarComando(string mensajeRecibido)
        {
            switch (mensajeRecibido)
            {
                case "1":
                    shield.D2.Color = FEZHAT.Color.Red;
                    shield.D3.Color = FEZHAT.Color.Red;
                    gridRojo.Visibility = Visibility.Visible;
                    gridVerde.Visibility = Visibility.Collapsed;
                    gridAzul.Visibility = Visibility.Collapsed;
                    break;
                case "2":
                    shield.D2.Color = FEZHAT.Color.Green;
                    shield.D3.Color = FEZHAT.Color.Green;
                    gridRojo.Visibility = Visibility.Collapsed;
                    gridVerde.Visibility = Visibility.Visible;
                    gridAzul.Visibility = Visibility.Collapsed;
                    break;
                case "3":
                    shield.D2.Color = FEZHAT.Color.Blue;
                    shield.D3.Color = FEZHAT.Color.Blue;
                    gridRojo.Visibility = Visibility.Collapsed;
                    gridVerde.Visibility = Visibility.Collapsed;
                    gridAzul.Visibility = Visibility.Visible;
                    break;
                case "4":
                    shield.D2.TurnOff();
                    shield.D3.TurnOff();
                    gridRojo.Visibility = Visibility.Collapsed;
                    gridVerde.Visibility = Visibility.Collapsed;
                    gridAzul.Visibility = Visibility.Collapsed;
                    break;
                case "5":
                    ModoPolicia();
                    break;
                case "6":
                    ModoAlerta();
                    break;

                default:
                    
                    break;
            }
        }

        private async void ModoPolicia()
        {
            for (int i = 0; i < 20; i++)
            {
                shield.D2.Color = FEZHAT.Color.Red;
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                shield.D2.TurnOff();
                shield.D3.Color = FEZHAT.Color.Blue;
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                shield.D3.TurnOff();
            }
        }

        private async void ModoAlerta()
        {
            audioAlarm.Play();

            for (int i = 0; i < 8; i++)
            {
                shield.D2.Color = FEZHAT.Color.Cyan;
                shield.D3.Color = FEZHAT.Color.Cyan;
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                shield.D2.TurnOff();
                shield.D3.TurnOff();
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }

        private void sldMotor_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            shield.S1.Position = sldMotor.Value;
        }
    }
}
