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
        private readonly byte[] crc8Table = new byte[]
        {
            0x00, 0x31, 0x62, 0x53, 0xC4, 0xF5, 0xA6, 0x97,
            0xB9, 0x88, 0xDB, 0xEA, 0x7D, 0x4C, 0x1F, 0x2E,
            0x43, 0x72, 0x21, 0x10, 0x87, 0xB6, 0xE5, 0xD4,
            0xFA, 0xCB, 0x98, 0xA9, 0x3E, 0x0F, 0x5C, 0x6D,
            0x86, 0xB7, 0xE4, 0xD5, 0x42, 0x73, 0x20, 0x11,
            0x3F, 0x0E, 0x5D, 0x6C, 0xFB, 0xCA, 0x99, 0xA8,
            0xC5, 0xF4, 0xA7, 0x96, 0x01, 0x30, 0x63, 0x52,
            0x7C, 0x4D, 0x1E, 0x2F, 0xB8, 0x89, 0xDA, 0xEB,
            0x3D, 0x0C, 0x5F, 0x6E, 0xF9, 0xC8, 0x9B, 0xAA,
            0x84, 0xB5, 0xE6, 0xD7, 0x40, 0x71, 0x22, 0x13,
            0x7E, 0x4F, 0x1C, 0x2D, 0xBA, 0x8B, 0xD8, 0xE9,
            0xC7, 0xF6, 0xA5, 0x94, 0x03, 0x32, 0x61, 0x50,
            0xBB, 0x8A, 0xD9, 0xE8, 0x7F, 0x4E, 0x1D, 0x2C,
            0x02, 0x33, 0x60, 0x51, 0xC6, 0xF7, 0xA4, 0x95,
            0xF8, 0xC9, 0x9A, 0xAB, 0x3C, 0x0D, 0x5E, 0x6F,
            0x41, 0x70, 0x23, 0x12, 0x85, 0xB4, 0xE7, 0xD6,
            0x7A, 0x4B, 0x18, 0x29, 0xBE, 0x8F, 0xDC, 0xED,
            0xC3, 0xF2, 0xA1, 0x90, 0x07, 0x36, 0x65, 0x54,
            0x39, 0x08, 0x5B, 0x6A, 0xFD, 0xCC, 0x9F, 0xAE,
            0x80, 0xB1, 0xE2, 0xD3, 0x44, 0x75, 0x26, 0x17,
            0xFC, 0xCD, 0x9E, 0xAF, 0x38, 0x09, 0x5A, 0x6B,
            0x45, 0x74, 0x27, 0x16, 0x81, 0xB0, 0xE3, 0xD2,
            0xBF, 0x8E, 0xDD, 0xEC, 0x7B, 0x4A, 0x19, 0x28,
            0x06, 0x37, 0x64, 0x55, 0xC2, 0xF3, 0xA0, 0x91,
            0x47, 0x76, 0x25, 0x14, 0x83, 0xB2, 0xE1, 0xD0,
            0xFE, 0xCF, 0x9C, 0xAD, 0x3A, 0x0B, 0x58, 0x69,
            0x04, 0x35, 0x66, 0x57, 0xC0, 0xF1, 0xA2, 0x93,
            0xBD, 0x8C, 0xDF, 0xEE, 0x79, 0x48, 0x1B, 0x2A,
            0xC1, 0xF0, 0xA3, 0x92, 0x05, 0x34, 0x67, 0x56,
            0x78, 0x49, 0x1A, 0x2B, 0xBC, 0x8D, 0xDE, 0xEF,
            0x82, 0xB3, 0xE0, 0xD1, 0x46, 0x77, 0x24, 0x15,
            0x3B, 0x0A, 0x59, 0x68, 0xFF, 0xCE, 0x9D, 0xAC
        };

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
            
            for (int i = 0; i < 10; i++)
            {
                string temp_str = i.ToString().Substring(0, 1);

                msg[0] = Convert.ToChar(temp_str);

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

                src_msg_chars[7] = CRC8(src_msg_chars, 7);
                string temp_src_msg = string.Join(",", src_msg_chars.Select(p => p.ToString()).ToArray()); //"60,43,53,..."

                serial_port.Write(src_msg_chars, 0, src_msg_chars.Count());

                //Thread.Sleep(1000);

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

        public byte CRC8(byte[] bytes, int len)
        {
            byte crc = 0xFF;
            for (var i = 0; i < len; i++)
            {
                byte temp = bytes[i];
                byte x = (byte)(crc ^ temp);

                crc = crc8Table[x];
            }
            return crc;
        }
    }
}
