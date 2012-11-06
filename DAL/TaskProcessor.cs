using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WALT.DTO;
using System.Collections;
using System.Data.OleDb;
using System.Data;
using System.Reflection;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Configuration;
using System.DirectoryServices;

namespace WALT.DAL
{
    public class TaskProcessor : Processor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        public TaskProcessor(Mediator mediator)
            : base(mediator)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="status"></param>
        /// <param name="order"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<DTO.Task> GetTaskList(DTO.Profile assigned, DTO.Profile owner, Task.StatusEnum? status,
            Task.ColumnEnum? sort, bool order, int start, int size, bool recursive, ref int count,
            Dictionary<Task.ColumnEnum, string> filters)
        {
            if (assigned == null && owner == null)
            {
                throw new Exception("Either an assigned or owner profile is required to get a task list.");
            }

            List<DTO.Task> tasks = new List<DTO.Task>();

            // Get all records for the profile or owner

            IQueryable<task> query1 = null;

            if (assigned != null)
            {
                if (!status.HasValue)
                {
                    query1 = from item in _db.GetContext().tasks
                             where item.profile_id == assigned.Id && item.deleted == false
                             select item;
                }
                else
                {
                    query1 = from item in _db.GetContext().tasks
                             where item.profile_id == assigned.Id && item.status == status.Value.ToString() && item.deleted == false
                             select item;
                }
            }
            else
            {
                if (!status.HasValue)
                {
                    query1 = from item in _db.GetContext().tasks
                             where item.owner_id == owner.Id && item.deleted == false
                             select item;
                }
                else
                {
                    query1 = from item in _db.GetContext().tasks
                             where item.owner_id == owner.Id && item.status == status.Value.ToString() && item.deleted == false
                             select item;
                }
            }

            // Apply filters

            IQueryable<task> query2 = null;

            if (filters != null)
            {
                var predicate = PredicateBuilder.True<task>();

                foreach (Task.ColumnEnum k in filters.Keys)
                {
                    string filter = filters[k];

                    switch (k)
                    {
                        case Task.ColumnEnum.INSTANTIATED:                            
                            predicate = predicate.And(x => x.instantiated == bool.Parse(filter.ToUpper()));
                            break;

                        case Task.ColumnEnum.ASSIGNED:
                            predicate = predicate.And(x => x.profile1.display_name.ToUpper().Contains(filter.ToUpper()));
                            break;

                        case Task.ColumnEnum.COMPLEXITY:
                            predicate = predicate.And(x => x.complexity.title.ToUpper().Contains(filter.ToUpper()));
                            break;

                        case Task.ColumnEnum.OWNER:
                            predicate = predicate.And(x => x.profile.display_name.ToUpper().Contains(filter.ToUpper()));
                            break;

                        case Task.ColumnEnum.PROGRAM:
                            predicate = predicate.And(x => x.program.id == Convert.ToInt64(filter));
                            break;

                        case Task.ColumnEnum.TASKTYPE:
                            predicate = predicate.And(x => x.task_type.title.ToUpper().Contains(filter.ToUpper()));
                            break;

                        case Task.ColumnEnum.TITLE:
                            predicate = predicate.And(x => x.title.ToUpper().Contains(filter.ToUpper()));
                            break;

                        case Task.ColumnEnum.SOURCE:
                            predicate = predicate.And(x => x.source.ToUpper().Contains(filter.ToUpper()));
                            break;

                        case Task.ColumnEnum.STARTDATE:
                            {
                                string[] split = filter.Split(',');

                                for (int i = 0; i < split.Count(); i++)
                                {
                                    DateTime d = split[i].Substring(1) == ">" || split[i].Substring(1) == "<" ?
                                        Convert.ToDateTime(split[i].Substring(1)) : Convert.ToDateTime(split[i].Substring(1));

                                    if (split[i].StartsWith("<"))
                                    {
                                        predicate = predicate.And(x => x.start_date < d);
                                    }
                                    else if (split[i].StartsWith(">"))
                                    {
                                        predicate = predicate.And(x => x.start_date > d);
                                    }
                                    else
                                    {
                                        predicate = predicate.And(x => x.start_date == d);
                                    }
                                }
                            }
                            break;

                        case Task.ColumnEnum.DUEDATE:
                            {
                                string[] split = filter.Split(',');

                                for (int i = 0; i < split.Count(); i++)
                                {
                                    DateTime d = split[i].Substring(1) == ">" || split[i].Substring(1) == "<" ?
                                        Convert.ToDateTime(split[i].Substring(1)) : Convert.ToDateTime(split[i].Substring(1));

                                    if (split[i].StartsWith("<"))
                                    {
                                        predicate = predicate.And(x => x.due_date < d);
                                    }
                                    else if (split[i].StartsWith(">"))
                                    {
                                        predicate = predicate.And(x => x.due_date > d);
                                    }
                                    else
                                    {
                                        predicate = predicate.And(x => x.due_date == d);
                                    }
                                }
                            }
                            break;

                        case Task.ColumnEnum.STATUS:
                            predicate = predicate.And(x => x.status.ToUpper().Contains(filter.ToUpper()));
                            break;

                        case Task.ColumnEnum.HOURS:
                            {
                                if (filter.StartsWith("<"))
                                {
                                    predicate = predicate.And(x => x.hours < double.Parse(filter.Substring(1)));
                                }
                                else if (filter.StartsWith(">"))
                                {
                                    predicate = predicate.And(x => x.hours > double.Parse(filter.Substring(1)));
                                }
                                else if (filter.StartsWith("="))
                                {
                                    predicate = predicate.And(x => x.hours == double.Parse(filter.Substring(1)));
                                }
                                else
                                {
                                    predicate = predicate.And(x => x.hours < double.Parse(filter.Substring(1)));
                                }
                            }
                            break;

                        case Task.ColumnEnum.ESTIMATE:
                            {
                                if (filter.StartsWith("<"))
                                {
                                    predicate = predicate.And(x => x.estimate < double.Parse(filter.Substring(1)));
                                }
                                else if (filter.StartsWith(">"))
                                {
                                    predicate = predicate.And(x => x.estimate > double.Parse(filter.Substring(1)));
                                }
                                else if (filter.StartsWith("="))
                                {
                                    predicate = predicate.And(x => x.estimate == double.Parse(filter.Substring(1)));
                                }
                                else
                                {
                                    predicate = predicate.And(x => x.estimate < double.Parse(filter.Substring(1)));
                                }
                            }
                            break;

                        case Task.ColumnEnum.COMPLETEDDATE:
                            {
                                string[] split = filter.Split(',');

                                for (int i = 0; i < split.Count(); i++)
                                {
                                    DateTime d = split[i].Substring(1) == ">" || split[i].Substring(1) == "<" ?
                                        Convert.ToDateTime(split[i].Substring(1)) : Convert.ToDateTime(split[i].Substring(1));

                                    if (split[i].StartsWith("<"))
                                    {
                                        predicate = predicate.And(x => x.completed_date < d);
                                    }
                                    else if (split[i].StartsWith(">"))
                                    {
                                        predicate = predicate.And(x => x.completed_date > d);
                                    }
                                    else
                                    {
                                        predicate = predicate.And(x => x.completed_date == d);
                                    }
                                }
                            }
                            break;

                        case Task.ColumnEnum.CREATED:
                            {
                                string[] split = filter.Split(',');

                                for (int i = 0; i < split.Count(); i++)
                                {
                                    DateTime d = split[i].Substring(1) == ">" || split[i].Substring(1) == "<" ?
                                        Convert.ToDateTime(split[i].Substring(1)) : Convert.ToDateTime(split[i].Substring(1));

                                    if (split[i].StartsWith("<"))
                                    {
                                        predicate = predicate.And(x => x.created < d);
                                    }
                                    else if (split[i].StartsWith(">"))
                                    {
                                        predicate = predicate.And(x => x.created > d);
                                    }
                                    else
                                    {
                                        predicate = predicate.And(x => x.created == d);
                                    }
                                }
                            }
                            break;

                        case Task.ColumnEnum.EXITCRITERIA:
                            predicate = predicate.And(x => x.exit_criteria.ToUpper().Contains(filter.ToUpper()));
                            break;

                        case Task.ColumnEnum.WBS:
                            predicate = predicate.And(x => x.wbs.ToUpper().Contains(filter.ToUpper()));
                            break;

                        case Task.ColumnEnum.OWNERCOMMENTS:
                            predicate = predicate.And(x => x.owner_comments.Contains(filter));
                            break;

                        case Task.ColumnEnum.ASSIGNEECOMMENTS:
                            predicate = predicate.And(x => x.assignee_comments.Contains(filter));
                            break;
                    }
                }

                query2 = query1.AsQueryable().Where(predicate);
            }
            else
            {
                query2 = query1;
            }

            // If there is a sort column then apply

            IQueryable<task> query3 = null;

            if (sort.HasValue)
            {
                switch (sort)
                {
                    case Task.ColumnEnum.INSTANTIATED:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.instantiated);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.instantiated);
                        }
                        break;

                    case Task.ColumnEnum.ASSIGNED:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.profile2.display_name); 
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.profile2.display_name);
                        }
                        break;

                    case Task.ColumnEnum.COMPLEXITY:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.complexity.title);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.complexity.title);
                        }
                        break;

                    case Task.ColumnEnum.OWNER:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.profile1.display_name);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.profile1.display_name);
                        }
                        break;

                    case Task.ColumnEnum.ORIGINATOR:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.profile.display_name);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.profile.display_name);
                        }
                        break;

                    case Task.ColumnEnum.PROGRAM:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.program.title);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.program.title);
                        }
                        break;

                    case Task.ColumnEnum.TASKTYPE:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.task_type.title);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.task_type.title);
                        }
                        break;

                    case Task.ColumnEnum.TITLE:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.title);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.title);
                        }
                        break;

                    case Task.ColumnEnum.SOURCE:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.source);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.source);
                        }
                        break;

                    case Task.ColumnEnum.SOURCEID:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.source_id);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.source_id);
                        }
                        break;

                    case Task.ColumnEnum.STARTDATE:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.start_date);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.start_date);
                        }
                        break;

                    case Task.ColumnEnum.DUEDATE:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.due_date);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.due_date);
                        }
                        break;

                    case Task.ColumnEnum.STATUS:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.status);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.status);
                        }
                        break;

                    case Task.ColumnEnum.HOURS:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.hours);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.hours);
                        }
                        break;

                    case Task.ColumnEnum.ESTIMATE:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.estimate);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.estimate);
                        }
                        break;

                    case Task.ColumnEnum.COMPLETEDDATE:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.completed_date);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.completed_date);
                        }
                        break;

                    case Task.ColumnEnum.EXITCRITERIA:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.exit_criteria);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.exit_criteria);
                        }
                        break;

                    case Task.ColumnEnum.WBS:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.wbs);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.wbs);
                        }
                        break;

                    case Task.ColumnEnum.OWNERCOMMENTS:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.owner_comments);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.owner_comments);
                        }
                        break;

                    case Task.ColumnEnum.ASSIGNEECOMMENTS:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.assignee_comments);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.assignee_comments);
                        }
                        break;

                    case Task.ColumnEnum.CREATED:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.created);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.created);
                        }
                        break;

                    case Task.ColumnEnum.ONHOLDDATE:
                        if (order == true)
                        {
                            query3 = query2.OrderBy(x => x.on_hold_date);
                        }
                        else
                        {
                            query3 = query2.OrderByDescending(x => x.on_hold_date);
                        }
                        break;

                    default:
                        query3 = query2;
                        break;
                }
            }
            else
            {
                query3 = query2;
            }

            IQueryable<task> query4 = null;

            count = 0;

            // Only grab records for the specified page

            if (size > 0)
            {
                count = query3.Count();
                query4 = query3.Skip(start).Take(size);
            }
            else
            {
                query4 = query3;
            }

            // Create the DTOs

            foreach (task r in query4)
            {
                tasks.Add(CreateTaskDTO(r, recursive));
            }

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<DTO.Task> GetTaskList(DTO.Profile profile, Task.StatusEnum? status)
        {
            int count = 0;
            return GetTaskList(profile, null, status, Task.ColumnEnum.NONE, true, 0, 0, false, ref count, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<DTO.Task> GetTaskList(DTO.Profile profile, Task.StatusEnum? status, Task.ColumnEnum col, bool ascending)
        {
            int count = 0;
            return GetTaskList(profile, null, status, col, ascending, 0, 0, false, ref count, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public Dictionary<long, string> GetParentableTaskList(Profile assignee, Task currTask)
        {
            Dictionary<long, string> tasks = new Dictionary<long, string>();
            
            var spent = from t in _db.GetContext().tasks                        
                        where t.status == Task.StatusEnum.OPEN.ToString() &&
                        t.profile_id == assignee.Id && t.deleted == false && 
                        t.id != currTask.Id &&
                        _db.GetContext().weekly_tasks.Count(x => x.task_id == t.id) == 0
                        orderby t.title
                        select t;

            foreach (task task in spent) 
            {
                tasks.Add(task.id, task.title);
            }

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<DTO.Task> GetTaskListByOwner(DTO.Profile owner, Task.StatusEnum? status)
        {
            List<DTO.Task> tasks = new List<DTO.Task>();
            IQueryable<task> query;

            if (status.HasValue)
            {
                query = from item in _db.GetContext().tasks
                        where item.status == status.Value.ToString() &&
                            item.owner_id == owner.Id && item.deleted == false
                        select item;
            }
            else
            {
                query = from item in _db.GetContext().tasks
                        where item.owner_id == owner.Id && item.deleted == false
                        select item;
            }

            foreach (task r in query)
            {
                tasks.Add(CreateTaskDTO(r, false));
            }

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="childTask"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public DTO.Task GetRootTask(DTO.Task childTask, DTO.Profile profile)
        {
            DTO.Task rootTask = childTask;

            if (childTask != null && profile != null)
            {
                rootTask = GetTask(FindRootTaskID(childTask.Id, profile), profile, true, true);
            }

            return rootTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private long FindRootTaskID(long id, DTO.Profile profile)
        {
            long rootId = id;

            task t = _db.GetContext().tasks.SingleOrDefault(x => x.id == id);

            if (t != null)
            {
                if (t.owner_id == profile.Id ||
                    t.profile_id == profile.Id ||
                    CanViewTask(profile.Id, t.owner_id) ||
                    CanViewTask(profile.Id, t.profile_id))
                {
                    if (t.parent_id == null || t.parent_id == t.id || t.parent_id == 0)
                    {
                        rootId = t.id;
                    }
                    else
                    {
                        rootId = FindRootTaskID(t.parent_id.GetValueOrDefault(), profile);
                    }
                }
            }

            return rootId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tasksInPlan"></param>
        /// <returns></returns>
        public List<DTO.Task> GetUnplannedTaskList(Profile profile, List<long> weeklyTasksInPlan, List<long> tasksInPlan)
        {
            List<DTO.Task> tasks = new List<DTO.Task>();

            tasksInPlan.AddRange((from item in _db.GetContext().weekly_tasks
                                  where weeklyTasksInPlan.Contains(item.id)
                                  select item.task_id).ToList());

            var query = from item in _db.GetContext().tasks
                        where (item.status == "OPEN" || item.status == "HOLD") &&
                            !item.deleted && !tasksInPlan.Contains(item.id) && item.profile_id == profile.Id &&
                            _db.GetContext().tasks.Count(x => x.parent_id == item.id) == 0
                        orderby item.title
                        select item;

            foreach (task t in query)
            {
                tasks.Add(CreateTaskDTO(t, false));
            }

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="weekEnding"></param>
        /// <returns></returns>
        public List<DTO.Task> GetOpenTasksByWeek(long profileId, DateTime weekEnding)
        {
            List<DTO.Task> tasks = new List<Task>();
            var query = from wp in _db.GetContext().weekly_plans
                        join wt in _db.GetContext().weekly_tasks on wp.id equals wt.weekly_plan_id
                        join t in _db.GetContext().tasks on wt.task_id equals t.id
                        where wp.profile_id == profileId && wp.week_ending == weekEnding &&
                            (t.status == Task.StatusEnum.OPEN.ToString() || t.status == Task.StatusEnum.HOLD.ToString())
                        orderby t.title
                        select t;

            foreach (task t in query)
            {
                tasks.Add(CreateTaskDTO(t, null, false, true));
            }

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="loadTaskType"></param>
        /// <returns></returns>
        DTO.Complexity CreateComplexityDTO(complexity c, bool loadTaskType)
        {
            DTO.Complexity rec = null;

            if (c != null)
            {
                rec = new Complexity();
                rec.Id = c.id;
                rec.Title = c.title;
                rec.Hours = c.hours;
                rec.Active = c.active;

                if (loadTaskType)
                {
                    rec.TaskType = _mediator.GetAdminProcessor().CreateTaskTypeDTO(c.task_type, null);
                }
            }

            return rec;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Complexity GetComplexity(long id)
        {
            Complexity rec = null;
            complexity c = _db.GetContext().complexities.SingleOrDefault(x => x.id == id);

            if (c != null)
            {
                rec = CreateComplexityDTO(c, false);
            }

            return rec;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public List<Complexity> GetComplexityList(long teamID, bool active)
        {
            List<Complexity> Complexities = new List<Complexity>();

            var query = from item in _db.GetContext().complexities
                        where item.active == active && item.team_id == teamID
                        select item;

            foreach (complexity comp in query)
            {
                Complexities.Add(CreateComplexityDTO(comp, true));
            }

            return Complexities;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="taskTypeId"></param>
        /// <returns></returns>
        public List<Complexity> GetComplexityList(long teamId, long taskTypeId)
        {
            List<Complexity> Complexities = new List<Complexity>();

            var c = from item in _db.GetContext().complexities
                    where item.team_id == teamId && item.task_type_id == taskTypeId
                    orderby item.sort_order
                    select item;

            foreach (complexity com in c)
            {
                Complexities.Add(CreateComplexityDTO(com, false));
            }

            return Complexities;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="taskTypeId"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public List<Complexity> GetComplexityList(long teamId, long taskTypeId, bool active)
        {
            List<Complexity> Complexities = new List<Complexity>();

            var c = from item in _db.GetContext().complexities
                    where item.team_id == teamId && item.task_type_id == taskTypeId && item.active == active
                    select item;

            foreach (complexity com in c)
            {
                Complexities.Add(CreateComplexityDTO(com, false));
            }

            return Complexities;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public Task CreateTaskDTO(task r, bool recursive)
        {
            return CreateTaskDTO(r, null, recursive, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public Task CreateTaskDTO(task r, DTO.Profile profile, bool recursive, bool title_only)
        {
            Task task = new WALT.DTO.Task();

            if (r != null)
            {
                task.Id = r.id;
                task.Title = r.title;
                task.ModifiedDate = r.modified;
                task.Deleted = r.deleted;
                IQueryable<task> children = null;

                if (!title_only || recursive)
                {
                    children = from item in _db.GetContext().tasks
                               where item.parent_id == r.id && item.id != r.id && item.deleted == false
                               select item;

                    task.BaseTask = (children.Count() == 0);
                }

                if (!title_only)
                {
                    task.DueDate = r.due_date;
                    task.Assigned = _mediator.GetProfileProcessor().CreateProfileDTO(r.profile2, false, false);
                    task.Owner = _mediator.GetProfileProcessor().CreateProfileDTO(r.profile1, false, false);
                    task.Originator = _mediator.GetProfileProcessor().CreateProfileDTO(r.profile, false, false);
                    task.ParentId = r.parent_id ?? 0;
                    task.DueDate = r.due_date;
                    task.Hours = r.hours.GetValueOrDefault();
                    task.Estimate = r.estimate.GetValueOrDefault();
                    task.Complexity = _mediator.GetAdminProcessor().CreateComplexityDTO(r.complexity);
                    task.Source = r.source ?? string.Empty;
                    task.SourceID = r.source_id ?? string.Empty;
                    task.StartDate = r.start_date;
                    task.DueDate = r.due_date;
                    task.CompletedDate = r.completed_date;
                    task.Status = (DTO.Task.StatusEnum)Enum.Parse(typeof(DTO.Task.StatusEnum), r.status, true);
                    task.OwnerComments = r.owner_comments ?? string.Empty;
                    task.AssigneeComments = r.assignee_comments ?? string.Empty;
                    task.ExitCriteria = r.exit_criteria ?? string.Empty;
                    task.WBS = r.wbs ?? string.Empty;
                    task.Program = _mediator.GetAdminProcessor().CreateProgramDTO(r.program);
                    task.Created = r.created;
                    task.OnHoldDate = r.on_hold_date;
                    task.Instantiated = r.instantiated;
                    task.FullyAllocated = r.fully_allocated;

                    if (r.task_type != null)
                    {
                        task.TaskType = _mediator.GetAdminProcessor().CreateTaskTypeDTO(r.task_type, null);
                    }

                    if (task.BaseTask)
                    {
                        task.Spent = GetHoursSpent(r.id, false);
                    }
                    else
                    {
                        foreach (task rec in children)
                        {
                            task.HoursAllocatedToChildren += rec.hours ?? 0;
                            task.Spent += GetHoursSpent(rec.id, true);
                        }
                    }

                    task.Spent = Convert.ToDouble(decimal.Round(Convert.ToDecimal(task.Spent), 1));
                }

                if (recursive)
                {
                    foreach (task rec in children)
                    {
                        // Allow if you are the task owner, assignee, ALM or Directorate Manager

                        if (profile == null || 
                            rec.owner_id == profile.Id || 
                            rec.profile_id == profile.Id || 
                            CanViewTask(profile.Id, rec.owner_id) ||
                            CanViewTask(profile.Id, rec.profile_id)
                            
                            )
                        {
                            task.Children.Add(CreateTaskDTO(rec, profile, recursive, title_only));
                        }
                    }
                }
            }

            return task;
        }

        private bool CanViewTask(long manager_id, long? profile_id)
        {
            bool result = false;

            if (profile_id != null)
            {
                long id = profile_id.GetValueOrDefault();

                team_profile tp = _db.GetContext().team_profiles.SingleOrDefault(
                    x => x.profile_id == profile_id && x.role == "MEMBER");

                // Check to see if a team admin

                if (tp != null)
                {
                    team_profile tp2 = _db.GetContext().team_profiles.SingleOrDefault(
                        x => x.team_id == tp.team_id && x.profile_id == manager_id && x.role == "ADMIN");

                    if (tp2 == null)
                    {
                        team t = _db.GetContext().teams.SingleOrDefault(
                            x => x.id == tp.team_id && x.owner_id == manager_id);

                        if (t == null)
                        {
                            t = _db.GetContext().teams.SingleOrDefault(x => x.id == tp.team_id);

                            // Check to see if a directorate manager

                            if (t != null)
                            {
                                tp2 = _db.GetContext().team_profiles.SingleOrDefault(
                                    x => x.team_id == t.parent_id && x.profile_id == manager_id && x.role == "MANAGER");

                                result = tp2 != null;
                            }
                        }
                        else
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        result = true;
                    }
                }
            }

            return result;
        }

        private double GetHoursSpent(long taskId, bool recursive)
        {
            double hours = 0;
            IQueryable<long> children = null;

            if (recursive)
            {
                children = from item in _db.GetContext().tasks
                           where item.parent_id == taskId && item.id != taskId && item.deleted == false
                           select item.id;
            }

            if (children != null && children.Count() > 0)
            {
                foreach (long childId in children)
                {
                    hours += GetHoursSpent(childId, true);
                }
            }
            else
            {
                var spent = from wt in _db.GetContext().weekly_tasks
                            join wta in _db.GetContext().weekly_task_hours on wt.id equals wta.weekly_task_id
                            where wt.task_id == taskId
                            select wta.actual_hours;

                if (spent.Count() > 0)
                {
                    hours = spent.Sum();
                }
            }

            return hours;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public List<DTO.Task> GetTaskListByOwner(DTO.Profile owner)
        {
            List<DTO.Task> tasks = new List<DTO.Task>();

            var query = from item in _db.GetContext().tasks
                        where item.owner_id == owner.Id && item.deleted == false
                        select item;

            foreach (task r in query)
            {
                tasks.Add(CreateTaskDTO(r, false));
            }

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DTO.Task GetTask(long id, bool recursive)
        {
            task t = (from item in _db.GetContext().tasks
                      where item.id == id
                      select item).SingleOrDefault();

            if (t != null)
            {
                return CreateTaskDTO(t, recursive);
            }
            else return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="profile"></param>
        /// <param name="recursive"></param>
        /// <param name="titleOnly"></param>
        /// <returns></returns>
        public DTO.Task GetTask(long id, DTO.Profile profile, bool recursive, bool titleOnly)
        {
            task t = (from item in _db.GetContext().tasks
                      where item.id == id
                      select item).SingleOrDefault();

            if (t != null)
            {
                return CreateTaskDTO(t, profile, recursive, titleOnly);
            }
            else return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public DTO.Task GetTaskByWeeklyTaskID(long id, bool recursive)
        {
            DTO.Task task = null;

            task t = (from wt in _db.GetContext().weekly_tasks
                      join item in _db.GetContext().tasks on wt.task_id equals item.id
                      where wt.id == id
                      select item).SingleOrDefault();

            if (t != null)
            {
                task = CreateTaskDTO(t, recursive);
            }

            return task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public long GetTaskIdByWeeklyTaskId(long id)
        {
            var query = from wt in _db.GetContext().weekly_tasks
                        where wt.id == id
                        select wt.task_id;

            if (query.Count() > 0)
            {
                return query.First();
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Favorite GetFavorite(long id)
        {
            Favorite fav = null;
            favorite f = _db.GetContext().favorites.SingleOrDefault(x => x.id == id);

            if (f != null)
            {
                fav = CreateFavoriteDTO(f);
            }

            return fav;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<Favorite> GetFavorites(long profileID)
        {
            List<Favorite> favorites = new List<Favorite>();

            var query = from item in _db.GetContext().favorites
                        where item.profile_id == profileID
                        select item;

            foreach (favorite f in query)
            {
                favorites.Add(CreateFavoriteDTO(f));
            }

            return favorites;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileID"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public List<Favorite> GetFavorites(long profileID, bool template)
        {
            List<Favorite> favorites = new List<Favorite>();

            var query = from item in _db.GetContext().favorites
                        where item.profile_id == profileID && item.template == template
                        select item;

            foreach (favorite f in query)
            {
                favorites.Add(CreateFavoriteDTO(f));
            }

            return favorites;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public Favorite CreateFavoriteDTO(favorite f)
        {
            Favorite fav = null;

            if (f != null)
            {
                fav = new Favorite();
                fav.Id = f.id;
                fav.Title = f.title;
                fav.Profile = _mediator.GetProfileProcessor().GetProfile(f.profile_id);
                fav.TaskType = f.task_type_id.HasValue ? _mediator.GetAdminProcessor().CreateTaskTypeDTO(f.task_type, null) : null;
                fav.Program = f.program_id.HasValue ? _mediator.GetAdminProcessor().CreateProgramDTO(f.program) : null;
                fav.Complexity = f.complexity_id.HasValue ? _mediator.GetAdminProcessor().CreateComplexityDTO(f.complexity) : null;
                fav.Hours = f.hours.GetValueOrDefault();
                fav.Estimate = f.estimate.GetValueOrDefault();
                fav.Template = f.template;

                for (int i = 0; i < 7; i++)
                {
                    fav.PlanHours.Add(i, 0);
                }

                var query = from item in _db.GetContext().favorite_plan_hours
                            where item.favorite_id == f.id
                            select item;

                foreach (favorite_plan_hour fph in query)
                {
                    fav.PlanHours[fph.day_of_week] = fph.hours;
                }
            }

            return fav;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="weekEnding"></param>
        /// <returns></returns>
        public List<WeeklyPlan> GetTeamWeeklyPlanList(Team team, DateTime weekEnding)
        {
            List<WeeklyPlan> WL = new List<WeeklyPlan>();

            var query = from item1 in _db.GetContext().teams
                        join item2 in _db.GetContext().weekly_plans
                        on item1.id equals item2.team_id
                        where item2.week_ending == weekEnding
                        select item2;

            foreach (weekly_plan r in query)
            {
                WL.Add(CreateWeeklyPlanDTO(r, true));
            }

            return WL;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WeeklyPlan GetWeeklyPlan(long id, bool loadTasks)
        {
            WeeklyPlan weeklyPlan = null;
            weekly_plan wp = _db.GetContext().weekly_plans.SingleOrDefault(x => x.id == id);

            if (wp != null)
            {
                weeklyPlan = CreateWeeklyPlanDTO(wp, loadTasks);
            }

            return weeklyPlan;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="weekEnding"></param>
        /// <returns></returns>
        public WeeklyPlan GetWeeklyPlan(DateTime weekEnding, Profile profile)
        {
            WeeklyPlan weeklyPlan = null;
            weekly_plan wp = _db.GetContext().weekly_plans.SingleOrDefault(x => x.week_ending == weekEnding && x.profile_id == profile.Id);

            if (wp != null)
            {
                weeklyPlan = CreateWeeklyPlanDTO(wp, true);
            }

            return weeklyPlan;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weekEnding"></param>
        /// <param name="teamID"></param>
        /// <returns></returns>
        public List<WeeklyPlan> GetWeeklyPlans(DateTime weekEnding, long teamID)
        {
            List<WeeklyPlan> plans = new List<WeeklyPlan>();

            var query = from item in _db.GetContext().weekly_plans
                        where item.week_ending == weekEnding && item.team_id == teamID
                        select item;

            foreach (weekly_plan wp in query)
            {
                plans.Add(CreateWeeklyPlanDTO(wp, true));
            }

            return plans;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="weekEnding"></param>
        /// <returns></returns>
        public DateTime? GetWeeklyPlanModified(long profileId, DateTime weekEnding)
        {
            var query = from item in _db.GetContext().weekly_plans
                        where item.profile_id == profileId && item.week_ending == weekEnding
                        select item.modified;

            if (query.Count() == 0)
            {
                return null;
            }
            else
            {
                return query.First();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="weekEnding"></param>
        /// <returns></returns>
        public WeeklyPlan.StatusEnum? GetWeeklyPlanStatus(long profileId, DateTime weekEnding)
        {
            WeeklyPlan.StatusEnum? status = null;
            string getStatus = (from item in _db.GetContext().weekly_plans
                                where item.profile_id == profileId && item.week_ending == weekEnding
                                select item.state).SingleOrDefault();

            if (!string.IsNullOrEmpty(getStatus))
            {
                status = (DTO.WeeklyPlan.StatusEnum)Enum.Parse(typeof(DTO.WeeklyPlan.StatusEnum), getStatus, true);
            }

            return status;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadTasks">Loading WeeklyTasks is expensive, set false to not create WeeklyTasks on this plan.</param>
        /// <returns></returns>
        public WeeklyPlan CreateWeeklyPlanDTO(weekly_plan wp, bool loadTasks)
        {
            WeeklyPlan weeklyPlan = new WALT.DTO.WeeklyPlan();

            weeklyPlan.Id = wp.id;
            weeklyPlan.Profile = _mediator.GetProfileProcessor().CreateProfileDTO(wp.profile2);
            weeklyPlan.PlanApprovedBy = wp.plan_approved_by.HasValue ?
                _mediator.GetProfileProcessor().CreateProfileDTO(wp.profile1, false, false) : null;
            weeklyPlan.LogApprovedBy = wp.log_approved_by.HasValue ?
                _mediator.GetProfileProcessor().CreateProfileDTO(wp.profile, false, false) : null;
            weeklyPlan.Team = _mediator.GetAdminProcessor().CreateTeamDTO(wp.team, false);
            weeklyPlan.WeekEnding = wp.week_ending;
            weeklyPlan.PlanSubmitted = wp.plan_submitted;
            weeklyPlan.LogSubmitted = wp.log_submitted;

            weeklyPlan.State = (wp.state == null) ? DTO.WeeklyPlan.StatusEnum.NEW :
                (DTO.WeeklyPlan.StatusEnum)Enum.Parse(typeof(DTO.WeeklyPlan.StatusEnum), wp.state, true);

            weeklyPlan.Modified = wp.modified;

            if (loadTasks)
            {
                var query = from item in _db.GetContext().weekly_tasks
                            where item.weekly_plan_id == wp.id
                            orderby item.id
                            select item;

                foreach (weekly_task wt in query)
                {
                    weeklyPlan.WeeklyTasks.Add(CreateWeeklyTaskDTO(wt, true));
                }

                var getLeave = from item in _db.GetContext().weekly_plan_leaves
                               where item.weekly_plan_id == wp.id
                               select item;

                for (int i = 0; i < 7; i++)
                {
                    weeklyPlan.LeavePlanHours.Add(i, 0);
                    weeklyPlan.LeaveActualHours.Add(i, 0);
                }

                foreach (weekly_plan_leave wpl in getLeave)
                {
                    weeklyPlan.LeavePlanned = wpl.planned;
                    weeklyPlan.LeavePlanHours[wpl.day_of_week] = wpl.plan_hours;
                    weeklyPlan.LeaveActualHours[wpl.day_of_week] = wpl.actual_hours;
                }
            }
            else
            {
                weekly_plan_leave leave = _db.GetContext().weekly_plan_leaves.FirstOrDefault(x => x.weekly_plan_id == wp.id);

                if (leave != null)
                {
                    weeklyPlan.LeavePlanned = leave.planned;
                }
            }

            return weeklyPlan;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WeeklyTask GetWeeklyTask(long id)
        {
            WeeklyTask weeklyTask = null;
            weekly_task wt = _db.GetContext().weekly_tasks.SingleOrDefault(x => x.id == id);

            if (wt != null)
            {
                weeklyTask = CreateWeeklyTaskDTO(wt, false);
            }

            return weeklyTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wt1"></param>
        /// <returns></returns>
        private DTO.WeeklyTask CreateWeeklyTaskDTO(weekly_task wt, bool loadHours)
        {
            WeeklyTask weeklyTask = null;

            if (wt != null)
            {
                weeklyTask = new WeeklyTask();
                weeklyTask.Id = wt.id;
                weeklyTask.WeeklyPlanId = wt.weekly_plan_id;
                weeklyTask.Task = CreateTaskDTO(wt.task, false);
                weeklyTask.Comment = wt.comment ?? string.Empty;
                weeklyTask.UnplannedCode = wt.unplanned_code_id.HasValue ?
                    _mediator.GetAdminProcessor().CreateUnplannedCodeDTO(wt.unplanned_code) : null;
                weeklyTask.PlanDayComplete = wt.plan_day_complete.GetValueOrDefault(-1);
                weeklyTask.ActualDayComplete = wt.actual_day_complete.GetValueOrDefault(-1);

                if (loadHours)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        weeklyTask.PlanHours.Add(i, 0);
                        weeklyTask.ActualHours.Add(i, 0);
                    }

                    var getHours = from item in _db.GetContext().weekly_task_hours
                                   where item.weekly_task_id == wt.id
                                   select item;

                    foreach (var ph in getHours)
                    {
                        weeklyTask.PlanHours[ph.day_of_week] = ph.plan_hours;
                        weeklyTask.ActualHours[ph.day_of_week] = ph.actual_hours;
                    }

                    var getBarriers = from item in _db.GetContext().weekly_task_barriers
                                      where item.weekly_task_id == wt.id
                                      orderby item.id
                                      select item;

                    foreach (weekly_task_barrier wtb in getBarriers)
                    {
                        weeklyTask.Barriers.Add(CreateWeeklyBarrierDTO(wtb, true));
                    }
                }
            }

            return weeklyTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wtb"></param>
        /// <param name="loadHours"></param>
        /// <returns></returns>
        public WeeklyBarrier CreateWeeklyBarrierDTO(weekly_task_barrier wtb, bool loadHours)
        {
            WeeklyBarrier wb = null;

            if (wtb != null)
            {
                wb = new WeeklyBarrier();
                wb.Id = wtb.id;
                wb.WeeklyTaskId = wtb.weekly_task_id;
                wb.Barrier = _mediator.GetAdminProcessor().CreateBarrierDTO(wtb.barrier);
                wb.Comment = wtb.comment;
                wb.Ticket = wtb.ticket ?? string.Empty;
                wb.BarrierType = wtb.barrier_type == "EFFICIENCY" ?
                    WeeklyBarrier.BarriersEnum.EFFICIENCY : WeeklyBarrier.BarriersEnum.DELAY;

                if (loadHours)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        wb.Hours.Add(i, 0);
                    }

                    var getBarHours = from wtbh in _db.GetContext().weekly_task_barrier_hours
                                      where wtbh.weekly_task_barrier_id == wtb.id
                                      select wtbh;

                    foreach (weekly_task_barrier_hour wtbh in getBarHours)
                    {
                        wb.Hours[wtbh.day_of_week] = wtbh.hours;
                    }
                }
            }

            return wb;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WeeklyBarrier GetWeeklyBarrier(long id)
        {
            WeeklyBarrier wb = null;
            weekly_task_barrier wtb = _db.GetContext().weekly_task_barriers.SingleOrDefault(x => x.id == id);

            if (wtb != null)
            {
                wb = CreateWeeklyBarrierDTO(wtb, false);
            }

            return wb;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="ignoreWeeklyTaskID"></param>
        /// <returns></returns>
        public double GetTaskTotalSpent(long taskID, long ignoreWeeklyTaskID)
        {
            var query = from wt in _db.GetContext().weekly_tasks
                        join wta in _db.GetContext().weekly_task_hours on wt.id equals wta.weekly_task_id
                        where wt.task_id == taskID && wt.id != ignoreWeeklyTaskID
                        select wta.actual_hours;

            if (query.Count() > 0)
            {
                return query.Sum();
            }

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="barrierType"></param>
        /// <param name="ignoreWeeklyTaskID"></param>
        /// <returns></returns>
        public double GetTaskTotalBarriersHours(
            long taskID,
            WeeklyBarrier.BarriersEnum barrierType,
            List<WeeklyBarrier> ignoreBarriers
        )
        {
            var query = from wt in _db.GetContext().weekly_tasks
                        join wtb in _db.GetContext().weekly_task_barriers on wt.id equals wtb.weekly_task_id
                        join wtbh in _db.GetContext().weekly_task_barrier_hours on wtb.id equals wtbh.weekly_task_barrier_id
                        where wt.task_id == taskID && wtb.barrier_type == barrierType.ToString() && !ignoreBarriers.Select(x => x.Id).Contains(wtb.id)
                        select wtbh.hours;

            if (query.Count() > 0)
            {
                return query.Sum();
            }

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        public void DeleteFavorites(List<long> ids)
        {
            _db.GetContext().favorite_plan_hours.DeleteAllOnSubmit(from item in _db.GetContext().favorite_plan_hours
                                                                   where ids.Contains(item.favorite_id)
                                                                   select item);

            _db.GetContext().favorites.DeleteAllOnSubmit(from item in _db.GetContext().favorites
                                                         where ids.Contains(item.id)
                                                         select item);

            _db.SubmitChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public bool IsTaskAllowedToBeClosed(long id)
        {
            return (_db.GetContext().tasks.Count(x => x.parent_id == id && x.id != id && !x.deleted &&
                x.status != Task.StatusEnum.COMPLETED.ToString() && x.status != Task.StatusEnum.OBE.ToString()) == 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsTaskAllowedToBeDeleted(long id)
        {
            if (IsTaskPlanned(id)) return false;

            IQueryable<task> query = from item in _db.GetContext().tasks
                                        where item.parent_id == id && item.id != id && !item.deleted
                                        select item;

            int count = query.Count();

            if (count == 0)
            {
                return true;
            }
            else
            {
                int i = 0;
                List<task> children = query.ToList();
                bool childAllowed = true;

                while (childAllowed && i < count)
                {
                    if (children[i].instantiated)
                    {
                        childAllowed = false;
                    }
                    else
                    {
                        childAllowed = IsTaskAllowedToBeDeleted(children[i].id);
                    }

                    i++;
                }

                return childAllowed;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool IsTaskPlanned(Task task)
        {
            return IsTaskPlanned(task.Id);
        }

        public bool IsTaskPlanned(long id)
        {
            return _db.GetContext().weekly_tasks.Count(x => x.task_id == id) > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weeklyPlan"></param>
        /// <returns></returns>
        public void SaveWeeklyPlan(
            WeeklyPlan weeklyPlan,
            bool updateLeave,
            List<long> updateTaskIDs,
            List<long> deleteTaskIDs,
            List<long> deleteBarrierIDs,
            Profile editor
        )
        {
            bool insert = false;
            bool plan = false;
            long id = weeklyPlan.Id;
            weekly_plan wp = null;

            _db.BeginTransaction();

            try
            {
                if (id > 0)
                {
                    wp = _db.GetContext().weekly_plans.SingleOrDefault(x => x.id == id);
                }

                if (wp == null)
                {
                    wp = new weekly_plan();
                    _db.GetContext().weekly_plans.InsertOnSubmit(wp);
                    insert = true;
                    plan = true;
                }
                else if (wp.state == WeeklyPlan.StatusEnum.NEW.ToString() ||
                    wp.state == WeeklyPlan.StatusEnum.PLAN_READY.ToString())
                {
                    plan = true;
                }

                wp.profile2 = _db.GetContext().profiles.Single(x => x.id == weeklyPlan.Profile.Id);
                wp.profile1 = weeklyPlan.PlanApprovedBy != null ? _db.GetContext().profiles.Single(x => x.id == weeklyPlan.PlanApprovedBy.Id) : null;
                wp.profile = weeklyPlan.LogApprovedBy != null ? _db.GetContext().profiles.Single(x => x.id == weeklyPlan.LogApprovedBy.Id) : null;
                wp.week_ending = weeklyPlan.WeekEnding;
                wp.plan_submitted = weeklyPlan.PlanSubmitted;
                wp.log_submitted = weeklyPlan.LogSubmitted;
                wp.state = weeklyPlan.State.ToString();
                wp.modified = DateTime.Now;

                if (weeklyPlan.State == WeeklyPlan.StatusEnum.NEW || weeklyPlan.State == WeeklyPlan.StatusEnum.PLAN_READY ||
                    (weeklyPlan.State == WeeklyPlan.StatusEnum.PLAN_APPROVED && plan))
                {
                    wp.team = _db.GetContext().teams.Single(x => x.id == weeklyPlan.Team.Id);
                }

                _db.SubmitChanges();
                weeklyPlan.Id = wp.id;

                if (!insert)
                {
                    if (updateLeave)
                    {
                        var getLeave = from item in _db.GetContext().weekly_plan_leaves
                                       where item.weekly_plan_id == wp.id
                                       select item;

                        if (weeklyPlan.LeavePlanned.HasValue)
                        {
                            Dictionary<int, weekly_plan_leave> leaveMap = new Dictionary<int, weekly_plan_leave>();

                            foreach (weekly_plan_leave wpl in getLeave)
                            {
                                leaveMap.Add(wpl.day_of_week, wpl);
                            }

                            if (plan)
                            {
                                foreach (int day in weeklyPlan.LeavePlanHours.Keys)
                                {
                                    if (leaveMap.ContainsKey(day))
                                    {
                                        weekly_plan_leave wpl = leaveMap[day];

                                        if (weeklyPlan.LeavePlanHours[day] > 0)
                                        {
                                            wpl.plan_hours = weeklyPlan.LeavePlanHours[day];
                                        }
                                        else
                                        {
                                            _db.GetContext().weekly_plan_leaves.DeleteOnSubmit(wpl);
                                        }
                                    }
                                    else if (weeklyPlan.LeavePlanHours[day] > 0)
                                    {
                                        weekly_plan_leave wpl = new weekly_plan_leave();
                                        wpl.weekly_plan_id = wp.id;
                                        wpl.day_of_week = day;
                                        wpl.plan_hours = weeklyPlan.LeavePlanHours[day];
                                        wpl.planned = weeklyPlan.LeavePlanned.Value;
                                        _db.GetContext().weekly_plan_leaves.InsertOnSubmit(wpl);
                                    }
                                }
                            }
                            else
                            {
                                foreach (int day in weeklyPlan.LeaveActualHours.Keys)
                                {
                                    if (leaveMap.ContainsKey(day))
                                    {
                                        weekly_plan_leave wpl = leaveMap[day];

                                        if (weeklyPlan.LeaveActualHours[day] > 0 || wpl.plan_hours > 0)
                                        {
                                            wpl.actual_hours = weeklyPlan.LeaveActualHours[day];
                                        }
                                        else
                                        {
                                            _db.GetContext().weekly_plan_leaves.DeleteOnSubmit(wpl);
                                        }
                                    }
                                    else if (weeklyPlan.LeaveActualHours[day] > 0)
                                    {
                                        weekly_plan_leave wpl = new weekly_plan_leave();
                                        wpl.weekly_plan_id = wp.id;
                                        wpl.day_of_week = day;
                                        wpl.actual_hours = weeklyPlan.LeaveActualHours[day];
                                        wpl.planned = weeklyPlan.LeavePlanned.Value;
                                        _db.GetContext().weekly_plan_leaves.InsertOnSubmit(wpl);
                                    }
                                }
                            }
                        }
                        else
                        {
                            _db.GetContext().weekly_plan_leaves.DeleteAllOnSubmit(getLeave);
                        }

                        _db.SubmitChanges();
                    }

                    if (deleteBarrierIDs.Count > 0)
                    {
                        var delBarriers = from wtb in _db.GetContext().weekly_task_barriers
                                          join wtbh in _db.GetContext().weekly_task_barrier_hours on wtb.id equals wtbh.weekly_task_barrier_id into wtbj
                                          where deleteBarrierIDs.Contains(wtb.id)
                                          from subwtb in wtbj.DefaultIfEmpty()
                                          select new { wtb, subwtb };

                        foreach (var v in delBarriers)
                        {
                            if (v.subwtb != null)
                            {
                                _db.GetContext().weekly_task_barrier_hours.DeleteOnSubmit(v.subwtb);
                            }

                            _db.GetContext().weekly_task_barriers.DeleteOnSubmit(v.wtb);
                        }

                        _db.SubmitChanges();
                    }

                    if (deleteTaskIDs.Count > 0)
                    {
                        if (!plan)
                        {
                            var delBarriers = from wtb in _db.GetContext().weekly_task_barriers
                                              join wtbh in _db.GetContext().weekly_task_barrier_hours on wtb.id equals wtbh.weekly_task_barrier_id into wtbj
                                              where deleteTaskIDs.Contains(wtb.weekly_task_id)
                                              from subwtb in wtbj.DefaultIfEmpty()
                                              select new { wtb, subwtb };

                            foreach (var v in delBarriers)
                            {
                                if (v.subwtb != null)
                                {
                                    _db.GetContext().weekly_task_barrier_hours.DeleteOnSubmit(v.subwtb);
                                }

                                _db.GetContext().weekly_task_barriers.DeleteOnSubmit(v.wtb);
                            }
                        }

                        List<long> wtIDs = new List<long>();
                        var query = from wt in _db.GetContext().weekly_tasks
                                    join wth in _db.GetContext().weekly_task_hours on wt.id equals wth.weekly_task_id into wtj
                                    where deleteTaskIDs.Contains(wt.id)
                                    from subwth in wtj.DefaultIfEmpty()
                                    select new { wt, subwth };

                        foreach (var v in query)
                        {
                            if (v.subwth != null)
                            {
                                _db.GetContext().weekly_task_hours.DeleteOnSubmit(v.subwth);
                            }

                            if (!wtIDs.Contains(v.wt.id))
                            {
                                if (v.wt.unplanned_code_id.HasValue)
                                {
                                    task task = _db.GetContext().tasks.Single(x => x.id == v.wt.task_id);

                                    if (task.status == Task.StatusEnum.COMPLETED.ToString())
                                    {
                                        UpdateTaskStatus(task, Task.StatusEnum.OPEN, null, editor, false, true);
                                    }

                                    if ((from wt in _db.GetContext().weekly_tasks
                                         join wplan in _db.GetContext().weekly_plans on wt.weekly_plan_id equals wplan.id
                                         where wt.task_id == task.id && wplan.state != "NEW" && wplan.state != "PLAN_READY" && wplan.id != wp.id
                                         select wplan).Count() == 0)
                                    {
                                        bool inst = false;                                        
                                        task.instantiated = false;

                                        while (task.parent_id.HasValue && !inst)
                                        {
                                            task parent = _db.GetContext().tasks.Single(x => x.id == task.parent_id.Value);

                                            if (_db.GetContext().tasks.Count(x => x.parent_id == parent.id && x.id != task.id && x.instantiated == true) == 0)
                                            {
                                                parent.instantiated = false;
                                                task = parent;
                                            }
                                            else
                                            {
                                                inst = true;
                                            }
                                        }
                                    }
                                }

                                wtIDs.Add(v.wt.id);
                                _db.GetContext().weekly_tasks.DeleteOnSubmit(v.wt);
                            }
                        }

                        _db.SubmitChanges();
                    }
                }
                else if (weeklyPlan.LeavePlanned.HasValue)
                {
                    foreach (int day in weeklyPlan.LeavePlanHours.Keys)
                    {
                        if (weeklyPlan.LeavePlanHours[day] > 0)
                        {
                            weekly_plan_leave wpl = new weekly_plan_leave();
                            wpl.weekly_plan_id = wp.id;
                            wpl.day_of_week = day;
                            wpl.plan_hours = weeklyPlan.LeavePlanHours[day];
                            wpl.planned = weeklyPlan.LeavePlanned.Value;
                            _db.GetContext().weekly_plan_leaves.InsertOnSubmit(wpl);
                        }
                    }

                    _db.SubmitChanges();
                }

                foreach (WeeklyTask weeklyTask in weeklyPlan.WeeklyTasks)
                {
                    insert = false;
                    weekly_task wt = null;
                    bool update = updateTaskIDs.Contains(weeklyTask.Task.Id);

                    if (weeklyTask.Id > 0)
                    {
                        wt = _db.GetContext().weekly_tasks.SingleOrDefault(x => x.id == weeklyTask.Id);
                    }

                    if (wt == null)
                    {
                        wt = new weekly_task();
                        insert = true;

                        if (weeklyTask.Id < 0)
                        {
                            weeklyTask.Task.Id = 0;
                            weeklyTask.Task.Assigned = weeklyPlan.Profile;
                            weeklyTask.Task.Owner = editor;
                            update = true;
                        }
                    }

                    if (weeklyTask.ActualDayComplete != -1)
                    {
                        DateTime compDate = weeklyPlan.WeekEnding.AddDays(weeklyTask.ActualDayComplete - 6);

                        if (weeklyTask.Task.Status != Task.StatusEnum.COMPLETED ||
                            !weeklyTask.Task.CompletedDate.HasValue || weeklyTask.Task.CompletedDate != compDate)
                        {
                            weeklyTask.Task.Status = Task.StatusEnum.COMPLETED;
                            weeklyTask.Task.CompletedDate = compDate;
                            update = true;
                        }
                    }
                    else if (weeklyTask.Task.Status == Task.StatusEnum.COMPLETED)
                    {
                        weeklyTask.Task.Status = Task.StatusEnum.OPEN;
                        update = true;
                    }

                    if (update)
                    {
                        SaveTask(weeklyTask.Task, editor, false, true);
                    }

                    if (insert)
                    {
                        wt.task = _db.GetContext().tasks.Single(x => x.id == weeklyTask.Task.Id);

                        if (weeklyTask.UnplannedCode != null && !wt.task.instantiated)
                        {
                            task parent = wt.task;

                            while (!parent.instantiated && parent.parent_id.HasValue)
                            {
                                parent.instantiated = true;
                                parent.modified = DateTime.Now;
                                parent = _db.GetContext().tasks.Single(x => x.id == parent.parent_id.Value);
                            }

                            wt.task.instantiated = true;
                        }
                    }

                    wt.weekly_plan_id = wp.id;
                    wt.comment = weeklyTask.Comment;

                    if (weeklyTask.UnplannedCode != null)
                    {
                        wt.unplanned = true;
                        wt.unplanned_code = _db.GetContext().unplanned_codes.Single(x => x.id == weeklyTask.UnplannedCode.Id);
                    }
                    else
                    {
                        wt.unplanned = false;
                        wt.unplanned_code = null;
                    }

                    if (plan)
                    {
                        if (weeklyTask.PlanDayComplete == -1)
                        {
                            wt.plan_day_complete = null;
                        }
                        else
                        {
                            wt.plan_day_complete = weeklyTask.PlanDayComplete;
                        }
                    }
                    else if (weeklyTask.ActualDayComplete == -1)
                    {
                        wt.actual_day_complete = null;
                    }
                    else
                    {
                        wt.actual_day_complete = weeklyTask.ActualDayComplete;
                    }

                    if (insert)
                    {
                        _db.GetContext().weekly_tasks.InsertOnSubmit(wt);
                    }

                    _db.SubmitChanges();

                    if (insert)
                    {
                        if (plan)
                        {
                            foreach (int day in weeklyTask.PlanHours.Keys)
                            {
                                if (weeklyTask.PlanHours[day] > 0)
                                {
                                    weekly_task_hour wth = new weekly_task_hour();
                                    wth.weekly_task_id = wt.id;
                                    wth.day_of_week = day;
                                    wth.plan_hours = weeklyTask.PlanHours[day];
                                    _db.GetContext().weekly_task_hours.InsertOnSubmit(wth);
                                }
                            }
                        }
                        else
                        {
                            foreach (int day in weeklyTask.ActualHours.Keys)
                            {
                                if (weeklyTask.ActualHours[day] > 0)
                                {
                                    weekly_task_hour wth = new weekly_task_hour();
                                    wth.weekly_task_id = wt.id;
                                    wth.day_of_week = day;
                                    wth.actual_hours = weeklyTask.ActualHours[day];
                                    _db.GetContext().weekly_task_hours.InsertOnSubmit(wth);
                                }
                            }
                        }

                        _db.SubmitChanges();
                    }
                    else
                    {
                        Dictionary<int, weekly_task_hour> wthMap = new Dictionary<int, weekly_task_hour>();

                        var getHours = from item in _db.GetContext().weekly_task_hours
                                       where item.weekly_task_id == wt.id
                                       select item;

                        foreach (weekly_task_hour wth in getHours)
                        {
                            wthMap.Add(wth.day_of_week, wth);
                        }

                        if (plan)
                        {
                            foreach (int day in weeklyTask.PlanHours.Keys)
                            {
                                if (wthMap.ContainsKey(day))
                                {
                                    weekly_task_hour wth = wthMap[day];

                                    if (weeklyTask.PlanHours[day] > 0)
                                    {
                                        wth.plan_hours = weeklyTask.PlanHours[day];
                                    }
                                    else
                                    {
                                        _db.GetContext().weekly_task_hours.DeleteOnSubmit(wth);
                                    }
                                }
                                else if (weeklyTask.PlanHours[day] > 0)
                                {
                                    weekly_task_hour wth = new weekly_task_hour();
                                    wth.weekly_task_id = wt.id;
                                    wth.day_of_week = day;
                                    wth.plan_hours = weeklyTask.PlanHours[day];
                                    _db.GetContext().weekly_task_hours.InsertOnSubmit(wth);
                                }
                            }

                            _db.SubmitChanges();
                        }
                        else
                        {
                            List<int> delBarrierDays = new List<int>();

                            foreach (int day in weeklyTask.ActualHours.Keys)
                            {
                                if (wthMap.ContainsKey(day))
                                {
                                    weekly_task_hour wth = wthMap[day];

                                    if (weeklyTask.ActualHours[day] > 0 || wth.plan_hours > 0)
                                    {
                                        wth.actual_hours = weeklyTask.ActualHours[day];
                                    }
                                    else
                                    {
                                        _db.GetContext().weekly_task_hours.DeleteOnSubmit(wth);
                                    }
                                }
                                else if (weeklyTask.ActualHours[day] > 0)
                                {
                                    weekly_task_hour wth = new weekly_task_hour();
                                    wth.weekly_task_id = wt.id;
                                    wth.day_of_week = day;
                                    wth.actual_hours = weeklyTask.ActualHours[day];
                                    _db.GetContext().weekly_task_hours.InsertOnSubmit(wth);
                                }

                                if (weeklyTask.ActualHours[day] == 0)
                                {
                                    delBarrierDays.Add(day);
                                }
                            }

                            _db.GetContext().weekly_task_barrier_hours.DeleteAllOnSubmit(
                                from item1 in _db.GetContext().weekly_task_barriers
                                join item2 in _db.GetContext().weekly_task_barrier_hours on item1.id equals item2.weekly_task_barrier_id
                                where item1.weekly_task_id == wt.id && item1.barrier_type == "EFFICIENCY" &&
                                   delBarrierDays.Contains(item2.day_of_week)
                                select item2);

                            _db.SubmitChanges();
                        }
                    }

                    if (!plan && weeklyTask.Barriers.Count > 0)
                    {
                        foreach (WeeklyBarrier wb in weeklyTask.Barriers)
                        {
                            weekly_task_barrier wtb = null;
                            insert = false;

                            if (wb.Id > 0)
                            {
                                wtb = _db.GetContext().weekly_task_barriers.SingleOrDefault(x => x.id == wb.Id);
                            }

                            if (wtb == null)
                            {
                                wtb = new weekly_task_barrier();
                                _db.GetContext().weekly_task_barriers.InsertOnSubmit(wtb);
                                insert = true;
                            }

                            wtb.barrier = _db.GetContext().barriers.Single(x => x.id == wb.Barrier.Id);
                            wtb.barrier_type = wb.BarrierType.ToString();
                            wtb.weekly_task_id = wt.id;
                            wtb.comment = wb.Comment;
                            wtb.ticket = wb.Ticket;
                            _db.SubmitChanges();

                            if (insert)
                            {
                                foreach (int day in wb.Hours.Keys)
                                {
                                    if (wb.Hours[day] > 0)
                                    {
                                        weekly_task_barrier_hour wtbh = new weekly_task_barrier_hour();
                                        wtbh.weekly_task_barrier_id = wtb.id;
                                        wtbh.day_of_week = day;
                                        wtbh.hours = wb.Hours[day];
                                        _db.GetContext().weekly_task_barrier_hours.InsertOnSubmit(wtbh);
                                    }
                                }
                            }
                            else
                            {
                                Dictionary<int, weekly_task_barrier_hour> wtbhMap = new Dictionary<int, weekly_task_barrier_hour>();

                                var getBarHours = from item in _db.GetContext().weekly_task_barrier_hours
                                                  where item.weekly_task_barrier_id == wtb.id
                                                  select item;

                                foreach (weekly_task_barrier_hour wtbh in getBarHours)
                                {
                                    wtbhMap.Add(wtbh.day_of_week, wtbh);
                                }

                                foreach (int day in wb.Hours.Keys)
                                {
                                    if (wtbhMap.ContainsKey(day))
                                    {
                                        weekly_task_barrier_hour wtbh = wtbhMap[day];

                                        if (wb.Hours[day] > 0)
                                        {
                                            wtbh.hours = wb.Hours[day];
                                        }
                                        else
                                        {
                                            _db.GetContext().weekly_task_barrier_hours.DeleteOnSubmit(wtbh);
                                        }
                                    }
                                    else if (wb.Hours[day] > 0)
                                    {
                                        weekly_task_barrier_hour wtbh = new weekly_task_barrier_hour();
                                        wtbh.weekly_task_barrier_id = wb.Id;
                                        wtbh.day_of_week = day;
                                        wtbh.hours = wb.Hours[day];
                                        _db.GetContext().weekly_task_barrier_hours.InsertOnSubmit(wtbh);
                                    }
                                }
                            }

                            _db.SubmitChanges();
                        }
                    }
                }

                if (plan && weeklyPlan.State == WeeklyPlan.StatusEnum.PLAN_APPROVED)
                {
                    var getTasks = from t in _db.GetContext().tasks
                                   join wt in _db.GetContext().weekly_tasks on t.id equals wt.task_id
                                   where wt.weekly_plan_id == wp.id
                                   select t;

                    foreach (task task in getTasks)
                    {
                        task parent = task;

                        while (!parent.instantiated && parent.parent_id.HasValue)
                        {
                            parent.instantiated = true;
                            parent = _db.GetContext().tasks.Single(x => x.id == parent.parent_id.Value);
                        }

                        task.instantiated = true;
                    }

                    _db.SubmitChanges();
                }

                _db.CommitTransaction();
            }
            catch
            {
                _db.CancelTransaction();
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listTask"></param>
        /// <returns></returns>
        public void SaveTaskList(List<Task> listTask, Profile editor)
        {
            _db.BeginTransaction();

            try
            {
                for (int i = 0; i < listTask.Count; i++)
                {
                    if (!listTask[i].Error)
                        SaveTask(listTask[i], editor, true, true);
                }

                _db.CommitTransaction();
            }
            catch
            {
                _db.CancelTransaction();
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskIDs"></param>
        /// <param name="status"></param>
        public List<DTO.Task> RejectTasks(List<long> taskIDs)
        {
            List<DTO.Task> rejectTasks = new List<Task>();
            IQueryable<task> tasks = from item in _db.GetContext().tasks
                                     where taskIDs.Contains(item.id) && item.status != Task.StatusEnum.REJECTED.ToString()
                                     select item;

            foreach (task t in tasks)
            {
                t.profile1 = t.profile;
                t.status = Task.StatusEnum.REJECTED.ToString();

                Task task = new Task();
                task.Id = t.id;
                task.Title = t.title;
                task.Owner = _mediator.GetProfileProcessor().CreateProfileDTO(t.profile);
                rejectTasks.Add(task);
            }

            _db.SubmitChanges();

            return rejectTasks;
        }

        private bool UpdateTaskStatus(task t, Task.StatusEnum status, DateTime? changeDate, Profile editor, bool parentChange, bool initial)
        {
            if ((!parentChange && t.status == status.ToString()) || status == Task.StatusEnum.REJECTED)
            {
                return false;
            }

            // if reopening a task, reopen all its parents
            if (initial && (t.instantiated || status == Task.StatusEnum.OPEN || (status == Task.StatusEnum.HOLD &&
                (t.status == Task.StatusEnum.COMPLETED.ToString() || t.status == Task.StatusEnum.OBE.ToString()))))
            {
                task parent = t;
                bool nextParent = true;

                while (nextParent && parent.parent_id.HasValue)
                {
                    nextParent = false;
                    parent = _db.GetContext().tasks.Single(x => x.id == parent.parent_id.Value);

                    if (parent.status != status.ToString() &&
                        (parent.status == Task.StatusEnum.COMPLETED.ToString() ||
                         parent.status == Task.StatusEnum.OBE.ToString() ||
                         parent.status == Task.StatusEnum.HOLD.ToString()))
                    {
                        if (editor != null && !IsAllowedToModifyTask(parent, editor))
                        {
                            throw new Exception("You do not have access to modify the status of parent task " + parent.title);
                        }

                        parent.status = Task.StatusEnum.OPEN.ToString();
                        parent.completed_date = null;
                        parent.on_hold_date = null;
                        parent.modified = DateTime.Now;
                        nextParent = true;
                    }

                    if (t.instantiated && !parent.instantiated)
                    {
                        parent.instantiated = true;
                        parent.modified = DateTime.Now;
                        nextParent = true;
                    }
                }
            }

            if (status == Task.StatusEnum.OPEN)
            {
                t.completed_date = null;
                t.on_hold_date = null;
            }
            else
            {
                if (!changeDate.HasValue)
                {
                    changeDate = DateTime.Now;
                }

                // update all child tasks to same status, unless completed
                var children = from item in _db.GetContext().tasks
                               where item.parent_id == t.id && item.id != t.id && item.status != status.ToString() &&
                                item.status != Task.StatusEnum.COMPLETED.ToString() && item.status != Task.StatusEnum.OBE.ToString()
                               select item;

                foreach (task child in children)
                {
                    if (status != Task.StatusEnum.COMPLETED)
                    {
                        UpdateTaskStatus(child, status, changeDate, editor, false, false);
                    }
                    else if (!child.deleted)
                    {
                        throw new Exception("Child task " + child.title +
                            " is active. All child tasks must be Completed or OBE in order to mark parent task Completed.");
                    }
                }

                if (status == Task.StatusEnum.HOLD)
                {
                    t.on_hold_date = changeDate;
                    t.completed_date = null;
                }
                else
                {
                    t.completed_date = changeDate;
                }

                // if closing a task, close all of its fully allocated parents if all siblings are also closed
                if (initial && (status == Task.StatusEnum.COMPLETED || status == Task.StatusEnum.OBE))
                {
                    task child = t;

                    while (child != null && child.parent_id.HasValue)
                    {
                        task parent = _db.GetContext().tasks.Single(x => x.id == child.parent_id.Value);

                        if (parent.fully_allocated &&
                            _db.GetContext().tasks.Count(x => x.parent_id == parent.id && x.id != child.id && !x.deleted &&
                                (x.status != Task.StatusEnum.COMPLETED.ToString() && x.status != Task.StatusEnum.OBE.ToString())) == 0)
                        {
                            parent.status = status.ToString();
                            parent.completed_date = changeDate;
                            parent.modified = DateTime.Now;
                            child = parent;
                        }
                        else
                        {
                            child = null;
                        }
                    }
                }
            }

            t.status = status.ToString();
            t.modified = DateTime.Now;

            return true;
        }

        public bool IsAllowedToModifyTask(task t, Profile editor)
        {
            if (t.profile_id == editor.Id || t.owner_id == editor.Id)
            {
                return true;
            }

            team_profile tp = _db.GetContext().team_profiles.SingleOrDefault(x => x.profile_id == t.owner_id && x.role == "MEMBER");

            if (tp != null && _mediator.GetAdminProcessor().IsTeamAdmin(editor.Id, tp.team_id))
            {
                return true;
            }
            else if (t.profile_id.HasValue)
            {
                tp = _db.GetContext().team_profiles.SingleOrDefault(x => x.profile_id == t.profile_id.Value && x.role == "MEMBER");

                if (tp != null && _mediator.GetAdminProcessor().IsTeamAdmin(editor.Id, tp.team_id))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="favorites"></param>
        /// <param name="profile"></param>
        public void SaveFavorites(List<Favorite> favorites, Profile profile)
        {
            _db.BeginTransaction();

            try
            {
                foreach (Favorite fav in favorites)
                {
                    bool insert = false;
                    favorite f = null;

                    if (fav.Id > 0)
                    {
                        f = _db.GetContext().favorites.SingleOrDefault(x => x.id == fav.Id);
                    }
                    
                    if (f == null)
                    {
                        f = _db.GetContext().favorites.SingleOrDefault(x => x.title == fav.Title && x.profile_id == profile.Id);
                    }

                    if (f == null)
                    {
                        f = new favorite();
                        f.profile = _db.GetContext().profiles.Single(x => x.id == profile.Id);
                        _db.GetContext().favorites.InsertOnSubmit(f);
                        insert = true;
                    }

                    f.title = fav.Title;
                    f.task_type = fav.TaskType != null ? _db.GetContext().task_types.Single(x => x.id == fav.TaskType.Id) : null;
                    f.program = fav.Program != null ? _db.GetContext().programs.Single(x => x.id == fav.Program.Id) : null;
                    f.hours = fav.Hours > 0 ? fav.Hours : (double?)null;

                    if (fav.Complexity != null && fav.Complexity.Id > 0)
                    {
                        f.complexity = _db.GetContext().complexities.Single(x => x.id == fav.Complexity.Id);
                        f.estimate = f.complexity.hours;
                    }
                    else
                    {
                        f.complexity = null;
                        f.estimate = fav.Estimate > 0 ? fav.Estimate : (double?)null;
                    }

                    f.template = fav.Template;

                    _db.SubmitChanges();

                    fav.Id = f.id;
                    SaveFavoritePlanHours(fav, insert);
                }

                _db.CommitTransaction();
            }
            catch
            {
                _db.CancelTransaction();
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fav"></param>
        /// <param name="insert"></param>
        private void SaveFavoritePlanHours(Favorite fav, bool insert)
        {
            if (fav.PlanHours.Count > 0)
            {
                Dictionary<int, favorite_plan_hour> fphMap = new Dictionary<int, favorite_plan_hour>();

                if (!insert)
                {
                    var query = from item in _db.GetContext().favorite_plan_hours
                                where item.favorite_id == fav.Id
                                select item;

                    foreach (favorite_plan_hour fph in query)
                    {
                        fphMap.Add(fph.day_of_week, fph);
                    }
                }

                foreach (int day in fav.PlanHours.Keys)
                {
                    if (fphMap.ContainsKey(day))
                    {
                        favorite_plan_hour fph = fphMap[day];

                        if (fav.PlanHours[day] > 0)
                        {
                            fph.hours = fav.PlanHours[day];
                        }
                        else
                        {
                            _db.GetContext().favorite_plan_hours.DeleteOnSubmit(fph);
                        }
                    }
                    else if (fav.PlanHours[day] > 0)
                    {
                        favorite_plan_hour fph = new favorite_plan_hour();
                        fph.favorite_id = fav.Id;
                        fph.day_of_week = day;
                        fph.hours = fav.PlanHours[day];
                        _db.GetContext().favorite_plan_hours.InsertOnSubmit(fph);
                    }
                }

                _db.SubmitChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskIDs"></param>
        /// <param name="profile"></param>
        public void SaveTasksAsFavorites(List<long> taskIDs, Profile profile)
        {
            IQueryable<task> getTasks = from item in _db.GetContext().tasks
                                        where taskIDs.Contains(item.id)
                                        select item;

            List<string> titles = getTasks.Select(x => x.title).ToList();
            IQueryable<favorite> getFavs = from item in _db.GetContext().favorites
                                           where item.profile_id == profile.Id && titles.Contains(item.title)
                                           select item;

            Dictionary<string, favorite> favMap = new Dictionary<string, favorite>();

            foreach (favorite fav in getFavs)
            {
                favMap.Add(fav.title, fav);
            }

            foreach (task t in getTasks)
            {
                favorite fav;

                if (favMap.ContainsKey(t.title))
                {
                    fav = favMap[t.title];
                }
                else
                {
                    fav = new favorite();
                    fav.profile_id = profile.Id;
                    fav.title = t.title;
                    fav.template = false;
                    favMap.Add(fav.title, fav);
                    _db.GetContext().favorites.InsertOnSubmit(fav);
                }

                fav.task_type = t.task_type;
                fav.program = t.program;
                fav.hours = t.hours;
                fav.complexity = t.complexity;
                fav.estimate = t.estimate;
            }

            _db.SubmitChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        public void SaveTask(Task task, Profile editor, bool saveChildren, bool initial)
        {
            task t = null;
            bool insert = false;

            _db.BeginTransaction();

            try
            {
                if (task.Id == 0)
                {
                    t = new task();
                    t.status = task.Status.ToString();
                    t.created = DateTime.Now;
                    t.profile = _db.GetContext().profiles.SingleOrDefault(x => x.id == editor.Id);

                    _db.GetContext().tasks.InsertOnSubmit(t);
                    insert = true;
                }
                else
                {
                    t = _db.GetContext().tasks.Single(x => x.id == task.Id);
                }

                if (task.TaskType != null)
                {
                    t.task_type = _db.GetContext().task_types.SingleOrDefault(x => x.id == task.TaskType.Id);
                }
                
                t.profile1 = _db.GetContext().profiles.SingleOrDefault(x => x.id == task.Owner.Id);

                if (task.Assigned != null)
                {
                    t.profile2 = _db.GetContext().profiles.SingleOrDefault(x => x.id == task.Assigned.Id);
                }

                if (task.Program != null && task.Program.Id > 0)
                {
                    t.program = _db.GetContext().programs.SingleOrDefault(x => x.id == task.Program.Id);
                }

                if (task.Complexity != null && task.Complexity.Id > 0)
                {
                    t.complexity = _db.GetContext().complexities.SingleOrDefault(x => x.id == task.Complexity.Id);
                }

                bool parentChange = (task.ParentId > 0 && (!t.parent_id.HasValue || t.parent_id.Value != task.ParentId));

                if (initial && ((insert && !parentChange) || task.Status == Task.StatusEnum.REJECTED))
                {
                    initial = false;
                }

                bool setSource = false;
                task.Source = string.IsNullOrEmpty(task.Source) ? "WALT" : task.Source;

                if (string.IsNullOrEmpty(task.SourceID))
                {
                    task.SourceID = Guid.NewGuid().ToString();
                    setSource = true;
                }                

                t.parent_id = task.ParentId > 0 ? task.ParentId : (long?)null;
                t.title = task.Title;
                t.start_date = task.StartDate;
                t.due_date = task.DueDate;
                t.hours = task.Hours;
                t.estimate = task.Estimate;
                t.source = task.Source;
                t.source_id = task.SourceID;
                t.exit_criteria = task.ExitCriteria;
                t.wbs = task.WBS;
                t.owner_comments = task.OwnerComments;
                t.assignee_comments = task.AssigneeComments;
                t.fully_allocated = task.FullyAllocated;
                t.modified = DateTime.Now;

                DateTime? changeDate = null;

                if (task.Status == DTO.Task.StatusEnum.HOLD)
                {
                    if (task.OnHoldDate.HasValue)
                    {
                        changeDate = task.OnHoldDate;
                    }
                    else if (t.status != task.Status.ToString() || !t.on_hold_date.HasValue)
                    {
                        changeDate = DateTime.Now;
                    }
                    else
                    {
                        changeDate = t.on_hold_date;
                    }

                    if (!initial)
                    {
                        t.on_hold_date = changeDate;
                    }
                }
                else if (task.Status == DTO.Task.StatusEnum.COMPLETED || task.Status == DTO.Task.StatusEnum.OBE)
                {
                    if (task.CompletedDate.HasValue)
                    {
                        changeDate = task.CompletedDate;
                    }
                    else if (t.status != task.Status.ToString() || !t.completed_date.HasValue)
                    {
                        changeDate = DateTime.Now;
                    }
                    else
                    {
                        changeDate = t.completed_date;
                    }

                    if (!initial)
                    {
                        t.completed_date = changeDate;
                    }
                }
                else if (!initial)
                {
                    t.completed_date = null;
                    t.on_hold_date = null;
                }

                if (!initial)
                {
                    t.status = task.Status.ToString();
                }

                _db.SubmitChanges();

                task.Id = t.id;

                if (setSource)
                {
                    task.SourceID = "WALT_" + t.id;
                    t.source_id = task.SourceID;
                    _db.SubmitChanges();
                }

                for (int i = 0; i < task.Children.Count(); i++)
                {
                    task.Children[i].ParentId = task.Id;

                    if (saveChildren && !task.Children[i].Error)
                    {
                        SaveTask(task.Children[i], editor, true, false);
                    }
                }

                if (initial && UpdateTaskStatus(t, task.Status, changeDate, editor, parentChange, true))
                {
                    _db.SubmitChanges();
                }

                _db.CommitTransaction();
            }
            catch
            {
                _db.CancelTransaction();
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        public void DeleteTask(long taskId)
        {
            task t = _db.GetContext().tasks.SingleOrDefault(x => x.id == taskId);

            if (t != null)
            {
                _db.BeginTransaction();

                try
                {
                    DeleteTask(t);
                    _db.SubmitChanges();
                    _db.CommitTransaction();
                }
                catch
                {
                    _db.CancelTransaction();
                    throw;
                }
            }
        }

        private void DeleteTask(task t)
        {
            if (t == null) return;

            if (!t.instantiated &&
                _db.GetContext().weekly_tasks.Count(x => x.task_id == t.id) == 0)
            {
                var children = from item in _db.GetContext().tasks
                               where item.parent_id == t.id && item.id != t.id && !item.deleted
                               select item;

                foreach (task child in children)
                {
                    DeleteTask(child);
                }

                t.deleted = true;
            }
            else if (t.instantiated)
            {
                throw new Exception("Child task " + t.title + " is instantiated");
            }
            else
            {
                throw new Exception("Child task " + t.title + " is in a plan");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskIDs"></param>
        public void DeleteTasks(List<long> taskIDs)
        {
            _db.BeginTransaction();

            try
            {
                var tasks = from item in _db.GetContext().tasks
                            where taskIDs.Contains(item.id)
                            select item;
            
                foreach (task t in tasks)
                {
                    DeleteTask(t);
                }

                _db.SubmitChanges();
                _db.CommitTransaction();
            }
            catch
            {
                _db.CancelTransaction();
                throw;
            }
        }

        /// <summary>
        /// Columns can be in any order in the spreadsheet.  The possible columns are:
        /// Owner	Assigneed	Title	TaskType	Source	SourceID	StartDate	DueDate	RE	HoursAllocated	Complexity	Program	ExitCriteria	WBS	OwnerComments	AssigneeComments
        /// The only required column is Title.
        /// </summary>
        /// <param name="filename"></param>
        public List<Task> ImportFromExcel(string filename, DTO.Profile profile)//, List<List<string>> errors)
        {
            List<Task> tasks = new List<Task>();
            OleDbConnection conn;
            Dictionary<string, Task> wbs_map = new Dictionary<string, Task>();
            string fileTitle = string.Empty;

            try
            {
                string connstr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    filename + ";Extended Properties=Excel 12.0";

                conn = new OleDbConnection(connstr);
                conn.Open();
                DataTable table = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                // Get the name of the first sheet in the workbook

                string sheet_name = table.Rows[0]["TABLE_NAME"].ToString();
                conn.Close();

                // Select all records into a DataSet

                OleDbDataAdapter da = new OleDbDataAdapter();
                da.SelectCommand = new OleDbCommand("select * from [" + sheet_name + "]", conn);
                DataSet ds = new DataSet();
                da.Fill(ds, "Tasks");

                // Examine the column names and make sure we have the required fields
                string titleFld = string.Empty;

                if (ds.Tables[0].Columns.Contains("Title"))
                {
                    titleFld = "Title";
                }
                else if (ds.Tables[0].Columns.Contains("Title *"))
                {
                    titleFld = "Title *";
                }
                else
                {
                    throw new Exception("Spreadsheet must have a \"Title\" column.");
                }

                string programFld = string.Empty;

                if (ds.Tables[0].Columns.Contains("Program"))
                {
                    programFld = "Program";
                }
                else if (ds.Tables[0].Columns.Contains("Program *"))
                {
                    programFld = "Program *";
                }
                else
                {
                    throw new Exception("Spreadsheet must have a \"Program\" column.");
                }

                // Create the Task DTO objects and return a list 

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Task t = new Task();
                    List<string> errors = new List<string>();
                    List<string> warnings = new List<string>();

                    if (!string.IsNullOrEmpty(ds.Tables[0].Rows[i][titleFld].ToString()))
                    {
                        t.Title = ds.Tables[0].Rows[i][titleFld].ToString();
                    }
                    else
                    {
                        t.Error = true;
                        errors.Add("Error: You must enter a title.");
                    }

                    t.Originator = profile;
                    string name = string.Empty;
                    Profile owner = null;
                    bool noOwner = false;

                    if (ds.Tables[0].Columns.Contains("Owner"))
                    {
                        name = ds.Tables[0].Rows[i]["Owner"].ToString();
                    }

                    if (name != string.Empty)
                    {
                        owner = SearchProfiles(name);
                    }
                    else
                    {
                        owner = profile;
                        warnings.Add("Warning: Owner blank, assigning you as the owner.");
                    }

                    if (owner != null)
                    {
                        t.Owner = owner;
                    }
                    else
                    {
                        t.Error = true;
                        t.Owner = new Profile();
                        t.Owner.DisplayName = name;
                        errors.Add("Error: Owner " + name + " could not be found.");
                        noOwner = true;
                    }

                    name = string.Empty;
                    Profile assigned = null;

                    if (ds.Tables[0].Columns.Contains("Assignee"))
                    {
                        name = ds.Tables[0].Rows[i]["Assignee"].ToString();
                    }

                    if (name != string.Empty)
                    {
                        assigned = SearchProfiles(name);
                    }
                    else if (noOwner)
                    {
                        assigned = new Profile();
                    }
                    else
                    {
                        assigned = t.Owner;
                        warnings.Add("Warning: Assignee blank, assigning the owner as the assignee.");
                    }

                    if (assigned != null)
                    {
                        t.Assigned = assigned;
                    }
                    else
                    {
                        t.Error = true;
                        t.Assigned = new Profile();
                        t.Assigned.DisplayName = name;
                        errors.Add("Error: Assignee " + name + " could not be found.");
                    }

                    if (ds.Tables[0].Columns.Contains("Source"))
                    {
                        t.Source = ds.Tables[0].Rows[i]["Source"].ToString();

                        // If the user provided a source , then check to make sure they are not using WALT as Source.
                        if (t.Source == "WALT")
                        {
                           t.Error = true;
                           errors.Add("Error: You can not use \"WALT\" as a source for your tasks.");
                        }
                    }

                    if (t.Source == string.Empty)
                    {
                        if (filename.Contains('\\'))
                        {
                            t.Source = filename.Substring(filename.LastIndexOf('\\') + 1);
                        }
                        else
                        {
                            t.Source = filename;
                        }

                        warnings.Add("Info: File name was assigned as the source.");
                    }

                    if (ds.Tables[0].Columns.Contains("SourceID"))
                    {
                        t.SourceID = ds.Tables[0].Rows[i]["SourceID"].ToString();

                        if (t.SourceID.StartsWith("WALT_"))
                        {
                            t.Error = true;
                            errors.Add("Error: You can not start the source ID with \"WALT_\".");
                        }
                        else if (t.SourceID != string.Empty &&
                            _db.GetContext().tasks.SingleOrDefault(x => x.source == t.Source && x.source_id == t.SourceID) != null)
                        {
                            t.Error = true;
                            errors.Add("Error: Task with source " + t.Source + " and source ID " + t.SourceID + " already exists.");
                        }
                    }

                    if (ds.Tables[0].Columns.Contains("ExitCriteria"))
                    {
                        t.ExitCriteria = ds.Tables[0].Rows[i]["ExitCriteria"].ToString();
                    }

                    if (ds.Tables[0].Columns.Contains("WBS"))
                    {
                        t.WBS = ds.Tables[0].Rows[i]["WBS"].ToString();
                        wbs_map[t.WBS] = t;
                    }

                    if (ds.Tables[0].Columns.Contains("Complexity"))
                    {
                        // TODO: Import complexity
                    }

                    if (ds.Tables[0].Columns.Contains("HoursAllocated") &&
                        (ds.Tables[0].Rows[i]["HoursAllocated"].ToString().Length > 0))
                    {
                        try
                        {
                            t.Hours = Convert.ToDouble(ds.Tables[0].Rows[i]["HoursAllocated"]);
                        }
                        catch
                        {
                            t.Error = true;

                            errors.Add("Error: Invalid HoursAllocated value.");
                        }
                    }

                    if (ds.Tables[0].Columns.Contains("OwnerComments"))
                    {
                        t.OwnerComments = ds.Tables[0].Rows[i]["OwnerComments"].ToString();
                    }

                    if (ds.Tables[0].Columns.Contains("AssigneeComments"))
                    {
                        t.AssigneeComments = ds.Tables[0].Rows[i]["AssigneeComments"].ToString();
                    }

                    if (ds.Tables[0].Columns.Contains("TaskType"))
                    {
                        // TODO
                    }

                    if (ds.Tables[0].Rows[i][programFld].ToString().Length == 0)
                    {
                        t.Error = true;
                        errors.Add("Error: Program must be specified.");
                    }
                    else
                    {
                        program program = _db.GetContext().programs.SingleOrDefault(x => x.title.ToUpper() == ds.Tables[0].Rows[i][programFld].ToString().ToUpper());

                        if (program != null)
                        {
                            t.Program = _mediator.GetAdminProcessor().CreateProgramDTO(program);
                        }
                        else
                        {
                            t.Error = true;
                            errors.Add("Error: Program " + ds.Tables[0].Rows[i][programFld].ToString() + " does not exist");
                            t.Program = new Program();
                            t.Program.Title = ds.Tables[0].Rows[i][programFld].ToString();
                        }
                    }

                    if (ds.Tables[0].Columns.Contains("R/E") &&
                        ds.Tables[0].Rows[i]["R/E"].ToString().Length > 0)
                    {
                        // TODO
                    }

                    if (ds.Tables[0].Columns.Contains("StartDate") &&
                        ds.Tables[0].Rows[i]["StartDate"].ToString().Length > 0)
                    {
                        try
                        {
                            t.StartDate = DateTime.Parse(ds.Tables[0].Rows[i]["StartDate"].ToString());
                        }
                        catch
                        {
                            t.Error = true;

                            errors.Add("Error: Invalid Start Date.");
                        }
                    }

                    if (ds.Tables[0].Columns.Contains("DueDate") &&
                        ds.Tables[0].Rows[i]["DueDate"].ToString().Length > 0)
                    {
                        try
                        {
                            t.DueDate = DateTime.Parse(ds.Tables[0].Rows[i]["DueDate"].ToString());
                        }
                        catch
                        {
                            t.Error = true;

                            errors.Add("Error: Invalid Due Date.");
                        }
                    }

                    errors.AddRange(warnings);
                    t.ErrorMessage = errors;
                                       
                    // If WBS is defined, then try and find the parent task and add to that list

                    if ((t.WBS.Length > 0) && (t.WBS.Contains(".")))
                    {
                        string parent = t.WBS.Substring(0, t.WBS.LastIndexOf("."));

                        if (wbs_map.ContainsKey(parent))
                        {
                            if (wbs_map[parent].Error)
                            {
                                t.Error = true;
                            }

                            wbs_map[parent].Children.Add(t);
                        }
                        else
                        {
                            tasks.Add(t);
                        }
                    }
                    else
                    {
                        tasks.Add(t);
                    }
                }

                conn.Close();
            }
            catch (Exception e)
            {
                throw e;
            }

            return tasks;
        }

        private Profile SearchProfiles(string name)
        {
            string nameFix = name.Trim().ToUpper();
            DTO.Profile profile = null;
            profile p = _db.GetContext().profiles.SingleOrDefault(
                x => x.display_name.ToUpper() == nameFix || x.username.ToUpper() == nameFix || x.employee_id == nameFix);

            if (p != null)
            {
                profile = _mediator.GetProfileProcessor().CreateProfileDTO(p, false, false);
            }
            else
            {
                profile = _mediator.GetProfileProcessor().GetProfileByADLookup(name);
            }

            return profile;
        }
    }
}
