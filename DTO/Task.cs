using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{
    [Serializable()]
    public class Task : Object
    {
        public enum StatusEnum
        {
            OPEN, COMPLETED, HOLD, REJECTED, OBE
        }

        public enum ColumnEnum
        {
            NONE, OWNER, ASSIGNED, TITLE, TASKTYPE, SOURCE, SOURCEID, STARTDATE,
            DUEDATE, STATUS, HOURS, ESTIMATE, COMPLEXITY, SPENT, PROGRAM, COMPLETEDDATE,
            INSTANTIATED, BASETASK, EXITCRITERIA, WBS, OWNERCOMMENTS, ASSIGNEECOMMENTS, CREATED, ONHOLDDATE, ORIGINATOR
        }

        public Profile Owner { get; set; }
        public Profile Assigned { get; set; }
        public Profile Originator { get; set; }
        public string Title { get; set; }
        public TaskType TaskType { get; set; }
        public string Source { get; set; }
        public string SourceID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public StatusEnum Status { get; set; }
        public double Hours { get; set; }
        public double Estimate { get; set; }
        public Complexity Complexity { get; set; }
        public double Spent { get; set; }
        public Program Program { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool Instantiated { get; set; }
        public bool BaseTask { get; set; }
        public string ExitCriteria { get; set; }
        public string WBS { get; set; }
        public string OwnerComments { get; set; }
        public string AssigneeComments { get; set; }
        public DateTime Created { get; set; }
        public DateTime? OnHoldDate { get; set; }
        public bool FullyAllocated { get; set; }
        public double HoursAllocatedToChildren { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool Deleted { get; set; }
        public List<string> ErrorMessage { get; set; }
       
        public List<Task> Children { get; set; }

        /// <summary>
        /// Constructor instatiates an object for each collection.
        /// </summary>
        public Task()
        {
            Children = new List<Task>();
            Status = StatusEnum.OPEN;
        }

        public int GetSize()
        {
            int size = sizeof(long);
            size += Owner.GetSize();
            size += Assigned.GetSize();
            size += Title.Length;
            size += TaskType != null ? TaskType.GetSize() : 0;
            size += Source != null ? Source.Length : 0;
            size += SourceID != null ? SourceID.Length : 0;
            size += sizeof(long);
            size += sizeof(long);
            size += sizeof(StatusEnum);
            size += sizeof(double);
            size += sizeof(double);
            size += Complexity != null ? Complexity.GetSize() : 0;
            size += sizeof(double);
            size += Program != null ? Program.GetSize() : 0;
            size += sizeof(long);
            size += sizeof(bool);
            size += sizeof(bool);
            size += ExitCriteria != null ? ExitCriteria.Length : 0;
            size += OwnerComments != null ? OwnerComments.Length : 0;
            size += AssigneeComments != null ? AssigneeComments.Length : 0;

            foreach (Task t in Children)
            {
                size += t.GetSize();
            }

            return size;
        }

        public long ID
        {
            get { return Id; }
        }

        public string OriginatorDisplayName
        {
            get { return Originator != null ? Originator.DisplayName : ""; }
        }

        public string OwnerDisplayName
        {
            get { return Owner != null ? Owner.DisplayName : ""; }
        }

        public string AssigneeDisplayName
        {
            get { return Assigned != null ? Assigned.DisplayName : ""; }
        }

        public string ProgramTitle
        {
            get { return Program != null ? Program.Title : ""; }
        }

        public string TaskTypeTitle
        {
            get { return TaskType != null ? TaskType.Title : ""; }
        }

        public string ComplexityTitle
        {
            get { return Complexity != null ? Complexity.Title : ""; }
        }

        private bool _error = false;
        public bool Error
        {
            get { return _error; }
            set { _error = value; }
        }
    }
}
