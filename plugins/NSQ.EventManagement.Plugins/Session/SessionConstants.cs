namespace NSQ.EventManagement.Plugins.Session
{
    internal static class SessionConstants
    {
        public const string StatusField = "hpst_status";      // verify logical name
        public const string EndTimeField = "hpst_endtime";

        // Verify these against your Status column's choices
        public const int StatusDraft = 744890000;
        public const int StatusPlanned = 744890001;
        public const int StatusCompleted = 744890002;
    }
}
