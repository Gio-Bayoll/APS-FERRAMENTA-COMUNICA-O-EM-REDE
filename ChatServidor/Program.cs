using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

class Program
{
    static TcpListener servidor = null;
    static List<TcpClient> clientes = new List<TcpClient>();


    static void AtenderCliente(TcpClient cliente)
    {
        Console.WriteLine("Thread iniciada para um cliente.");

        NetworkStream stream = cliente.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (true)
            {
                int bytesLidos = stream.Read(buffer, 0, buffer.Length);

                if (bytesLidos == 0)
                {
                    Console.WriteLine("Cliente desconectado.");
                    clientes.Remove(cliente);
                    break;
                }

                string msg = Encoding.UTF8.GetString(buffer, 0, bytesLidos);
                Console.WriteLine("Mensagem recebida: " + msg);

                EnviarParaTodos(msg, cliente);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro na comunicação com cliente: " + ex.Message);
        }
    }


    static void EnviarParaTodos(string mensagem, TcpClient remetente)
    {
        byte[] dados = Encoding.UTF8.GetBytes(mensagem);

        foreach (TcpClient cli in clientes)
        {
            if (cli != remetente)
            {
                try
                {
                    cli.GetStream().Write(dados, 0, dados.Length);
                    remetente.GetStream().Write(dados, 0, dados.Length);
                }
                catch
                {
                    // cliente caiu, ignorar
                }
            }
        }
    }


    static void Main()
    {
        try
        {
            Console.WriteLine("Iniciando servidor...");

            servidor = new TcpListener(IPAddress.Any, 5000);
            servidor.Start();

            Console.WriteLine("Servidor iniciado na porta 5000.");
            Console.WriteLine("Aguardando conexões...");

            while (true)
            {
                Console.WriteLine("Aguardando um cliente se conectar...");

                TcpClient cliente = servidor.AcceptTcpClient();
                Console.WriteLine("Cliente conectado!");

                clientes.Add(cliente);

                Thread t = new Thread(() => AtenderCliente(cliente));
                t.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro: " + ex.Message);
        }
    }
}
