using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientAppNameSpace.Src
{
    public class TempClass: IDisposable
    {
        private SerialPort _serial_port;
        private CRC8 _sum;
        private int _baud = 9600;
        private string _com_port_data;
        private Log _log;

        public TempClass(string com_port_data, int baud)
        {
            this._log = new Log("sessionn_num");

            this._sum = new CRC8();

            this._baud = baud;
            this._com_port_data = com_port_data;
            InitComPort();

            this._log.WriteInfo("*** Init session ***");
        }

        public void InitComPort()
        {
            try
            {
                if (this._serial_port != null && this._serial_port.IsOpen)
                {
                    this._serial_port.Close();
                    this._serial_port = null;
                }

                if (this._serial_port == null)
                    this._serial_port = new SerialPort(this._com_port_data);

                //4800, 9600, 19200, 38400, 57600, 115200
                this._serial_port.BaudRate = _baud;
                //serial_port.BaudRate = 9600;

                this._serial_port.Parity = Parity.None;
                this._serial_port.DataBits = 8;

                this._serial_port.StopBits = StopBits.Two;
                this._serial_port.ReadTimeout = 10000; //-1 200
                this._serial_port.WriteTimeout = 10000; //-1 50
                this._serial_port.Handshake = Handshake.None;
                this._serial_port.Encoding = Encoding.Default;
                //serial_port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

                this._serial_port.Open();
            }
            catch (Exception ex)
            {
                this._log.WriteError($"Init func exception: {ex.Message}");
            }
        }

        public void CheckEndChar(char[] msg, byte value)
        {
            for (int i = 0; i < msg.Length - 1; i++)
            {
                if (msg[i] == '\0' || msg[i] > 255)
                    msg[i] = (char)1;
            }
        }

        public string WriteBytes(int index, byte[] transmit_msg_chars)
        {
            try
            { 
                int index_crc8 = transmit_msg_chars.Length - 4; // 3 crars - 'crc8' char and ':\r\n' char

                // Очистим буфер In перед отправкой данных
                if (this._serial_port.IsOpen)
                    this._serial_port.DiscardInBuffer();

                // Get CRC8
                transmit_msg_chars[index_crc8] = this._sum.Crc8Bytes(transmit_msg_chars, index_crc8);
                string temp_transmit_msg = string.Join(",", transmit_msg_chars.Select(p => p.ToString()).ToArray());

                this._serial_port.Write(transmit_msg_chars, 0, transmit_msg_chars.Count());

                this._log.WriteInfo($"Write: {temp_transmit_msg}");

                return temp_transmit_msg;
            }
            catch (Exception ex)
            {
                this._log.WriteError($"WriteBytes func exception: {ex.Message}");
                throw new Exception("In WriteBytes");
            }
        }

        public string ReadBytes()
        {
            try
            { 
                char[] result = new char[Const.FRAME_LENGTH];
                try
                {
                    int i = 0;
                    int x = 0;

                    try
                    {
                        do
                        {
                            x = this._serial_port.ReadByte();
                            result[i++] = (char)x;

                        } while (x != '\r' || i == result.Length - 1);
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        //MessageBox.Show($"{ex.Message} (index={i})");
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show($"{ex.Message} (index={i})");
                    }
                }
                catch (IOException)
                {
                    this.InitComPort();
                }
                catch (TimeoutException)
                {
                    this.InitComPort();
                }

                // Очистим буфер Out после приема данных
                if (this._serial_port.IsOpen)
                    this._serial_port.DiscardOutBuffer();

                string res = string.Join(",", result.Select(x => ((byte)x).ToString()).ToArray());
                this._log.WriteInfo($"Read: {res}");
                return res;
            }
            catch (Exception ex)
            {
                this._log.WriteError($"ReadBytes func exception: {ex.Message}");
            }

            return "";
        }

        public void Dispose()
        {
            if (this._serial_port != null)
                if (this._serial_port.IsOpen)
                    this._serial_port.Close();
        }

        #region COM_PORT_RECEIVED

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

        #endregion
    }
}
