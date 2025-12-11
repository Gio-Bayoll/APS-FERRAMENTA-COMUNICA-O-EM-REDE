using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatCliente
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient cliente;
        NetworkStream stream;
        Thread threadReceber;
        bool conectado = false;
        bool darkMode = false;

        private void btnConectar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cliente = new TcpClient();
                cliente.Connect(txtIP.Text, 5000);

                stream = cliente.GetStream();
                conectado = true;

                txtChat.AppendText("Conectado ao servidor!\n");

                // Iniciar thread para receber mensagens
                threadReceber = new Thread(ReceberMensagens);
                threadReceber.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao conectar: " + ex.Message);
            }
        }

        private void btnTema_Click(object sender, RoutedEventArgs e)
        {
            if (darkMode)
            {
                // Carrega tema CLARO
                var light = new ResourceDictionary();
                light.Source = new Uri("Tema/TemaClaro.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(light);

                btnTema.Content = "Dark Mode";
                darkMode = false;
            }
            else
            {
                // Carrega tema ESCURO
                var dark = new ResourceDictionary();
                dark.Source = new Uri("Tema/TemaEscuro.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(dark);

                btnTema.Content = "Light Mode";
                darkMode = true;
            }
        }


        private void ReceberMensagens()
        {
            try
            {
                byte[] buffer = new byte[1024];

                while (conectado)
                {
                    int bytesLidos = stream.Read(buffer, 0, buffer.Length);
                    if (bytesLidos <= 0) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, bytesLidos);

                    // Atualizar UI
                    Dispatcher.Invoke(() =>
                    {
                        txtChat.AppendText(msg + "\n");
                        txtChat.ScrollToEnd();
                    });
                }
            }
            catch
            {
                // Ignora erros de cancelamento quando o app fecha
            }
        }


        private void btnEnviar_Click(object sender, RoutedEventArgs e)
        {
            if (!conectado) return;

            string hora = DateTime.Now.ToString("HH:mm");
            string texto = $"[{hora}] {txtNome.Text}: {txtMensagem.Text}";


            byte[] dados = Encoding.UTF8.GetBytes(texto);
            stream.Write(dados, 0, dados.Length);

            txtMensagem.Clear(); // limpa campo de digitação
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            conectado = false;
            try
            {
                stream?.Close();
                cliente?.Close();
            }
            catch { }

            base.OnClosing(e);
        }


        public MainWindow()
        {
            InitializeComponent();
        }
    }
}