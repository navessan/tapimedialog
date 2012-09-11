using System;
using System.Collections.Generic;
using System.Text;

namespace C4F.DevKit.Telephony
{
    public enum verbosity
    {
        /// <summary>
        /// NONE - Syslog Only if Selected
        /// </summary>
        NONE = 0,
        /// <summary>
        /// LOW - Statistics and Errors
        /// </summary>
        LOW = 1,
        /// <summary>
        /// MEDIUM - Statistics, Errors and Results
        /// </summary>
        MEDIUM = 2,
        /// <summary>
        /// HIGH - Statistics, Errors, Results and Major I/O Events
        /// </summary>
        HIGH = 3,
        /// <summary>
        /// DEBUG - Statistics, Errors, Results, I/O and Program Flow
        /// </summary>
        DEBUG = 4,
        /// <summary>
        /// DEVEL - Developer DEBUG Level
        /// </summary>
        DEVDBG = 5
    }

    /// <summary>
    /// Represents the call direction.
    /// </summary>
    public enum CallDirection
    {

        /// <summary>
        /// Incoming call.
        /// </summary>
        Incoming,

        /// <summary>
        /// Outgoing Call.
        /// </summary>
        Outgoing,

    }

    /// <summary>
    /// Represents the state of a call.
    /// </summary>
    public enum CallState
    {

        /// <summary>
        /// The call has been created, but Connect has not been called yet. This is the initial state for 
        /// both incoming and outgoing calls.
        /// </summary>
        Idle = 0,

        /// <summary>
        /// Connect has been called, and the service provider is working on making a connection. 
        /// This state is valid only on outgoing calls. 
        /// This message is optional, because a service provider may have a call transition directly to the connected state.
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// Call has been connected to the remote end and communication can take place.
        /// </summary>
        Connected = 2,

        /// <summary>
        /// Call has been disconnected.
        /// </summary>
        Disconnected = 3,

        /// <summary>
        /// A new call has appeared, and is being offered to an application.
        /// </summary>
        Offering = 4,

        /// <summary>
        /// The call is in the hold state.
        /// </summary>
        Hold = 5,

        /// <summary>
        /// The call is queued.
        /// </summary>
        Queued = 6,

    }

    /// <summary>
    /// Represents media types supported by the line.
    /// </summary>
    [Flags]
    public enum TapiMediaType
    {

        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents an audio media stream that is entering or leaving the computer.
        /// </summary>
        Audio = 8,

        /// <summary>
        /// Represents a data media stream that is associated with a data modem.
        /// </summary>
        DataModem = 16,

        /// <summary>
        /// Represents a data media stream that is associated with a G3 protocol fax.
        /// </summary>
        G3Fax = 32,

        /// <summary>
        /// Represent a stream which is on a multitrack
        /// </summary>
        MultiTrack = 65536,

        /// <summary>
        /// Represents a video media stream that is entering or leaving the computer.
        /// </summary>
        Video = 32768

    }

}
