using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using C4F.DevKit.Telephony;

namespace tapimedialog
{
    class medialog_wrapper
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        [DllImport("user32.dll", EntryPoint = "PostMessage", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hwnd, uint Msg, long wParam, long lParam);

        public delegate void log(string str, verbosity message_level);
        public log addtolog;

        private string _outfilepath;
        private string _window_class;
        private uint _message_code;

        public System.Data.DataTable mappings;

        public medialog_wrapper(string outfilepath)
        {
            _window_class = "TfMain";

            if (string.IsNullOrEmpty(outfilepath))
                _outfilepath = Environment.GetEnvironmentVariable("programfiles") + @"\pmt\medialog\private\CISCO_CALL.INI";
            else
                _outfilepath = outfilepath;
            
            _message_code = 1803;
        }

        public void send_signal(CallInfo call)
        {

            List<string> contents = new List<string>();
            if (mappings == null)
            {
                addtolog("No mappings fields", verbosity.MEDIUM);
                return;
            }

            string value;
            string value_name;
            //string field;
            string ColumnName = "";

            addtolog("Mappings in callinfo file: ",verbosity.HIGH);

            List<string> call_fields = call.PropertyGetList();
            foreach (string field in call_fields)
            {
                if (mappings.Columns.Contains(field))
                {
                    value_name = mappings.Rows[0][field].ToString();
                    value = (string)call.PropertyGet(field);

                    addtolog("field:"+field +"\t name="+ value_name + "\t " + value, verbosity.HIGH);
                    if (!string.IsNullOrEmpty(value_name))
                    {
                        contents.Add(value_name+"=" + value);
                    }
                }
            }

            addtolog("Writing callinfo file " + _outfilepath, verbosity.HIGH);
            try
            {
                File.WriteAllLines(_outfilepath, contents.ToArray());
            }
            catch (Exception ex)
            {
                if (addtolog != null) addtolog(ex.ToString(), verbosity.LOW);
                return;
            }

            //----------------------------
            IntPtr hwndMedialog = IntPtr.Zero;
            //hwndMedialog = FindWindow(_window_class, null);
            if (addtolog != null) addtolog("hwndMedialog=" + hwndMedialog.ToString(),verbosity.DEBUG);
            if (hwndMedialog != IntPtr.Zero)
                PostMessage(hwndMedialog, _message_code, 0, 0);
        }
    }
}
