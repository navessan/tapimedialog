using System;
using System.Collections.Generic;
using System.Text;
using TAPI3Lib;
using System.Diagnostics;
using System.Xml;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Globalization;

namespace C4F.DevKit.Telephony
{

    /// <summary>
    /// This class provides basic telephony functionality.
    /// </summary>
    internal class TapiBase
    {

        #region Private Members

        /// <summary>
        /// TAPI object.
        /// </summary>
        private TAPIClass tapiObject = new TAPIClass();

        /// <summary>
        /// List of tokens of the registered lines.
        /// </summary>
        private List<int> registrationTokens = new List<int>();

        /// <summary>
        /// Represents the currently available calls.
        /// </summary>
        private List<CallInfo> availableCalls = new List<CallInfo>();

        /// <summary>
        /// Represents the currently connected call.
        /// </summary>
        private CallInfo currentCall = null;

        /// <summary>
        /// Represents the ITCallInfo interface for current call.
        /// </summary>
        private ITCallInfo iTCurrentCallInfo = null;

        /// <summary>
        /// Represents the file play back terminal on the current call.
        /// </summary>
        private ITTerminal playbackTerminal = null;

        /// <summary>
        /// Represents the file recording terminal on the current call.
        /// </summary>
        private ITTerminal fileRecordingTerminal = null;

        #endregion

        public bool debug=false;

        #region Constants
        
        #endregion

        #region Internal Properties

        /// <summary>
        /// Represents the currently connected call.
        /// </summary>
        internal CallInfo CurrentCall
        {

            get { return this.currentCall; }

        }

