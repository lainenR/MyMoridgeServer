using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.BusinessLogic
{
    public class TimeScheduler
    {
        private int UTCOffset;

        public TimeScheduler(DateTime dateForUTCOffset)
        {
            UTCOffset = Common.GetSwedishDateTimeOffsetFromUTC(dateForUTCOffset);
        }

        public TimeSpan GetMorningStartTimeSwedishTimeCompensateUTC()
        {
            return new TimeSpan(TimeSchedule.MorningStartHour - UTCOffset, 0, 0);
        }

        public TimeSpan GetMorningEndTimeSwedishTimeCompensateUTC()
        {
            return new TimeSpan(TimeSchedule.MorningEndHour - UTCOffset, 0, 0);
        }

        public TimeSpan GetAfterLunchStartTimeSwedishTimeCompensateUTC()
        {
            return new TimeSpan(TimeSchedule.AfterLunchStartHour - UTCOffset, 0, 0);
        }

        public TimeSpan GetAfterLunchEndTimeSwedishTimeCompensateUTC()
        {
            return new TimeSpan(TimeSchedule.AfterLunchEndHour - UTCOffset, 0, 0);
        }
    }
}