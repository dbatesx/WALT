using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
     [Serializable()]
    public class Alert : Object
    {
        public enum AlertEnum { TASK, WEEKLY_TASK, BARRIER, SYSTEM };
        public enum ColumnEnum
        {
            PROFILE, CREATOR, ENTRYDATE, SUBJECT, MESSAGE, LINKEDTYPE, ACKNOWLEDGED
        }

        public Profile Profile { get; set; }
        public Profile Creator { get; set; }
        public DateTime EntryDate { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public long LinkedId { get; set; }
        public AlertEnum? LinkedType { get; set; }
        public bool Acknowledged { get; set; }

        public long ID
        {
            get { return Id; }
        }

        public string Sender
        {
            get { return Creator.DisplayName; }
        }

        public string SentTo
        {
            get { return Profile.DisplayName; }
        }

        public string Type
        {
            get
            {
                if (LinkedType.HasValue)
                {
                    if (LinkedType.Value == AlertEnum.WEEKLY_TASK)
                    {
                        return "Task";
                    }

                    string type = LinkedType.Value.ToString();
                    return type[0] + type.Substring(1).ToLower();
                }

                return string.Empty;
            }
        }
    }
}
