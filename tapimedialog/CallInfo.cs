using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace C4F.DevKit.Telephony
{

    /// <summary>
    /// Represents a call.
    /// </summary>
    public class CallInfo
    {

        #region Internal Members

        /// <summary>
        /// Id of the caller party.
        /// </summary>
        internal string callerIdName;
        internal string callerIdNumber;

        /// <summary>
        /// Id of the called party.
        /// </summary>
        internal string calledIdName;
        internal string calledIdNumber;
        /// <summary>
        /// Direction of the call.
        /// </summary>
        internal CallDirection callDirection;

        /// <summary>
        /// State of the call.
        /// </summary>
        internal CallState callState;

        /// <summary>
        /// Hash code to uniquely identify a call.
        /// </summary>
        internal int hashCode;

        /// <summary>
        /// Represents the time when the call was initiated.
        /// </summary>
        internal DateTime callInitiateTime;

        /// <summary>
        /// Represents the time when the call was connected.
        /// </summary>
        internal DateTime startTime;

        /// <summary>
        /// Represents the time when the call was disconnected.
        /// </summary>
        internal DateTime endTime;

        /// <summary>
        /// Name of the communication line.
        /// </summary>
        internal string lineName;

        #endregion

        #region Public Properties

        /// <summary>
        /// Id of the caller party.
        /// </summary>
        public string CallerIdName
        {
            get { return this.callerIdName; }
        }

        public string CallerIdNumber
        {
            get { return this.callerIdNumber; }
        }

        /// <summary>
        /// Id of the called party.
        /// </summary>
        public string CalledIdName
        {
            get { return this.calledIdName; }
        }

        public string CalledIdNumber
        {
            get { return this.calledIdNumber; }
        }

        /// <summary>
        /// Direction of the call.
        /// </summary>
        public CallDirection CallDirection
        {
            get { return this.callDirection; }
        }

        /// <summary>
        /// State of the call.
        /// </summary>
        public CallState CallState
        {
            get { return this.callState; }
        }

        /// <summary>
        /// Represents the time when the call was initiated.
        /// </summary>
        public DateTime CallInitiateTime
        {
            get { return this.callInitiateTime; }
        }

        /// <summary>
        /// Represents the time when the call was connected.
        /// </summary>
        public DateTime StartTime
        {
            get { return this.startTime; }
        }

        /// <summary>
        /// Represents the time when the call was disconnected.
        /// </summary>
        public DateTime EndTime
        {
            get { return this.endTime; }
        }

        /// <summary>
        /// Name of the communication line.
        /// </summary>
        public string LineName
        {
            get { return this.lineName; }
        }

        public int CallId
        {
            get { return this.hashCode; }
        }

        public bool PropertySet(/*object p, */string propName, object value)
        {
            //Type t = p.GetType();
            Type t = typeof(CallInfo);
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
            Type t = typeof(CallInfo);
            PropertyInfo info = t.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (info == null)
                return null;
            return info.GetValue(this,null);            
        }

        public static List<string> PropertyGetList(/*object p*/)
        {
            //Type t = p.GetType();
            Type t = typeof(CallInfo);
            List<string> list=new List<string>();
            PropertyInfo[] infos = t.GetProperties();
            foreach (PropertyInfo info in infos)
            {
                list.Add(info.Name);
            }
            return list;
        }

        #endregion

    }

}
