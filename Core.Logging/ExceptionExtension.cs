﻿using System;

namespace Core.Logging
{
    public static class ExceptionExtension
    {
        /// <summary>
        /// Gets the reference ID, a unique identifier for the error. 
        /// </summary>
        /// <remarks>This function has a side effect of updating the data collection
        /// on each exception in the chain.</remarks>
        /// <returns></returns>
        public static Guid GetReferenceID(this Exception ex)
        {
            const string refID = "CorrelationId";
            Guid ret;

            if (ex == null)
            {
                //Lowest level in recursive function, at no point did
                //this exception have a reference id, so create one here and bubble it
                //back up the call stack.
                return Guid.NewGuid();
            }

            if (ex.Data.Contains(refID))
            {
                //Short cut, found the reference id, we are done no need to call
                //recursively to get the reference id.
                ret = (Guid)ex.Data[refID];
            }
            else
            {
                //Recursively call down the inner exceptions to see if we
                //have a reference id in one of the inner exceptions.
                ret = GetReferenceID(ex.InnerException);
                ex.Data.Add(refID, ret);
            }
            return ret;
        }
    }
}
