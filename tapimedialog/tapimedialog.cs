using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using C4F.DevKit.Telephony;

namespace tapimedialog
{
    class tapimedialog
    {
        private TapiBase tapiBase;
        private medialog_wrapper medialog;

        private DataSet ds_conf;
        private DataTable mappings;

        private string output_file;
        private string tapi_line_name;
        private verbosity debug_level;

        public void start()
        {
            debug_level = verbosity.HIGH;
            log("==================");
            load_config();
            
            initializetapi();
            
            medialog = new medialog_wrapper(output_file);
            medialog.addtolog = new medialog_wrapper.log(log);
            medialog.mappings = mappings;

        }

        void initializetapi()
        {
            List<string> lines_names = null;
            try
            {
                tapiBase = new TapiBase();
                if(debug_level>verbosity.MEDIUM)
                    tapiBase.debug = true;
                tapiBase.addtolog = new TapiBase.log(this.log);
                tapiBase.InitializeTapi(tapi_line_name);
                tapiBase.OnCallConnected += new TapiBase.CallNotificationEventHandler(tapiBase_OnCallConnected);

                lines_names = tapiBase.GetAddressLinesNames();
            }
            catch (Exception e)
            {
                log("Failed to initialize TAPI");
                log(e.ToString(),verbosity.LOW);
                System.Environment.Exit(255);
            }
            log("TAPI initialized, lines founded " + lines_names.Count + ": ");

            foreach (string name in lines_names)
            {
                log(name);
            }
        }

        private bool load_config()
        {
            bool config = false;
            string config_file_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\config.xml";
            string value = "";
            string value_name = "";
            try
            {
                ds_conf = new DataSet();
                ds_conf.ReadXml(config_file_path);
                config = true;
            }
            catch (Exception ex)
            {
                log("Error reading config: \n" + ex.Message);
                return false;
            }

            log(ds_conf.Tables["configuration"].ToString());

            value_name = "tapi_line_name";
            if ((ds_conf.Tables[0].Columns.Contains(value_name)))
            {
                value = ds_conf.Tables[0].Rows[0][value_name].ToString();
                if (string.IsNullOrEmpty(value))
                {
                    //ignore
                }
                else
                {
                    tapi_line_name = value;
                    log(value_name + "=" + value);
                }
            }

            value_name = "debug_level";
            if ((ds_conf.Tables[0].Columns.Contains(value_name)))
            {
                value = ds_conf.Tables[0].Rows[0][value_name].ToString();
                if (string.IsNullOrEmpty(value))
                {
                    //ignore
                }
                else
                {
                    try
                    {
                        debug_level = (verbosity)Enum.Parse(typeof(verbosity), value, true);
                    }
                    catch (Exception)
                    {
                        log("Unknown debug level: "+value);
                    }
                    
                    log(value_name + "=" + debug_level.ToString());
                }
            }

            value_name = "output_file";
            if ((ds_conf.Tables[0].Columns.Contains(value_name)))
            {
                value = ds_conf.Tables[0].Rows[0][value_name].ToString();
                if (string.IsNullOrEmpty(value))
                {
                    log(value_name + " doesn't set in config file");
                    config = false;
                }
                else
                {
                    output_file = value;
                    log(value_name + "=" + value);
                    config = true;
                }
            }
            value_name = "mappings";

            if (ds_conf.Tables.Contains(value_name))
            {
                mappings = ds_conf.Tables[value_name];
                string ColumnName = "";

                //last column is configuration_Id: 0
                for (int i = 0; i < mappings.Columns.Count-1; i++)
                {
                    ColumnName = mappings.Columns[i].ColumnName;
                    value = mappings.Rows[0][ColumnName].ToString();
                    log(ColumnName + ": " + value);
                }

            }

            return config;
        }

        public void shutdown()
        {
            log("shutdowning tapi...");
            tapiBase.TapiShutdown();
        }

        /// <summary>
        /// This method handles incoming call event.
        /// </summary>
        /// <param name="call">Call object.</param>
        private void tapiBase_OnCallConnected(CallInfo call)
        {
            log("Connected!");
            log("CalledIdName: " + call.calledIdName);
            log("CalledIdNumber: " + call.calledIdNumber);
            log("CalleRIdName: " + call.callerIdName);
            log("CalleRIdNumber: " + call.callerIdNumber);
            this.medialog.send_signal(call);
        }

        delegate void AddLogDelegate(string text);


        public void log(string str)
        {
            string time = DateTime.Now.ToString();
            System.IO.File.AppendAllText("log.txt", time + ": " + str + "\n", Encoding.UTF8);
            Console.WriteLine(str);
        }
        public void log(string str, verbosity message_level)
        {
            if (message_level <= debug_level)
                log(str);
        }
    }

}
