using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
     [Serializable()]
    public class TaskType : Object 
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Complexity> Complexities { get; set; }
        public List<TaskType> Children { get; set; }
        public bool Active { get; set; }
        public long TeamId { get; set; }
        public string ParentTitle { get; set; }
        public bool Error { get; set; }
        public List<string> ErrorMessage { get; set; }

        public string OutputErrorMessages
        {
            get
            {
                string errors = string.Empty;

                foreach (string msg in ErrorMessage)
                {
                    errors += msg + "<br />";
                }

                return errors;
            }
        }

        public string Status
        {
            get
            {
                return Error ? "Fail" : "Pass";
            }
        }

        public string Level1
        {
            get
            {
                return GetLevelHours("Level 1");
            }
        }

        public string Level2
        {
            get
            {
                return GetLevelHours("Level 2");
            }
        }

        public string Level3
        {
            get
            {
                return GetLevelHours("Level 3");
            }
        }

        public string Level4
        {
            get
            {
                return GetLevelHours("Level 4");
            }
        }

        public string Level5
        {
            get
            {
                return GetLevelHours("Level 5");
            }
        }

        public string Level6
        {
            get
            {
                return GetLevelHours("Level 6");
            }
        }

        private string GetLevelHours(string level)
        {
            foreach (Complexity comp in Complexities)
            {
                if (comp.Title == level)
                {
                    return comp.Hours.ToString();
                }
            }

            return string.Empty;
        }

        public TaskType()
        {
            Complexities = new List<Complexity>();
            Children = new List<TaskType>();
            ErrorMessage = new List<string>();
        }

        public int GetSize()
        {
            int size = sizeof(long);
            size += Title.Length;
            size += Description != null ? Description.Length : 0;

            foreach (Complexity c in Complexities)
            {
                size += c.GetSize();
            }

            foreach (TaskType t in Children)
            {
                size += t.GetSize();
            }

            size += sizeof(bool);

            return size;
        }
    }
}
