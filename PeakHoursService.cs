namespace ClaudeLight;

/// <summary>
/// Determines peak/off-peak status based on Anthropic's rate limit schedule.
/// Peak hours: Mon–Fri, 5:00 AM – 11:00 AM Pacific Time.
/// Weekends are always off-peak.
/// </summary>
sealed class PeakHoursService
{
    // Anthropic peak window in ET (5–11 AM PT = 8 AM–2 PM ET)
    private const int PeakStartHour = 8;
    private const int PeakEndHour   = 14;

    private static readonly TimeZoneInfo PacificTZ =
        GetEasternTimeZone();

    public bool   IsPeak           { get; private set; }
    public string StatusText       { get; private set; } = "";
    public string PacificTimeText  { get; private set; } = "";
    public TimeSpan TimeUntilChange { get; private set; }

    // Fired whenever IsPeak, StatusText, or PacificTimeText changes
    public event EventHandler? StateChanged;

    // Fired only on peak → off-peak transition
    public event EventHandler? OffPeakStarted;

    private bool? _previousIsPeak;

    public void Update()
    {
        var utcNow  = DateTime.UtcNow;
        var etNow   = TimeZoneInfo.ConvertTimeFromUtc(utcNow, PacificTZ);
        var weekday = etNow.DayOfWeek;

        bool isWeekday     = weekday >= DayOfWeek.Monday && weekday <= DayOfWeek.Friday;
        bool inPeakWindow  = etNow.Hour >= PeakStartHour && etNow.Hour < PeakEndHour;
        bool newIsPeak     = isWeekday && inPeakWindow;

        TimeUntilChange = ComputeTimeUntilChange(etNow, weekday, isWeekday, newIsPeak);

        int h = (int)TimeUntilChange.TotalHours;
        int m = TimeUntilChange.Minutes;

        IsPeak          = newIsPeak;
        StatusText      = newIsPeak
            ? $"Off-peak in {h}h {m}m"
            : $"Peak starts in {h}h {m}m";
        PacificTimeText = etNow.ToString("h:mm tt") + " ET";

        // Transition detection: peak → off-peak
        if (_previousIsPeak == true && !newIsPeak)
            OffPeakStarted?.Invoke(this, EventArgs.Empty);

        _previousIsPeak = newIsPeak;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    // -------------------------------------------------------------------------
    // Time-until-change calculation
    // -------------------------------------------------------------------------

    private static TimeSpan ComputeTimeUntilChange(
        DateTime ptNow, DayOfWeek weekday, bool isWeekday, bool currentlyPeak)
    {
        if (currentlyPeak)
            return TimeUntil(ptNow, PeakEndHour);

        if (isWeekday && ptNow.Hour < PeakStartHour)
            return TimeUntil(ptNow, PeakStartHour);

        // Weekend or after peak on a weekday — find next weekday morning
        DateTime nextWeekdayStart = NextWeekdayDate(ptNow, weekday, isWeekday);
        return TimeUntilDate(ptNow, nextWeekdayStart.Date.AddHours(PeakStartHour));
    }

    private static TimeSpan TimeUntil(DateTime now, int targetHour)
    {
        var target = now.Date.AddHours(targetHour);
        var diff   = target - now;
        return diff < TimeSpan.Zero ? TimeSpan.Zero : diff;
    }

    private static TimeSpan TimeUntilDate(DateTime now, DateTime target)
    {
        var diff = target - now;
        return diff < TimeSpan.Zero ? TimeSpan.Zero : diff;
    }

    private static DateTime NextWeekdayDate(DateTime ptNow, DayOfWeek weekday, bool isWeekday)
    {
        int daysToAdd = weekday switch
        {
            DayOfWeek.Friday   when isWeekday => 3, // Fri after peak → Mon
            DayOfWeek.Saturday               => 2, // Sat → Mon
            DayOfWeek.Sunday                 => 1, // Sun → Mon
            _                                => 1, // Any other weekday after peak → next day
        };
        return ptNow.Date.AddDays(daysToAdd);
    }

    // -------------------------------------------------------------------------
    // Timezone helper — handles both Windows and IANA identifiers
    // -------------------------------------------------------------------------

    private static TimeZoneInfo GetEasternTimeZone()
    {
        // Windows identifier
        try { return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"); }
        catch { /* fall through */ }

        // IANA identifier
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/New_York"); }
        catch { /* fall through */ }

        // Hard fallback: UTC-5 (no DST)
        return TimeZoneInfo.CreateCustomTimeZone(
            "ET-Fallback", TimeSpan.FromHours(-5), "Eastern (fallback)", "Eastern (fallback)");
    }
}
