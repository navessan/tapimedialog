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

        public delegate void log_delegate(string str, verbosity message_level);
        public log_delegate addtolog;

        private string _outfilepath;
        private string _window_class;
        private uint _message_code;
        private Encoding _encoding;

        public Dictionary<string, string> mappings;

        private void log(string str, verbosity message_level)
        {
            if (addtolog != null)
                addtolog(str, message_level);
        }

        public medialog_wrapper(Configuration config)
        {
            _window_class = "TfMain";
            _message_code = 1803;

            if (string.IsNullOrEmpty(config.OutputFile))
                _outfilepath = Environment.GetEnvironmentVariable("programfiles") + @"\pmt\medialog\private\CISCO_CALL.INI";
            else
                _outfilepath = config.OutputFile;

            if (true)
                _encoding = Encoding.Default;

            mappings = config.Mappings;
        }

        public void send_signal(CallInfo call)
        {

            List<string> contents = new List<string>();
            if (mappings == null)
            {
                log("No mappings fields", verbosity.MEDIUM);
                return;
            }

            string value = null;
            string value_name;
            string field = "";

            log("Mappings in callinfo file: ", verbosity.HIGH);

            List<string> call_fields = CallInfo.PropertyGetList();
            for (int i = 0; i < call_fields.Count; i++)
            {
                field = call_fields[i].ToUpper();

                if (mappings.ContainsKey(field))
                {
                    value_name = mappings[field].ToString();
                    Object obj = call.PropertyGet(field);

                    if (obj.GetType() == typeof(String))
                        value = (string)obj;
                    else
                        if (obj.GetType() == typeof(Int32))
                            value = ((Int32)obj).ToString();
                        else
                            if (obj.GetType() == typeof(CallDirection))
                                value = ((CallDirection)obj).ToString();
                    
                    value= value.Replace("\n", " ").Replace("\t"," ");

                    log("field:" + field + "\t name=" + value_name + "\t value:" + value, verbosity.HIGH);
                    if (!string.IsNullOrEmpty(value_name))
                    {
                        contents.Add(value_name + "=" + value);
                    }
                }
            }

            log("Writing callinfo file " + _outfilepath, verbosity.HIGH);
            try
            {
                File.WriteAllLines(_outfilepath, contents.ToArray(), _encoding);
            }
            catch (Exception ex)
            {
                log(ex.ToString(), verbosity.LOW);
                return;
            }

            //----------------------------
            IntPtr hwndMedialog = IntPtr.Zero;
            hwndMedialog = FindWindow(_window_class, null);
            log("hwndMedialog=" + hwndMedialog.ToString(), verbosity.DEBUG);
            if (hwndMedialog != IntPtr.Zero)
                PostMessage(hwndMedialog, _message_code, 0, 0);
        }
    }
}
