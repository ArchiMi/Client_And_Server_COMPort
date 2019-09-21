using ClientAppNameSpace.Src;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Linq;
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

                for (int i = 0; i < 1000000; i++)
                {
                    /*
                    char[] msg = new char[Const.FRAME_LENGTH];
                    msg[0] = Const.CHR_COLON;
                    msg[1] = Const.CHR_MOTOR_X;
                    msg[2] = Const.CHR_FUNC_MOVE_UP;
                    msg[3] = (char)150;
                    msg[4] = (char)i;
                    msg[5] = (char)152;
                    msg[6] = (char)153;
                    msg[7] = (char)154;
                    msg[8] = (char)155;
                    msg[9] = (char)1;

                    // Временное произвольное значение в качестве одного из параметров, для проверки CRC8
                    msg[11] = (char)rand.Next(256);

                    msg[13] = Const.CHR_COLON;
                    msg[14] = '\r';
                    msg[15] = '\n';
                    msg[16] = ' ';
                    */  
                    
                    List<char> msg = new List<char>();
                    msg.Add(Const.CHR_COLON);
                    msg.Add(Const.CHR_MOTOR_X);
                    msg.Add(Const.CHR_FUNC_MOVE_UP);
                    msg.Add((char)150);
                    msg.Add((char)i);
                    msg.Add((char)152);
                    msg.Add((char)153);
                    msg.Add((char)154);
                    msg.Add((char)155);
                    msg.Add((char)0);

                    // Временное произвольное значение в качестве одного из параметров, для проверки CRC8
                    msg.Add((char)rand.Next(256));

                    int x = rand.Next(4);
                    for (int j = 0; j < x; j++)
                        msg.Add((char)rand.Next(256));

                    msg.Add((char)0);
                    msg.Add(Const.CHR_COLON);
                    msg.Add('\r');
                    msg.Add('\n');
                    msg.Add(' ');

                    // Проверка на служебные символы и что-то сделать с ним если вдруг символ обнаружен
                    tc_item.CheckEndChar(msg, (byte)i);

                    // Array of char to Array of byte
                    byte[] temp = msg.Select(c => (byte)c).ToArray();

                    // Send request
                    string temp_transmit_msg = tc_item.WriteBytes(i, temp);

                    // Get response
                    string temp_res_msg = tc_item.ReadBytes();

                    // Log
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        string time = DateTime.Now.ToString("dd/MM/yy HH:mm:ss fff");
                        //string message = $"[{index.ToString("000000000")}] : ==> '{temp_src_msg}' and '{temp_res_msg}'";
                        string message = $" => '{temp_transmit_msg}' and '{temp_res_msg}'";

                        // Init Error True!
                        bool is_error = false;

                        // Index CRC8 Request
                        byte transmit_crc8_char = 0;
                        byte receiv_crc8_char = 0;
                        int index_crc8_req = tc_item.GetCRC8Index(temp_transmit_msg);
                        if (index_crc8_req > 0) {
                            transmit_crc8_char = byte.Parse(temp_transmit_msg.Split(',')[index_crc8_req]);

                            // Index CRC8 Response
                            int index_crc8_res =  tc_item.GetCRC8Index(temp_res_msg);
                            if (index_crc8_res > 0) {
                                receiv_crc8_char = byte.Parse(temp_res_msg.Split(',')[index_crc8_res]);

                                // Equal
                                is_error = transmit_crc8_char == receiv_crc8_char;
                            }
                        }

                        this.msg_list.Items.Add(new ItemRecord(
                            i, 
                            DateTime.Now, 
                            $"1", 
                            $"{message}. {transmit_crc8_char} = {receiv_crc8_char}", 
                            $"ping", 
                            !is_error)
                        );
                        this.msg_list.ScrollIntoView(this.msg_list.Items[this.msg_list.Items.Count - 1]);
                    }));
                }
            }
            finally
            {
                tc_item.Dispose();

                this.Dispatcher.Invoke((Action)(() =>
                {
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
