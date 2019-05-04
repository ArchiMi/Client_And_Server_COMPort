using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ThreadState = System.Threading.ThreadState;

namespace ArduinoDeamon
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly byte[] crc8Table = new byte[]
        {
            0x00, 0x5E, 0xBC, 0xE2, 0x61, 0x3F, 0xDD, 0x83,
            0xC2, 0x9C, 0x7E, 0x20, 0xA3, 0xFD, 0x1F, 0x41,
            0x9D, 0xC3, 0x21, 0x7F, 0xFC, 0xA2, 0x40, 0x1E,
            0x5F, 0x01, 0xE3, 0xBD, 0x3E, 0x60, 0x82, 0xDC,
            0x23, 0x7D, 0x9F, 0xC1, 0x42, 0x1C, 0xFE, 0xA0,
            0xE1, 0xBF, 0x5D, 0x03, 0x80, 0xDE, 0x3C, 0x62,
            0xBE, 0xE0, 0x02, 0x5C, 0xDF, 0x81, 0x63, 0x3D,
            0x7C, 0x22, 0xC0, 0x9E, 0x1D, 0x43, 0xA1, 0xFF,
            0x46, 0x18, 0xFA, 0xA4, 0x27, 0x79, 0x9B, 0xC5,
            0x84, 0xDA, 0x38, 0x66, 0xE5, 0xBB, 0x59, 0x07,
            0xDB, 0x85, 0x67, 0x39, 0xBA, 0xE4, 0x06, 0x58,
            0x19, 0x47, 0xA5, 0xFB, 0x78, 0x26, 0xC4, 0x9A,
            0x65, 0x3B, 0xD9, 0x87, 0x04, 0x5A, 0xB8, 0xE6,
            0xA7, 0xF9, 0x1B, 0x45, 0xC6, 0x98, 0x7A, 0x24,
            0xF8, 0xA6, 0x44, 0x1A, 0x99, 0xC7, 0x25, 0x7B,
            0x3A, 0x64, 0x86, 0xD8, 0x5B, 0x05, 0xE7, 0xB9,
            0x8C, 0xD2, 0x30, 0x6E, 0xED, 0xB3, 0x51, 0x0F,
            0x4E, 0x10, 0xF2, 0xAC, 0x2F, 0x71, 0x93, 0xCD,
            0x11, 0x4F, 0xAD, 0xF3, 0x70, 0x2E, 0xCC, 0x92,
            0xD3, 0x8D, 0x6F, 0x31, 0xB2, 0xEC, 0x0E, 0x50,
            0xAF, 0xF1, 0x13, 0x4D, 0xCE, 0x90, 0x72, 0x2C,
            0x6D, 0x33, 0xD1, 0x8F, 0x0C, 0x52, 0xB0, 0xEE,
            0x32, 0x6C, 0x8E, 0xD0, 0x53, 0x0D, 0xEF, 0xB1,
            0xF0, 0xAE, 0x4C, 0x12, 0x91, 0xCF, 0x2D, 0x73,
            0xCA, 0x94, 0x76, 0x28, 0xAB, 0xF5, 0x17, 0x49,
            0x08, 0x56, 0xB4, 0xEA, 0x69, 0x37, 0xD5, 0x8B,
            0x57, 0x09, 0xEB, 0xB5, 0x36, 0x68, 0x8A, 0xD4,
            0x95, 0xCB, 0x29, 0x77, 0xF4, 0xAA, 0x48, 0x16,
            0xE9, 0xB7, 0x55, 0x0B, 0x88, 0xD6, 0x34, 0x6A,
            0x2B, 0x75, 0x97, 0xC9, 0x4A, 0x14, 0xF6, 0xA8,
            0x74, 0x2A, 0xC8, 0x96, 0x15, 0x4B, 0xA9, 0xF7,
            0xB6, 0xE8, 0x0A, 0x54, 0xD7, 0x89, 0x6B, 0x35
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
                serial_port.ReadTimeout = -1; //-1 200
                serial_port.WriteTimeout = -1; //-1 50
                serial_port.Handshake = Handshake.None;
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
            msg[0] = 'D';
            msg[1] = 'A';
            msg[2] = 'T';
            msg[3] = 'A';
            msg[4] = '0';
            msg[5] = '0';
            msg[6] = '0';
            msg[7] = '0';
            msg[8] = '\r';
            
            for (int i = 0; i < 150; i++)
            {
                string temp_str = i.ToString().Substring(0, 1);

                msg[4] = Convert.ToChar(temp_str);

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

                string result = serial_port.ReadTo("\r");
                //string result = serial_port.ReadLine();
                result = result.Trim();
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








        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //thraed.Suspend();
        }

        public byte CRC8(byte[] bytes, int len)
        {
            byte crc = 0;
            for (var i = 0; i < len; i++)
            {
                byte temp = bytes[i];
                int temp_index = crc ^ temp;
                crc = crc8Table[temp_index];
            }
            return crc;
        }
    }
}
