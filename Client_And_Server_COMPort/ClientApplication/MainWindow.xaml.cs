﻿using ArduinoDeamon.Src;
using System;
using System.ComponentModel;
using System.IO;
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
        private const int FRAME_LENGTH = 17;

        private SerialPort serial_port;
        private crc8 sum;
        private Thread thraed;
        private string com_port;

        private void InitComPort(string com_port)
        {
            //Add record 25.05.2019
            if (serial_port != null)
                serial_port.Close();

            if (serial_port != null && serial_port.IsOpen)
            {
                serial_port.Close();
                serial_port = null;
            }

            if (serial_port == null)
            {
                serial_port = new SerialPort(com_port);

                //4800, 9600, 19200, 38400, 57600, 115200
                serial_port.BaudRate = 256000;  
                //serial_port.BaudRate = 9600;

                serial_port.Parity = Parity.None;
                serial_port.DataBits = 8;

                serial_port.StopBits = StopBits.Two;
                serial_port.ReadTimeout = 10000; //-1 200
                serial_port.WriteTimeout = 10000; //-1 50
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

            this.sum = new crc8();
            if (LoadListComPorts() > 0)
            {
                this.com_port = (string)cb_ComPorts.SelectedValue;
                InitComPort(this.com_port);
            }
        }

        /*
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
        */

        private void Window_Closed(object sender, EventArgs e)
        {
            if (serial_port != null && serial_port.IsOpen)
                serial_port.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            msg_list.Items.Clear();
        }

        private void CheckEndChar(char[] msg, byte value)
        {
            msg[0] = (char)value;

            for (int i = 0; i < msg.Length - 1; i++)
            {
                //msg[i] = (char)i; //Convert.ToChar(temp_str);

                if (msg[i] == '\0' || msg[i] == '\r' || msg[i] > 255)
                    msg[i] = (char)1;
            }
        }

        private void DoStart()
        {
            try
            {
                Random rand = new Random();

                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.btn_start.IsEnabled = false;
                    this.btn_clear.IsEnabled = false;
                }));

                char[] msg = new char[FRAME_LENGTH];
                msg[0] = (char)148;
                msg[1] = (char)149;
                msg[2] = (char)150;
                msg[3] = (char)151;
                msg[4] = (char)152;
                msg[5] = (char)153;
                msg[6] = (char)154;
                msg[7] = (char)155;
                msg[8] = (char)0;

                msg[16] = '\r';
            
                for (int i = 0; i < 500000; i++)
                {
                    msg[10] = (char)rand.Next(256);

                    CheckEndChar(msg, (byte)i);

                    byte[] temp = msg.Select(c => (byte)c).ToArray();

                    PortWrite(i, temp);
                }
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

        private void Btn_on_Click(object sender, RoutedEventArgs e)
        {
            thraed = new Thread(DoStart);
            thraed.IsBackground = true;
            thraed.Start();
        }

        private void PortWrite(int index, byte[] src_msg_chars)
        {
            int index_crc8 = src_msg_chars.Length - 2; //2 crars - 'crc8' char and '\r' char

            //Очистим буфер In перед отправкой данных
            if (serial_port.IsOpen)
                serial_port.DiscardInBuffer();

            //Get CRC8
            src_msg_chars[index_crc8] = this.sum.Crc8Bytes(src_msg_chars, index_crc8);
            string temp_src_msg = string.Join(",", src_msg_chars.Select(p => p.ToString()).ToArray());

            //Write
            try
            {
                serial_port.Write(src_msg_chars, 0, src_msg_chars.Count());
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }

            //Read
            char[] result = new char[FRAME_LENGTH];
            try
            {
                ReadBytes(result);
            }
            catch (IOException)
            {
                InitComPort(this.com_port);
            }
            catch (TimeoutException)
            {
                InitComPort(this.com_port);
            }
                
            string temp_res_msg = string.Join(",", result.Select(x => ((byte)x).ToString()).ToArray());

            //Log
            this.Dispatcher.Invoke((Action)(() =>
            {
                string time = DateTime.Now.ToString("dd/MM/yy HH:mm:ss fff");
                //this.msg_list.Items.Add($"{time}: Message '{message}' ({(hash.Trim().Equals(result.Trim()) ? "TRUE" : "FALSE")})");
                this.msg_list.Items.Add($"{time} [{index.ToString("000000000")}] : ==> '{temp_src_msg}' and '{temp_res_msg}'");
                //this.msg_list.SelectedIndex = this.msg_list.Items.Count;
                this.msg_list.ScrollIntoView(this.msg_list.Items[this.msg_list.Items.Count - 1]);
            }));

            //Очистим буфер Out после приема данных
            if (serial_port.IsOpen)
                serial_port.DiscardOutBuffer();            
        }

        private void ReadBytes(char[] bytes)
        {
            int i = 0;
            int x = 0;

            try
            {
                do
                {
                    x = serial_port.ReadByte();
                    bytes[i++] = (char)x;

                } while (x != '\r' || i == bytes.Length - 1);
            }
            catch(IndexOutOfRangeException ex)
            {
                //MessageBox.Show($"{ex.Message} (index={i})");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //thraed.Suspend();
        }
    }
}
