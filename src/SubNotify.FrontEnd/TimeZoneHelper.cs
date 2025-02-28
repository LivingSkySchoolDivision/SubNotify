namespace SubNotify.FrontEnd;

public class TimeZoneHelper
{
    
    public static DateTime ConvertUTCToLocalTime(DateTime UTCTime, TimeZoneInfo Timezone) 
    {
        // Construct a new DateTime object that we set in UTC, because c# timezone stuff behaves very strangely if the local timezone is UTC
        DateTime fakeUTCTime = new DateTime(
            UTCTime.Year,
            UTCTime.Month,
            UTCTime.Day,
            UTCTime.Hour,
            UTCTime.Minute,
            UTCTime.Second,
            DateTimeKind.Utc
        );

        // This fucking converts the wrong way for shit's sake.

        DateTime convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(fakeUTCTime, Timezone);

        return convertedDateTime;
    }

    public static DateTime ConvertUTCToLocalTime(DateTime UTCTime, string TimeZoneString) 
    {
        if (!string.IsNullOrEmpty(TimeZoneString)) 
        {
            try {
                TimeZoneInfo parsedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneString);
                return ConvertUTCToLocalTime(UTCTime, parsedTimeZone);
            }         
            catch {} // Will fall through to the exception below
        } 
        
        throw new Exception("Could not parse timezone");        
    }
}
