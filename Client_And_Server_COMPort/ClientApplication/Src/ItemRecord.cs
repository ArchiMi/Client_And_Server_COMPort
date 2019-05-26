using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDeamon.Src
{
    public class ItemRecord
    {
        private int _index;
        private DateTime _date_time;
        private string _status;
        private bool _is_error;
        private string _message;
        private string _cmd;

        public int Index
        {
            get { return this._index; } 
        }

        public DateTime DateTimeOperation
        {
            get { return this._date_time; }
        }

        public string Status
        {
            get { return this._status; }
        }

        public bool IsError
        {
            get { return this._is_error; }
        }

        public string Message
        {
            get { return this._message; }
        }

        public string CMD
        {
            get { return this._cmd; }
        }

        public ItemRecord(int index, DateTime date_time, string status, string message, string cmd, bool is_error)
        {
            this._index = index;
            this._date_time = date_time;
            this._status = status;
            this._message = message;
            this._cmd = cmd;
            this._is_error = is_error;
        }
    }
}