        /// <summary>
        /// Represents the currently available calls.
        /// </summary>
        internal List<CallInfo> AvailableCalls
        {

            get { return this.availableCalls; }

        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new object of TapiManager.
        /// </summary>
        internal TapiBase()
        {
            //InitializeTapi();
        }

        #endregion

        #region Delegates and Events

        /// <summary>
        /// Delegate to handle the incoming and outgoing call event.
        /// </summary>
        /// <param name="callInfo">Information about the call which initiated the event.</param>
        internal delegate void CallNotificationEventHandler(CallInfo callInfo);

        /// <summary>
        /// This event is raised when there is an incoming call.
        /// </summary>
        internal event CallNotificationEventHandler OnIncomingCall;

        /// <summary>
        /// This event is raised when there is an outging call.
        /// </summary>
        internal event CallNotificationEventHandler OnOutgoingCall;

        /// <summary>
        /// This event is raised when a call has been connected.
        /// </summary>
        internal event CallNotificationEventHandler OnCallConnected;

        /// <summary>
        /// This event is raised when a call has been disconnected.
        /// </summary>
        internal event CallNotificationEventHandler OnCallDisconnected;

        /// <summary>
        /// This event is raised when a call is placed on hold.
        /// </summary>
        internal event CallNotificationEventHandler OnCallHold;

        /// <summary>
        /// Delegate to handle the digit event.
        /// </summary>
        /// <param name="callInfo">Information about the call which initiated the digit event.</param>
        /// <param name="digit">Received digit.</param>
        internal delegate void DigitNotificationEventHandler(CallInfo callInfo, char digit);

        /// <summary>
        /// This event is raised when a digit is received.
        /// </summary>
        internal event DigitNotificationEventHandler OnDigitReceived;

        /// <summary>
        /// Delegate to handle the end of playback file event.
        /// </summary>
        /// <param name="callInfo">Information about the call on which the file was being played.</param>
        internal delegate void EndOfPlaybackNotificationEventHandler(CallInfo callInfo);

        /// <summary>
        /// This event is raised when the end of playback file event is generated.
        /// </summary>
        internal event EndOfPlaybackNotificationEventHandler OnEndOfFilePlayback;

        public delegate void log_delegate(string str, verbosity message_level);
        public log_delegate addtolog;

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the communication lines that are currently available.
        /// </summary>
        /// <returns>List of available lines with audio support.</returns>
        internal List<Line> GetAvailableLines()
        {
            List<Line> lines = new List<Line>();
            Line line = null;
            List<ITAddress> iTAddresses = GetAddressLines();

            try
            {
                foreach (ITAddress iAddress in iTAddresses)
                {
                    ITAddressCapabilities iCapabilities = (ITAddressCapabilities)iAddress;

                    // Check if it supports phone number.
                    if ((iCapabilities.get_AddressCapability(ADDRESS_CAPABILITY.AC_ADDRESSTYPES) & 1) == 1)
                    {
                        // Check if it supports audio.
                        if (TapiBase.CheckLineForAudioSupport(iAddress))
                        {
                            line = new Line();
                            line.lineName = iAddress.AddressName;
                            ITMediaSupport iMedia = (ITMediaSupport)iAddress;
                            line.supportedMediaTypes = (TapiMediaType)iMedia.MediaTypes;
                            line.maxCallsOnHold = iCapabilities.get_AddressCapability(ADDRESS_CAPABILITY.AC_MAXONHOLDCALLS);
                            if (iCapabilities.get_AddressCapability(ADDRESS_CAPABILITY.AC_GENERATEDIGITSUPPORT) != 0 && iCapabilities.get_AddressCapability(ADDRESS_CAPABILITY.AC_MONITORDIGITSUPPORT) != 0)
                            {
                                line.isDtmfSupported = true;
                            }
                            else
                            {
                                line.isDtmfSupported = false;
                            }
                            lines.Add(line);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw ;
            }
            return lines;
        }
 
        /// <summary>
        /// Unregisters all the registered lines and then ends the TAPI session.
        /// </summary>
        internal void TapiShutdown()
        {
            try
            {
                this.tapiObject.ITTAPIEventNotification_Event_Event -= new TAPI3Lib.ITTAPIEventNotification_EventEventHandler(Event);
                foreach (int token in this.registrationTokens)
                    this.tapiObject.UnregisterNotifications(token);
                this.tapiObject.Shutdown();
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        #endregion

        #region Private Methods

        private void log(string str, verbosity message_level)
        {
            if (addtolog != null)
                addtolog(str, message_level);
        }

        /// <summary>
        /// Initializes the tapi session.
        /// </summary>
        /// <param name="tapi_line_name"></param>
        public void InitializeTapi(string tapi_line_name)
        {
            try
            {
                this.currentCall = null;
                this.iTCurrentCallInfo = null;
                this.registrationTokens = new List<int>();
                this.availableCalls = new List<CallInfo>();

                // Initialize TAPI.
                this.tapiObject.Initialize();

                if (string.IsNullOrEmpty(tapi_line_name))
                {
                    // Get Available address lines.
                    List<ITAddress> iTAddresses = GetAddressLines();
                    foreach (ITAddress iTAddress in iTAddresses)
                    {
                        // Check whether is supports Audio.
                        if (TapiBase.CheckLineForAudioSupport(iTAddress))
                        {
                            // Register for call notification events.
                            RegisterLineForIncomingCalls(iTAddress);
                        }
                    }
                }
                else
                {
                    ITAddress iTAddress = GetLineObject(tapi_line_name);
                    // Register for call notification events.
                    if (iTAddress == null)
                    {
                        log("Line \"" + tapi_line_name + "\" not found",verbosity.LOW);
                        throw new Exception("Line \"" + tapi_line_name + "\" not found");
                    }
                    else
                        RegisterLineForIncomingCalls(iTAddress);
                }

                if(this.debug)
                    this.tapiObject.ITTAPIEventNotification_Event_Event += new TAPI3Lib.ITTAPIEventNotification_EventEventHandler(Event_Debug);
                else
                    this.tapiObject.ITTAPIEventNotification_Event_Event += new TAPI3Lib.ITTAPIEventNotification_EventEventHandler(Event);

                // Specify which events it must receive. Sets the event filter mask.
                this.tapiObject.EventFilter = (int)(TAPI_EVENT.TE_CALLNOTIFICATION | TAPI_EVENT.TE_CALLSTATE | TAPI_EVENT.TE_DIGITEVENT | TAPI_EVENT.TE_GATHERDIGITS | TAPI_EVENT.TE_FILETERMINAL);
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets the list of ITAddress interfaces based on the currently available address lines.
        /// </summary>
        /// <returns>List of ITAddress interfaces.</returns>
        private List<ITAddress> GetAddressLines()
        {
            List<ITAddress> iTAddresses = new List<ITAddress>();
            try
            {
                // Enumerate the addresses that are currently available.
                TAPI3Lib.IEnumAddress enumAddresses = this.tapiObject.EnumerateAddresses();
                ITAddress iTAddress;
                uint linesFetched = 0;
                while (true)
                {
                    enumAddresses.Next(1, out iTAddress, ref linesFetched);
                    if (iTAddress == null)
                    {
                        break;
                    }
                    iTAddresses.Add(iTAddress);
                }
            }
            catch (Exception exception)
            {
                throw;
            }

            return iTAddresses;
        }

        /// <summary>
        /// Gets the list of names of ITAddress interfaces based on the currently available address lines.
        /// </summary>
        /// <returns>List of names ITAddress interfaces.</returns>
        public List<string> GetAddressLinesNames()
        {
            List<string> lines_names = new List<string>();
            List<ITAddress> iTAddresses = null;
            
            try
            {
               iTAddresses = GetAddressLines();
            }
            catch (Exception)
            {
                throw;
            }

            foreach (ITAddress addr in iTAddresses)
            {
                lines_names.Add(addr.AddressName);
            }

            return lines_names;
        }

        /// <summary>
        /// Gets the ITAddress interface based on the specified line name.
        /// </summary>
        /// <param name="lineName">Name of the line.</param>
        /// <returns>ITAddress interface if match found, else returns null.</returns>
        private ITAddress GetLineObject(string lineName)
        {
            List<ITAddress> iTAddresses = GetAddressLines();
            foreach (ITAddress iTAddress in iTAddresses)
            {
                if (String.Compare(iTAddress.AddressName, lineName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return iTAddress;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks a line for audio support.
        /// </summary>
        /// <param name="iTAddress">ITAddress interface of the line to be checked.</param>
        /// <returns>True, if line supports audio.</returns>
        private static bool CheckLineForAudioSupport(ITAddress iTAddress)
        {
            try
            {
                // Check whether the address line supports audio.
                ITMediaSupport iMediaSupport = (ITMediaSupport)iTAddress;
                return iMediaSupport.QueryMediaType(TapiConstants.TAPIMEDIATYPE_AUDIO);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Method which raises an event based on the call state.
        /// </summary>
        /// <param name="iTCallInfo">ITCallInfo interface.</param>
        /// <param name="callState">Call state.</param>
        private void CallEventMethod(ITCallInfo iTCallInfo, CALL_STATE callState)
        {
            CallInfo call = TapiBase.GetCallDetails(iTCallInfo);

            switch (iTCallInfo.CallState)
            {
                case CALL_STATE.CS_INPROGRESS:
                    call.callInitiateTime = DateTime.Now;
                    call.callDirection = CallDirection.Outgoing;
                    this.availableCalls.Add(call);
                    // Make the dialed call as current call.
                    this.currentCall = call;
                    this.iTCurrentCallInfo = iTCallInfo;
                    break;

                case CALL_STATE.CS_OFFERING:
                    call.callInitiateTime = DateTime.Now;
                    call.callDirection = CallDirection.Incoming;
                    this.availableCalls.Add(call);
                    break;

                case CALL_STATE.CS_CONNECTED:
                    for (int i = 0; i < this.availableCalls.Count; i++)
                    {
                        // Set the start time.
                        if (this.availableCalls[i].hashCode == call.hashCode)
                        {
                            call.startTime = DateTime.Now;
                            call.callInitiateTime = this.availableCalls[i].callInitiateTime;
                            call.callDirection = this.availableCalls[i].callDirection;
                            this.availableCalls.RemoveAt(i);
                            this.availableCalls.Add(call);
                            this.currentCall = call;
                            this.iTCurrentCallInfo = iTCallInfo;
                            break;
                        }
                    }
                    break;

                case CALL_STATE.CS_DISCONNECTED:
                    for (int i = 0; i < this.availableCalls.Count; i++)
                    {
                        if (this.availableCalls[i].hashCode == call.hashCode)
                        {
                            // Set call end time.
                            call.startTime = this.availableCalls[i].startTime;
                            call.callInitiateTime = this.availableCalls[i].callInitiateTime;
                            call.callDirection = this.availableCalls[i].callDirection;
                            call.endTime = DateTime.Now;
                            this.availableCalls.RemoveAt(i);
                            this.currentCall = null;
                            this.iTCurrentCallInfo = null;
                            break;
                        }
                    }
                    break;

                case CALL_STATE.CS_HOLD:
                    for (int i = 0; i < this.availableCalls.Count; i++)
                    {
                        if (this.availableCalls[i].hashCode == call.hashCode)
                        {
                            call.callDirection = this.availableCalls[i].callDirection;
                            this.availableCalls.RemoveAt(i);
                            this.availableCalls.Add(call);
                            this.currentCall = call;
                            this.iTCurrentCallInfo = iTCallInfo;
                            break;
                        }
                    }
                    break;
            }

            if (callState == CALL_STATE.CS_OFFERING)
                OnIncomingCall(call);
            else if (callState == CALL_STATE.CS_INPROGRESS)
                OnOutgoingCall(call);
            else if (callState == CALL_STATE.CS_CONNECTED)
                OnCallConnected(call);
            else if (callState == CALL_STATE.CS_DISCONNECTED)
                OnCallDisconnected(call);
            else if (callState == CALL_STATE.CS_HOLD)
                OnCallHold(call);
        }

        /// <summary>
        /// Extracts the call details from ITCallInfo interface.
        /// </summary>
        /// <param name="iTCallInfo">ITCallInfo interface.</param>
        /// <returns>Call details.</returns>
        private static CallInfo GetCallDetails(ITCallInfo iTCallInfo)
        {
            CallInfo call = new CallInfo();

            try
            {
                call.callerIdName = iTCallInfo.get_CallInfoString(CALLINFO_STRING.CIS_CALLERIDNAME);
            }
            catch
            {
                call.callerIdName = String.Empty;
            }
            try
            {
                call.callerIdNumber = iTCallInfo.get_CallInfoString(CALLINFO_STRING.CIS_CALLERIDNUMBER);
            }
            catch
            {
                call.callerIdNumber = String.Empty;
            }

            try
            {
                call.calledIdName = iTCallInfo.get_CallInfoString(CALLINFO_STRING.CIS_CALLEDIDNAME);
            }
            catch
            {
                call.calledIdName = String.Empty;
            }
            try
            {
                call.calledIdNumber = iTCallInfo.get_CallInfoString(CALLINFO_STRING.CIS_CALLEDIDNUMBER);
            }
            catch
            {
                call.calledIdNumber = String.Empty;
            }

            call.callState = (CallState)iTCallInfo.CallState;
            call.hashCode = iTCallInfo.GetHashCode();
            call.lineName = iTCallInfo.Address.AddressName;
            return call;
        }

        /// <summary>
        /// Gets the ITCallInfo interface based on the specified call.
        /// </summary>
        /// <param name="call">Call details.</param>
        /// <returns>ITCallInfo interface if match found, else returns null.</returns>
        private ITCallInfo ConvertToCallInfoObject(CallInfo call)
        {
            // Get the available address lines with audio support.
            List<Line> addressLines = GetAvailableLines();

            foreach (Line addressLine in addressLines)
            {
                ITAddress iTAddress = GetLineObject(addressLine.lineName);

                if (String.Equals(iTAddress.AddressName, call.lineName))
                {
                    IEnumCall iEnumCall = iTAddress.EnumerateCalls();

                    uint callsFetched = 0;
                    ITCallInfo iTCallInfo = null;
                    try
                    {
                        while (true)
                        {
                            // Enumerate calls on the address line.
                            iEnumCall.Next(1, out iTCallInfo, ref callsFetched);
                            if (iTCallInfo == null)
                                break;
                            if (iTCallInfo.GetHashCode() == call.hashCode)
                                return iTCallInfo;
                        }
                    }
                    catch
                    {
                        // Do nothing. Check the next address line.
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// TAPI calls this method when an event occurs.
        /// </summary>
        /// <param name="tapiEvent">TapiEvent.</param>
        /// <param name="pEvent">Object associated with the event.</param>
        private void Event(TAPI_EVENT tapiEvent, object pEvent)
        {
            switch (tapiEvent)
            {
                case TAPI_EVENT.TE_DIGITEVENT:
                    ITDigitDetectionEvent iTDigitDetectionEvent = (ITDigitDetectionEvent)pEvent;
                    char digit = (char)iTDigitDetectionEvent.Digit;
                    OnDigitReceived(this.currentCall, digit);
                    break;

                case TAPI_EVENT.TE_CALLSTATE:
                    ITCallStateEvent iTCallStateEvent = (ITCallStateEvent)pEvent;

                    ITCallInfo iTCallInfo = iTCallStateEvent.Call;

                    switch (iTCallInfo.CallState)
                    {
                        case CALL_STATE.CS_INPROGRESS:
                            CallEventMethod(iTCallInfo, CALL_STATE.CS_INPROGRESS);
                            break;

                        case CALL_STATE.CS_CONNECTED:
                            CallEventMethod(iTCallInfo, CALL_STATE.CS_CONNECTED);
                            break;

                        case CALL_STATE.CS_DISCONNECTED:
                            ITBasicCallControl2 iTBasicCallControl2 = (ITBasicCallControl2)iTCallInfo;
                            if (this.fileRecordingTerminal != null)
                            {
                                ITMediaControl mediaControl = (ITMediaControl)this.fileRecordingTerminal;
                                mediaControl.Stop();
                            }
                            CallEventMethod(iTCallInfo, CALL_STATE.CS_DISCONNECTED);
                            break;

                        case CALL_STATE.CS_OFFERING:
                            CallEventMethod(iTCallInfo, CALL_STATE.CS_OFFERING);
                            break;

                        case CALL_STATE.CS_HOLD:
                            CallEventMethod(iTCallInfo, CALL_STATE.CS_HOLD);
                            break;
                    }
                    break;
                case TAPI_EVENT.TE_FILETERMINAL:
                    ITFileTerminalEvent iTFileTerminalEvent = (ITFileTerminalEvent)pEvent;
                    if (iTFileTerminalEvent.Cause == FT_STATE_EVENT_CAUSE.FTEC_END_OF_FILE)
                    {
                        OnEndOfFilePlayback(this.currentCall);
                    }
                    break;
            }
        }

        public void Event_Debug(TAPI3Lib.TAPI_EVENT te, object eobj)
        {
            TAPI3Lib.ITCallStateEvent call_state_event = null;
            TAPI3Lib.ITCallInfoChangeEvent call_info_change_event = null;
            TAPI3Lib.ITCallNotificationEvent call_notification_event = null;
            TAPI3Lib.ITCallInfo call_info = null;

            string c = "";

            //System.Threading.Thread.Sleep(500);
            switch (te)
            {
                case TAPI3Lib.TAPI_EVENT.TE_CALLNOTIFICATION:
                    log("TE_CALLNOTIFICATION: ",verbosity.HIGH);
                    call_notification_event = (TAPI3Lib.ITCallNotificationEvent)eobj;
                    call_info = call_notification_event.Call;
                    break;
                case TAPI3Lib.TAPI_EVENT.TE_DIGITEVENT:
                    TAPI3Lib.ITDigitDetectionEvent dd = (TAPI3Lib.ITDigitDetectionEvent)eobj;
                    log("Dialed digit" + dd.ToString(), verbosity.HIGH);
                    break;
                case TAPI3Lib.TAPI_EVENT.TE_GENERATEEVENT:
                    log("digit dialed!", verbosity.HIGH);
                    TAPI3Lib.ITDigitGenerationEvent dg = (TAPI3Lib.ITDigitGenerationEvent)eobj;
                    log("Dialed digit" + dg.ToString(), verbosity.HIGH);
                    break;
                case TAPI3Lib.TAPI_EVENT.TE_PHONEEVENT:
                    log("A phone event!", verbosity.HIGH);
                    break;
                case TAPI3Lib.TAPI_EVENT.TE_GATHERDIGITS:
                    log("Gather digit event!", verbosity.HIGH);
                    break;
                case TAPI3Lib.TAPI_EVENT.TE_CALLSTATE:
                    call_state_event = (TAPI3Lib.ITCallStateEvent)eobj;
                    call_info = call_state_event.Call;
                    log("TE_CALLSTATE: " + call_info.Address.AddressName, verbosity.HIGH);
                    break;
                case TAPI3Lib.TAPI_EVENT.TE_CALLINFOCHANGE:
                    call_info_change_event = (TAPI3Lib.ITCallInfoChangeEvent)eobj;
                    call_info = call_info_change_event.Call;
                    log("TE_CALLINFOCHANGE: " + call_info.Address.AddressName, verbosity.HIGH);
                    break;
            }
            if (call_info == null)
            {
                log("TAPI_EVENT: " + te.ToString() + ", call_info is null", verbosity.HIGH);
                return;
            }

            switch (call_info.CallState)
            {
                case TAPI3Lib.CALL_STATE.CS_INPROGRESS:
                    log("dialing " + call_info.Address.AddressName, verbosity.HIGH);
                    break;
                case TAPI3Lib.CALL_STATE.CS_CONNECTED:
                    log("Connected " + call_info.Address.AddressName, verbosity.HIGH);
                    try
                    {
                        c = call_info.get_CallInfoString(CALLINFO_STRING.CIS_CALLERIDNAME);
                        log("CALLERIDNAME " + c, verbosity.HIGH);
                        c = call_info.get_CallInfoString(CALLINFO_STRING.CIS_CALLERIDNUMBER);
                        log("CALLERIDNUMBER " + c, verbosity.HIGH);
                    }
                    catch (Exception ex)
                    {
                        log("Exception Catched: " + ex.ToString(), verbosity.HIGH);
                    }

                    break;
                case TAPI3Lib.CALL_STATE.CS_DISCONNECTED:
                    log("Disconnected", verbosity.HIGH);
                    break;
                case TAPI3Lib.CALL_STATE.CS_OFFERING:
                    log("Incoming on line " + call_info.Address.AddressName + "->" + call_info.Address.DialableAddress, verbosity.HIGH);
                    try
                    {
                        c = call_info.get_CallInfoString(CALLINFO_STRING.CIS_CALLEDIDNAME);
                        log("CALLEDIDNAME " + c, verbosity.HIGH);
                        //c = call_info.get_CallInfoString(CALLINFO_STRING.CIS_CALLEDIDNUMBER);
                        //log("CALLEDIDNUMBER " + c);
                        c = call_info.get_CallInfoString(CALLINFO_STRING.CIS_CALLERIDNAME);
                        log("CALLERIDNAME " + c, verbosity.HIGH);
                        c = call_info.get_CallInfoString(CALLINFO_STRING.CIS_CALLERIDNUMBER);
                        log("CALLERIDNUMBER " + c, verbosity.HIGH);
                        c = call_info.get_CallInfoString(CALLINFO_STRING.CIS_DISPLAYABLEADDRESS);
                        log("DISPLAYABLEADDRESS " + c, verbosity.HIGH);
                    }
                    catch (Exception ex)
                    {
                        System.Runtime.InteropServices.COMException e;

                        log("Exception Catched: " + ex.ToString(), verbosity.HIGH);
                    }
                    break;

                case TAPI3Lib.CALL_STATE.CS_QUEUED:
                    log("CALL_STATE: CS_QUEUED on line " + call_info.Address.AddressName, verbosity.HIGH);

                    break;
                case TAPI3Lib.CALL_STATE.CS_IDLE:
                    log("CALL_STATE: CS_IDLE on line " + call_info.Address.AddressName, verbosity.HIGH);
                    try
                    {
                        c = call_info.get_CallInfoString(CALLINFO_STRING.CIS_CALLERIDNAME);
                        log("CALLERIDNAME " + c, verbosity.HIGH);
                        c = call_info.get_CallInfoString(CALLINFO_STRING.CIS_CALLERIDNUMBER);
                        log("CALLERIDNUMBER " + c, verbosity.HIGH);
                    }
                    catch (Exception ex)
                    {
                        log("Exception Catched: " + ex.ToString(), verbosity.HIGH);
                    }

                    break;
            }

            /*
            log("CALLINFO_STRING:");
            foreach (string name in Enum.GetNames(typeof(CALLINFO_STRING)))
            {
                log(name + ":");
                try
                {
                    CALLINFO_STRING cis = (CALLINFO_STRING)Enum.Parse(typeof(CALLINFO_STRING), name);
                    log("" + call_info.get_CallInfoString(cis));
                }
                catch (Exception ex)
                {
                    log("Exception Catched: " + ex.ToString());
                }
            }
            */
            /*
            log("CALLINFO_LONG:");
            foreach (string name in Enum.GetNames(typeof(CALLINFO_LONG)))
            {
                log(name + ":");
                try
                {
                    CALLINFO_LONG cil = (CALLINFO_LONG)Enum.Parse(typeof(CALLINFO_LONG), name);
                    log("" + call_info.get_CallInfoLong(cil));
                }
                catch (Exception ex)
                {
                    log("Exception Catched: " + ex.ToString());
                }
            }
             */
            if (call_state_event != null & call_info != null)
                CallEventMethod(call_info, call_info.CallState);
        }

        /// <summary>
        /// Registers an address line for tapi call events.
        /// </summary>
        /// <param name="iTAddress">ITAddress interface of the line to be registered.</param>
        /// <returns>True if the method succeeds.</returns>
        private bool RegisterLineForIncomingCalls(ITAddress iTAddress)
        {
            try
            {
                int registrationToken = this.tapiObject.RegisterCallNotifications(iTAddress, true, true, TapiConstants.TAPIMEDIATYPE_AUDIO, 1);
                this.registrationTokens.Add(registrationToken);
            }
            catch(Exception ex) 
            {
                log("RegisterLineForIncomingCalls: iTAddress.AddressName=" + iTAddress.AddressName +" Exception: "+ex.ToString(), verbosity.DEBUG);
                return false;
            }

            log("Registered on line: " + iTAddress.AddressName,verbosity.MEDIUM);
            return true;
        }

        #endregion

    }

}
