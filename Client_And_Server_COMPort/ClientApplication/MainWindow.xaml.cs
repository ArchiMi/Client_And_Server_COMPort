using ArduinoDeamon.Src;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;

namespace ArduinoDeamon
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort serial_port;
        private Thread thraed;

        private void InitComPort(string com_port)
        {
            if (serial_port == null)
            {
                serial_port = new SerialPort(com_port);
                serial_port.BaudRate = 256000; //50, 75, 110, 150, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 
                serial_port.Parity = Parity.None;
                serial_port.DataBits = 8;

                serial_port.StopBits = StopBits.Two;
                serial_port.ReadTimeout = 1500; //-1 200
                serial_port.WriteTimeout = 1500; //-1 50
                serial_port.Handshake = Handshake.None;
                serial_port.Encoding = Encoding.Default;
                //serial_port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

                serial_port.Open();
            }
        }

        public int LoadListComPorts()
        {
            string[] enableComPorts = SerialPort.GetPortNames();

            foreach (string port in enableComPorts)
                cb_ComPorts.Items.Add(port);

            if (cb_ComPorts.Items.Count > 0)
                cb_ComPorts.SelectedIndex = 0;

            return cb_ComPorts.Items.Count;
        }

        public MainWindow()
        {
            InitializeComponent();

            if (LoadListComPorts() > 0)
                InitComPort((string)cb_ComPorts.SelectedValue);            
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                string message = sp.ReadLine();

                Dispatcher.BeginInvoke((Action)delegate
                {
                    this.msg_list.Items.Add($"<= Receive: {message}");
                });
            }
            catch (TimeoutException)
            {

            }
        }

        string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sb = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
                sb.Append(data[i].ToString("x2"));

            // Return the hexadecimal string.
            return sb.ToString();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (serial_port != null && serial_port.IsOpen)
                serial_port.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            msg_list.Items.Clear();
        }

        private void DoStart()
        {
            char[] msg = new char[9];
            msg[0] = (char)148;
            msg[1] = (char)149;
            msg[2] = (char)150;
            msg[3] = (char)151;
            msg[4] = (char)152;
            msg[5] = (char)153;
            msg[6] = (char)154;
            msg[7] = (char)155;
            msg[8] = '\r';
            
            for (int i = 0; i < 900000; i++)
            {
                msg[0] = (char)i; //Convert.ToChar(temp_str);
                if (msg[0] == 0 || msg[0] == 13)
                    msg[0] = (char)1;

                byte[] temp = msg.Select(c => (byte)c).ToArray();

                PortWrite(temp);
            }
        }

        private void Btn_on_Click(object sender, RoutedEventArgs e)
        {
            thraed = new Thread(DoStart);
            thraed.IsBackground = true;
            thraed.Start();
        }

        private void PortWrite(byte[] src_msg_chars)
        {
            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.btn_start.IsEnabled = false;
                    this.btn_clear.IsEnabled = false;
                }));

                //Очистим буфер In перед отправкой данных
                if (serial_port.IsOpen)
                    serial_port.DiscardInBuffer();

                crc8 sum = new crc8();
                src_msg_chars[7] = sum.Crc8Bytes(src_msg_chars, 7);
                string temp_src_msg = string.Join(",", src_msg_chars.Select(p => p.ToString()).ToArray()); //"60,43,53,..."

                serial_port.Write(src_msg_chars, 0, src_msg_chars.Count());

                //string result = serial_port.ReadTo("\r");
                //string result = serial_port.ReadLine();
                
                char[] result = new char[9];
                ReadBytes(result);
                //serial_port.Read(result, 0, 8);

                
                //result = result.Trim();
                string temp_res_msg = string.Join(",", result.Select(x => ((byte)x).ToString()).ToArray());

                //Log
                this.Dispatcher.Invoke((Action)(() =>
                {
                    string time = DateTime.Now.ToString("MM/dd/yy H:mm:ss fff");
                    //this.msg_list.Items.Add($"{time}: Message '{message}' ({(hash.Trim().Equals(result.Trim()) ? "TRUE" : "FALSE")})");
                    this.msg_list.Items.Add($"{time}: ==> '{temp_src_msg}' and '{temp_res_msg}'");
                }));

                //Очистим буфер Out после приема данных
                if (serial_port.IsOpen)
                    serial_port.DiscardOutBuffer();
            }
            finally
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.btn_start.IsEnabled = true;
                    this.btn_clear.IsEnabled = true;
                }));
            }
        }

        private void ReadBytes(char[] bytes)
        {
            int i = 0;
            int x = 0;

            do
            {
                x = serial_port.ReadByte();
                bytes[i++] = (char)x;

            } while (x != '\r');
        }






        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //thraed.Suspend();
        }
    }
}
