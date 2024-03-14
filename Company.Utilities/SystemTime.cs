using System;
using System.Collections.Generic;
using System.Text;


namespace Company.Utilities
{
    //Source: http://stackoverflow.com/questions/3116736/unit-testing-a-time-based-component
    //
    // Usage for application:
    //  DateTime now = SystemTime.Now;
    //
    // Usage for tests:
    // DateTime expectedDate = new DateTime(2010, 6, 25);
    // SystemTime.SetDateTime(expectedDate);
    // Assert.AreEqual<DateTime>(expectedDate, SystemTime.Now); 

    public static class SystemTime
    {
        public static Func<DateTime> DateProvider = () => DateTime.Now;

        public static DateTime Now
        {
            get
            {
                return DateProvider();
            }
        }

        public static DateTime Today
        {
            get
            {
                return Now.Date;
            }
        }

        public static DateTime UtcNow
        {
            get
            {
                return Now.ToUniversalTime();
            }
        }

        public static void SetDateTime(int year, int month, int day)
        {
            SetDateTime(new DateTime(year, month, day));
        }

        public static void SetDateTime(DateTime dateTime)
        {
            DateProvider = () => dateTime;
        }

        public static void Reset()
        {
            DateProvider = () => DateTime.Now;
        }
    }
}
