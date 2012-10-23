using System;
using System.Collections.Generic;
using C4F.DevKit.Telephony;
using System.Reflection;
using System.Text;

namespace tapimedialog
{
    #region -- Configuration Class --

    public class Configuration
    {
        private string _tapi_line_name;
        private string _output_file;
        private string _client_name;
        private verbosity _debug_level;
        private Encoding _encoding;
        private string _test;
        
        public Dictionary<string, string> Mappings;

        public string Tapi_line_name
        {
            get { return _tapi_line_name; }
            set { _tapi_line_name = value; }
        }

        public string OutputFile
        {
            get { return _output_file; }
            set { _output_file = value; }
        }

        public string Client_name
        {
            get { return _client_name; }
            set { _client_name = value; }
        }

        public verbosity Debug_level
        {
            get { return _debug_level; }
            set { _debug_level = value; }
        }

        public string test_
        {
            get { return _test; }
            set { _test = value; }
        }

        public Configuration()
        {
            _tapi_line_name = "";
            _output_file = "";
            _client_name = "";
            _debug_level = verbosity.MEDIUM;
            Mappings = null;
            _encoding = Encoding.Default;

        }

        public bool PropertySet(/*object p, */string propName, object value)
        {
            //Type t = p.GetType();
            Type t = typeof(Configuration);
            PropertyInfo info = t.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (info == null)
                return false;
            if (!info.CanWrite)
                return false;
            info.SetValue(this, value, null);
            return true;
        }
        public object PropertyGet(/*object p, */string propName)
        {
            //Type t = p.GetType();
            Type t = typeof(Configuration);
            PropertyInfo info = t.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (info == null)
                return null;
            return info.GetValue(this, null);
        }

        public static List<string> PropertyGetList(/*object p*/)
        {
            //Type t = p.GetType();
            Type t = typeof(Configuration);
            List<string> list = new List<string>();
            PropertyInfo[] infos = t.GetProperties();
            foreach (PropertyInfo info in infos)
            {
                list.Add(info.Name);
            }
            return list;
        }
    }
    #endregion


}
