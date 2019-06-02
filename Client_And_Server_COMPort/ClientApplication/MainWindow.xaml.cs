using ClientAppNameSpace.Src;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;

namespace ClientAppNameSpace
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    { 
        private Thread thread;
        private DataTable table;
        private bool is_started = false;

        public MainWindow()
        {
            InitializeComponent();

            this.msg_list.DataContext = CreateDataTable();

            //this.sum = new CRC8();
            if (LoadListComPorts() > 0) { }
                //this.com_port_data = (string)cb_ComPorts.SelectedValue;


        }

        DataTable CreateDataTable()
        {
            this.table = new DataTable("Customers");

            this.table.Columns.Add("Index", typeof(int));
            this.table.Columns.Add("DateTimeOperation", typeof(DateTime));
            this.table.Columns.Add("CMD", typeof(string));
            this.table.Columns.Add("Status", typeof(string));
            this.table.Columns.Add("IsError", typeof(bool));
            this.table.Columns.Add("Message", typeof(string));
            this.table.Columns.Add("Balance", typeof(decimal));

            return table;
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

        private void Button_Click_clear(object sender, RoutedEventArgs e)
        {
            this.msg_list.Items.Clear();
        }

        private void DoStart()
        {
            TempClass tc_item = null;

            try
            {   
                this.Dispatcher.Invoke((Action)(() =>
                {
                    tc_item = new TempClass((string)cb_ComPorts.SelectedValue, 256000);
                }));

                tc_item.InitComPort();
                
                //TEST
                Random rand = new Random();

                char[] msg = new char[Const.FRAME_LENGTH];
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
            
                for (int i = 0; i < 10000000; i++)
                {
                    // Временное произвольное значение в качестве одного из параметров, для проверки CRC8
                    msg[10] = (char)rand.Next(256);

                    tc_item.CheckEndChar(msg, (byte)i);

                    byte[] temp = msg.Select(c => (byte)c).ToArray();

                    string temp_transmit_msg = tc_item.WriteBytes(i, temp);
                    string temp_res_msg = tc_item.ReadBytes();

                    // Log
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        string time = DateTime.Now.ToString("dd/MM/yy HH:mm:ss fff");
                        //string message = $"[{index.ToString("000000000")}] : ==> '{temp_src_msg}' and '{temp_res_msg}'";
                        string message = $" => '{temp_transmit_msg}' and '{temp_res_msg}'";

                        char transmit_crc8_char = temp_transmit_msg[temp_transmit_msg.Length - 2];
                        char receiv_crc8_char = temp_res_msg[temp_res_msg.Length - 2];
                        bool is_error = transmit_crc8_char == receiv_crc8_char;

                        this.msg_list.Items.Add(new ItemRecord(i, DateTime.Now, "1", message, "ping", !is_error));
                        this.msg_list.ScrollIntoView(this.msg_list.Items[this.msg_list.Items.Count - 1]);
                    }));
                }
            }
            finally
            {
                tc_item.Dispose();

                this.Dispatcher.Invoke((Action)(() =>
                {
                    //this.btn_start.IsEnabled = true;
                    this.btn_clear.IsEnabled = true;
                }));
            }
        }

        private void Btn_on_Click(object sender, RoutedEventArgs e)
        {
            if (!this.is_started)
            {
                //Task.Factory.StartNew(() => { Foo(token.Token); });

                thread = new Thread(DoStart);
                thread.IsBackground = true;
                thread.Start();

                this.is_started = true;
                this.btn_start.Content = "Stop task";

                this.btn_clear.IsEnabled = false;
            }
            else
            {
                thread.Abort();
                thread.Join(250);

                this.is_started = false;
                this.btn_start.Content = "Start task";

                this.btn_clear.IsEnabled = true;
            }
        }
    }
}
