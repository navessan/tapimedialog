using System;
using System.Collections.Generic;
using System.Text;

namespace C4F.DevKit.Telephony
{

    /// <summary>
    /// Represents a communication line.
    /// </summary>
    public class Line
    {
        #region Internal Members

        /// <summary>
        /// Name of the line.
        /// </summary>
        internal string lineName;

        /// <summary>
        /// Media types supported by the line.
        /// </summary>
        internal TapiMediaType supportedMediaTypes;

        /// <summary>
        /// Indicates whether DTMF is supported by the line.
        /// </summary>
        internal bool isDtmfSupported;

        /// <summary>
        /// Maximum number of calls that can be on hold. If 0 then call hold is not supported on the line.
        /// </summary>
        internal int maxCallsOnHold;

        #endregion

        #region Public Properties

        /// <summary>
        /// Name of the line.
        /// </summary>
        public string LineName
        {
            
            get { return this.lineName; }

        }

        /// <summary>
        /// Media types supported by the line.
        /// </summary>
        public TapiMediaType SupportedMediaTypes
        {
            
            get { return this.supportedMediaTypes; }

        }

        /// <summary>
        /// Indicates whether DTMF is supported by the line.
        /// </summary>
        public bool IsDtmfSupported
        {

            get { return this.isDtmfSupported; }

        }
        /// <summary>
        /// Maximum number of calls that can be on hold. If 0 then call hold is not supported on the line.
        /// </summary>
        public int MaxCallsOnHold
        {

            get { return this.maxCallsOnHold; }

        }

        #endregion

    }

}
