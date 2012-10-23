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

        private verbosity debug_level;

        Configuration config;

        public void start()
        {
            debug_level = verbosity.HIGH;
            log("==================");
            
            if (!load_config())
            {
                shutdown();
            }
            if (!initializetapi())
            {
                shutdown();
            }
            
            medialog = new medialog_wrapper(config);
            medialog.addtolog = new medialog_wrapper.log_delegate(log);

        }

        private bool initializetapi()
        {
            List<string> lines_names = null;
            try
            {
                tapiBase = new TapiBase();
                if(debug_level>verbosity.MEDIUM)
                    tapiBase.debug = true;
                tapiBase.addtolog = new TapiBase.log_delegate(this.log);
                tapiBase.InitializeTapi(config.Tapi_line_name);
                tapiBase.OnCallConnected += new TapiBase.CallNotificationEventHandler(tapiBase_OnCallConnected);

                lines_names = tapiBase.GetAddressLinesNames();
            }
            catch (Exception e)
            {
                log("Failed to initialize TAPI");
                log(e.ToString(),verbosity.LOW);
                return false;
            }
            log("TAPI initialized, lines founded " + lines_names.Count + ": ",verbosity.HIGH);
            if (lines_names.Count == 0)
                return false;

            foreach (string name in lines_names)
            {
                log(name);
            }
            return true;
        }

        private bool load_config()
        {
            bool config_ok = false;
            string config_file_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\config.xml";
            string value = "";
            string value_name = "";
            
            DataSet ds_conf;
            DataTable mappings;
            config = new Configuration();

            try
            {
                ds_conf = new DataSet();
                ds_conf.ReadXml(config_file_path);
                config_ok = true;
            }
            catch (Exception ex)
            {
                log("Error reading config: \n" + ex.Message);
                return false;
            }

            log(ds_conf.Tables["configuration"].ToString(),verbosity.MEDIUM);

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
                    config.PropertySet(value_name, value);
                    log(value_name + "=" + value,verbosity.MEDIUM);
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
                        log("Unknown debug level: "+value,verbosity.LOW);
                    }

                    config.PropertySet(value_name, debug_level);
                    log(value_name + "=" + debug_level.ToString());
                }
            }

            value_name = "outputfile";
            if ((ds_conf.Tables[0].Columns.Contains(value_name)))
            {
                value = ds_conf.Tables[0].Rows[0][value_name].ToString();
                if (string.IsNullOrEmpty(value))
                {
                    log(value_name + " doesn't set in config file",verbosity.LOW);
                    config_ok = false;
                }
                else
                {
                    log(value_name + "=" + value,verbosity.MEDIUM);
                    config.PropertySet(value_name, value);
                    config_ok = true;
                }
            }
            value_name = "mappings";

            if (ds_conf.Tables.Contains(value_name))
            {
                mappings = ds_conf.Tables[value_name];
                string ColumnName = "";
                config.Mappings = new Dictionary<string, string>();
                
                List<string> call_fields = CallInfo.PropertyGetList();
                for (int i = 0; i < call_fields.Count;i++)
                {
                    call_fields[i]=call_fields[i].ToUpper();
                }
                    //last column is configuration_Id: 0
                    for (int i = 0; i < mappings.Columns.Count - 1; i++)
                    {
                        ColumnName = mappings.Columns[i].ColumnName.ToUpper();
                        if (call_fields.Contains(ColumnName))
                        {
                            value = mappings.Rows[0][ColumnName].ToString();
                            config.Mappings.Add(ColumnName, value);
                            log(ColumnName + ": " + value, verbosity.HIGH);
                        }
                        else
                            log("Unknown mapping field: " + ColumnName,verbosity.LOW);
                    }

            }

            config.Debug_level = debug_level;

            return config_ok;
        }

        public void shutdown()
        {
            if (tapiBase != null)
            {
                log("Shutdowning tapi...", verbosity.LOW);
                tapiBase.TapiShutdown();
            }
            log("Exiting...", verbosity.LOW);
            System.Environment.Exit(255);
        }

        /// <summary>
        /// This method handles incoming call event.
        /// </summary>
        /// <param name="call">Call object.</param>
        private void tapiBase_OnCallConnected(CallInfo call)
        {
            log("Connected!",verbosity.MEDIUM);
            log("CalledIdName: " + call.calledIdName, verbosity.MEDIUM);
            log("CalledIdNumber: " + call.calledIdNumber, verbosity.MEDIUM);
            log("CalleRIdName: " + call.callerIdName, verbosity.MEDIUM);
            log("CalleRIdNumber: " + call.callerIdNumber, verbosity.MEDIUM);
            this.medialog.send_signal(call);
        }

        public void log(string str)
        {
            string time = DateTime.Now.ToString();
            Console.WriteLine(str);
            try
            {
                System.IO.File.AppendAllText("log.txt", time + ": " + str + "\n", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void log(string str, verbosity message_level)
        {
            if (message_level <= debug_level)
                log(str);
        }
    }

}
