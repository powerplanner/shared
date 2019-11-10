using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerSending.Helpers
{
    public static class WeekHelper
    {
        /// <summary>
        /// Assumes both values are UTC. Returns which week the specified date is on
        /// </summary>
        /// <param name="weekOneStartsOn"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static Schedule.Week GetWeekOnDifferentDate(DateTime weekOneStartsOn, DateTime date)
        {
            int days;

            if (date.Date >= weekOneStartsOn.Date)
                days = (date.Date - weekOneStartsOn.Date).Days;

            else
                days = (weekOneStartsOn.Date - date.Date).Days + 6;

            if (days / 7 % 2 == 0)
            {
                return Schedule.Week.WeekOne;
            }

            else
            {
                return Schedule.Week.WeekTwo;
            }
        }
    }
}
