﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DB.Entities.UsageReports
{
    /// <summary>
    /// What a user has been upto in Outlook overrall, on a given date
    /// https://docs.microsoft.com/en-us/graph/api/reportroot-getemailactivityuserdetail?view=graph-rest-beta
    /// </summary>
    [Table("outlook_user_activity_log")]
    public class OutlookUsageActivityLog : UserRelatedAbstractUsageActivity
    {

        [Column("email_send_count")]
        public long SendCount { get; set; }


        [Column("email_receive_count")]
        public long ReceiveCount { get; set; }


        [Column("email_read_count")]
        public long ReadCount { get; set; }

        [Column("meeting_created_count")]
        public long MeetingsCreated { get; set; }

        [Column("meeting_interacted_count")]
        public long MeetingsInteracted { get; set; }

    }
}
