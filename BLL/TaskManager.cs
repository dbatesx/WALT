using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WALT.DTO;
using System.IO;
using System.Collections;

namespace WALT.BLL
{
    /// <summary>
    /// The TaskManager class inherits from the Manager class.
    /// </summary>
    public partial class TaskManager : Manager
    {
        private static string _id = typeof(TaskManager).ToString();

        #pragma warning disable 1591
        public enum PermissionType { None, Owner, Assignee, Both }
        #pragma warning restore 1591

        /// <summary>
        /// The TaskManger class should be accessed only through the GetInstance class 
        /// </summary>
        /// <returns>An Instance of the TaskManager, _dalMediator, _identity, _profile and the _logger </returns>
        public static TaskManager GetInstance()
        {
            TaskManager m = (TaskManager)GetSessionValue(_id);

            if (m == null)
            {
                m = new TaskManager();
                SetSessionValue(_id, m);
            }

            return m;
        }

        /// <summary>
        /// 
        /// </summary>
        private TaskManager()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Favorite> GetFavorites()
        {
            List<Favorite> favorites = null;

            try
            {
                favorites = _dalMediator.GetTaskProcessor().GetFavorites(_profile.Id);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return favorites;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileID"></param>
        /// <returns></returns>
        public List<Favorite> GetFavorites(long profileID)
        {
            List<Favorite> favorites = null;

            try
            {
                favorites = _dalMediator.GetTaskProcessor().GetFavorites(profileID);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
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
            List<Favorite> favorites = null;

            try
            {
                favorites = _dalMediator.GetTaskProcessor().GetFavorites(profileID, template);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return favorites;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="favId"></param>
        /// <returns></returns>
        public Favorite GetFavorite(long favId)
        {
            Favorite favorite = null;

            try
            {
                favorite = _dalMediator.GetTaskProcessor().GetFavorite(favId);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return favorite;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileID"></param>
        /// <returns></returns>
        public List<WeeklyTask> GetTaskTemplates(long profileID)
        {
            List<WeeklyTask> tasks = new List<WeeklyTask>();
            List<Favorite> templates = GetFavorites(profileID, true);

            foreach (Favorite fav in templates)
            {
                tasks.Add(FavoriteToWeeklyTask(fav));
            }

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="favID"></param>
        /// <returns></returns>
        public WeeklyTask GetTaskTemplate(long favID)
        {
            WeeklyTask wt = null;
            Favorite fav = _dalMediator.GetTaskProcessor().GetFavorite(favID);

            if (fav != null)
            {
                wt = FavoriteToWeeklyTask(fav);
            }

            return wt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fav"></param>
        /// <returns></returns>
        private WeeklyTask FavoriteToWeeklyTask(Favorite fav)
        {
            WeeklyTask wt = new WeeklyTask();
            wt.Task = new Task();
            wt.Task.Id = fav.Id;
            wt.Task.Title = fav.Title;
            wt.Task.TaskType = fav.TaskType;
            wt.Task.Program = fav.Program;
            wt.Task.Hours = fav.Hours;
            wt.Task.BaseTask = true;

            if (fav.Complexity != null)
            {
                wt.Task.Complexity = fav.Complexity;
                wt.Task.Estimate = fav.Complexity.Hours;
            }
            else
            {
                wt.Task.Estimate = fav.Estimate;
            }

            wt.Task.Status = Task.StatusEnum.OPEN;

            for (int i = 0; i < 7; i++)
            {
                wt.PlanHours.Add(i, fav.PlanHours[i]);
                wt.ActualHours.Add(i, 0);
            }

            wt.PlanDayComplete = fav.Template ? 0 : -1;
            wt.ActualDayComplete = -1;

            return wt;
        }


        /// <summary>
        /// Return a Task object based on a given ID.
        /// </summary>
        /// <param name="id">The unique ID of the Task to return.</param>
        /// <param name="recursive"></param>
        /// <returns>The Task object with the given id.</returns>
        public Task GetTask(long id, bool recursive)
        {
            return _dalMediator.GetTaskProcessor().GetTask(id, recursive);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="recursive"></param>
        /// <param name="titleOnly"></param>
        /// <returns></returns>
        public Task GetTask(long id, bool recursive, bool titleOnly)
        {
            return _dalMediator.GetTaskProcessor().GetTask(id, null, recursive, titleOnly);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public Task GetTaskByWeeklyTaskID(long id, bool recursive)
        {
            return _dalMediator.GetTaskProcessor().GetTaskByWeeklyTaskID(id, recursive);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public long GetTaskIdByWeeklyTaskId(long id)
        {
            return _dalMediator.GetTaskProcessor().GetTaskIdByWeeklyTaskId(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task GetRootTask(Task child, Profile user)
        {
            // TODO: Needs to get highest parent you have permission to see (either owner or assignee)

            return _dalMediator.GetTaskProcessor().GetRootTask(child, user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assigned"></param>
        /// <param name="owner"></param>
        /// <param name="status"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="recursive"></param>
        /// <param name="count"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public List<DTO.Task> GetTaskList(
            DTO.Profile assigned, DTO.Profile owner, Task.StatusEnum? status,
            Task.ColumnEnum? sort, bool ascending, int start, int size,
            bool recursive, ref int count, Dictionary<Task.ColumnEnum, string> filters)
        {
            return _dalMediator.GetTaskProcessor().GetTaskList(
                assigned, owner, status, sort, ascending, start, size, recursive, ref count, filters);
        }

        /// <summary>
        /// Gets task list for logged in user.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<Task> GetTaskList(Task.StatusEnum status)
        {
            return GetTaskList(_profile, status);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public List<Task> GetTaskList(Profile profile)
        {
            return GetTaskList(profile, null);
        }

        /// <summary>
        /// Gets task list for provided user by status.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<Task> GetTaskList(string profile, Task.StatusEnum status)
        {
            return GetTaskList(BLL.ProfileManager.GetInstance().GetProfile(profile), status);
        }

        /// <summary>
        /// Gets task list for provided user by status.
        /// </summary>
        /// <param name="profile_id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<Task> GetTaskList(long profile_id, Task.StatusEnum status)
        {
            return GetTaskList(BLL.ProfileManager.GetInstance().GetProfile(profile_id), status);
        }

        /// <summary>
        /// Accesses the GetTaskList() from the task processor.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="status"></param>
        /// <returns> A list of Tasks</returns>
        public List<Task> GetTaskList(Profile profile, Task.StatusEnum? status)
        {
            List<Task> task = new List<Task>();

            try
            {
                task = _dalMediator.GetTaskProcessor().GetTaskList(profile, status);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return task;
        }

        /// <summary>
        /// Accesses the GetTaskList() from the task processor.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="status"></param>
        /// <param name="sort"></param>
        /// <param name="ascending"></param>
        /// <returns> A list of Tasks</returns>
        public List<Task> GetTaskList(Profile profile, Task.StatusEnum status,Task.ColumnEnum sort, bool ascending)
        {
            List<Task> task = new List<Task>();

            try
            {
                task = _dalMediator.GetTaskProcessor().GetTaskList(profile, status, sort, ascending);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public List<Task> GetTaskListByOwner(Profile owner)
        {
            return GetTaskListByOwner(owner, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assignee"></param>
        /// <param name="currTask"></param>
        /// <returns></returns>
        public Dictionary<long, string> GetParentableTaskList(Profile assignee, Task currTask)
        {
            return _dalMediator.GetTaskProcessor().GetParentableTaskList(assignee, currTask);
        }

        /// <summary>
        /// Accesses the GetTaskList() from the task processor.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="status"></param>
        /// <returns> A list of Tasks</returns>
        public List<Task> GetTaskListByOwner(Profile owner, Task.StatusEnum? status)
        {
            List<Task> task = new List<Task>();

            try
            {
                ValidateProfile(owner);

                task = _dalMediator.GetTaskProcessor().GetTaskListByOwner(owner, status);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return task;
        }

        /// <summary>
        /// Accesses the GetTaskList() from the task processor.
        /// </summary>
        /// <returns>A list of Tasks</returns>
        public List<Task> GetUnplannedTaskList(List<long> weeklyTasksInPlan, List<long> tasksInPlan)
        {
            List<Task> tasks = new List<Task>();

            try
            {
                tasks = _dalMediator.GetTaskProcessor().GetUnplannedTaskList(_profile, weeklyTasksInPlan, tasksInPlan);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="weeklyTasksInPlan"></param>
        /// <param name="tasksInPlan"></param>
        /// <returns></returns>
        public List<Task> GetUnplannedTaskList(long profileId, List<long> weeklyTasksInPlan, List<long> tasksInPlan)
        {
            List<Task> tasks = new List<Task>();

            try
            {
                Profile profile = new Profile();
                profile.Id = profileId;
                tasks = _dalMediator.GetTaskProcessor().GetUnplannedTaskList(profile, weeklyTasksInPlan, tasksInPlan);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="weekEnding"></param>
        /// <returns></returns>
        public List<DTO.Task> GetOpenTasksByWeek(long profileId, DateTime weekEnding)
        {
            return _dalMediator.GetTaskProcessor().GetOpenTasksByWeek(profileId, weekEnding);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskTypeId"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public List<Complexity> GetComplexityList(long taskTypeId, bool active)
        {
            List<Complexity> complexities = new List<Complexity>();

            try
            {
                Team team = _dalMediator.GetAdminProcessor().GetTeam(_profile);
                long? id = _dalMediator.GetAdminProcessor().GetDirectorateId(team.Id);
                if (!id.HasValue) id = team.Id;
                complexities = _dalMediator.GetTaskProcessor().GetComplexityList(id.Value, taskTypeId, active);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return complexities;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="taskTypeId"></param>
        /// <returns></returns>
        public List<Complexity> GetComplexityList(long teamId, long taskTypeId)
        {
            List<Complexity> complexities = new List<Complexity>();

            try
            {
                complexities = _dalMediator.GetTaskProcessor().GetComplexityList(teamId, taskTypeId);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return complexities;
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
            List<Complexity> complexities = new List<Complexity>();

            try
            {
                complexities = _dalMediator.GetTaskProcessor().GetComplexityList(teamId, taskTypeId, active);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return complexities;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public List<Complexity> GetComplexityList(Team team, bool active)
        {
            List<Complexity> complexities = new List<Complexity>();

            try
            {
                long? id = _dalMediator.GetAdminProcessor().GetDirectorateId(team.Id);
                if (!id.HasValue) id = team.Id;
                complexities = _dalMediator.GetTaskProcessor().GetComplexityList(id.Value, active);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return complexities;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        public List<Complexity> GetComplexityList(bool active)
        {
            List<Complexity> complexities = new List<Complexity>();

            try
            {
                Team team = _dalMediator.GetAdminProcessor().GetTeam(_profile);
                long? id = _dalMediator.GetAdminProcessor().GetDirectorateId(team.Id);
                if (!id.HasValue) id = team.Id;
                complexities = _dalMediator.GetTaskProcessor().GetComplexityList(id.Value, active);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return complexities;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="complexityId"></param>
        /// <returns></returns>
        public Complexity GetComplexity(long complexityId)
        {
            Complexity complexity = new Complexity();

            try
            {
                complexity = _dalMediator.GetTaskProcessor().GetComplexity(complexityId);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
            return complexity;
        }
        /// <summary>
        /// Accesses the GetTeamWeeklyPlanList(Team team, DateTime weekEnding) from the task processor.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="weekEnding"></param>
        /// <returns> A list of the WeeklyPlan</returns>
        public List<WeeklyPlan> GetTeamWeeklyPlanList(Team team, DateTime weekEnding)
        {
            List<WeeklyPlan> weeklyPlan = new List<WeeklyPlan>();

            try
            {
                ValidateTeam(team);

                if (weekEnding == null)
                {
                    throw new Exception("Week Ending cannot be empty");
                }

                weeklyPlan = _dalMediator.GetTaskProcessor().GetTeamWeeklyPlanList(team, weekEnding);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return weeklyPlan;
        }

        /// <summary>
        /// Accesses the GetWeeklyPlan(DateTime weekEnding) from the task processor.
        /// </summary>
        /// <param name="weekEnding"></param>
        /// <returns> A list of the WeeklyPlans</returns>
        public WeeklyPlan GetWeeklyPlan(DateTime weekEnding)
        {
            return GetWeeklyPlan(weekEnding, GetProfile());
        }

        /// <summary>
        /// Accesses the GetWeeklyPlan(DateTime weekEnding) from the task processor.
        /// </summary>
        /// <param name="weekEnding"></param>
        /// <param name="profile"></param>
        /// <returns> A list of the WeeklyPlans</returns>
        public WeeklyPlan GetWeeklyPlan(DateTime weekEnding, Profile profile)
        {
            WeeklyPlan weeklyPlan = null;

            try
            {
                if (weekEnding == null)
                {
                    throw new Exception("Week Ending cannot be empty");
                }

                weeklyPlan = _dalMediator.GetTaskProcessor().GetWeeklyPlan(weekEnding, profile);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return weeklyPlan;
        }

        /// <summary>
        /// Accesses the GetWeeklyPlan(long id) from the task processor.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loadTasks"></param>
        /// <returns> A list of the WeeklyPlans</returns>
        public WeeklyPlan GetWeeklyPlan(long id, bool loadTasks)
        {
            WeeklyPlan weeklyPlan = null;

            try
            {
                weeklyPlan = _dalMediator.GetTaskProcessor().GetWeeklyPlan(id, loadTasks);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return weeklyPlan;
        }

        /// <summary>
        /// Accesses the GetWeeklyPlanTaskList(Profile profile, DateTime weekEnding) from the task processor.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="weekEnding"></param>
        /// <returns> The weekly plan for the requested profile and weekending.</returns>
        public WeeklyPlan GetWeeklyPlan(Profile profile, DateTime weekEnding)
        {
            WeeklyPlan weeklyPlan = new WeeklyPlan();

            try
            {
                ValidateProfile(profile);
                weeklyPlan = _dalMediator.GetTaskProcessor().GetWeeklyPlan(weekEnding, profile);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return weeklyPlan;
        }

        /// <summary>
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="weekEnding"></param>
        /// <returns> The weekly plan for the requested profile and weekending.</returns>
        public WeeklyPlan GetWeeklyPlan(long profileId, DateTime weekEnding)
        {
            WeeklyPlan weeklyPlan = new WeeklyPlan();
            Profile profile = new Profile();
            profile.Id = profileId;

            try
            {
                weeklyPlan = _dalMediator.GetTaskProcessor().GetWeeklyPlan(weekEnding, profile);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return weeklyPlan;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="weekEnding"></param>
        /// <returns></returns>
        public DateTime? GetWeeklyPlanModified(long profileId, DateTime weekEnding)
        {
            return _dalMediator.GetTaskProcessor().GetWeeklyPlanModified(profileId, weekEnding);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="weekEnding"></param>
        /// <returns></returns>
        public WeeklyPlan.StatusEnum? GetWeeklyPlanStatus(long profileId, DateTime weekEnding)
        {
            return _dalMediator.GetTaskProcessor().GetWeeklyPlanStatus(profileId, weekEnding);
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

            try
            {
                plans = _dalMediator.GetTaskProcessor().GetWeeklyPlans(weekEnding.Date, teamID);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return plans;
        }

        /// <summary>
        /// Accesses the GetWeeklyTask(long id) from the task processor.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A single WeeklyTask</returns>
        public WeeklyTask GetWeeklyTask(long id)
        {
            WeeklyTask weeklyTask = null;

            try
            {
                weeklyTask = _dalMediator.GetTaskProcessor().GetWeeklyTask(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return weeklyTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WeeklyBarrier GetWeeklyBarrier(long id)
        {
            WeeklyBarrier wb = null;

            try
            {
                wb = _dalMediator.GetTaskProcessor().GetWeeklyBarrier(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return wb;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        public void DeleteFavorites(List<long> ids)
        {
            if (ids.Count == 0) return;

            try
            {
                _dalMediator.GetTaskProcessor().DeleteFavorites(ids);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns>A boolean indicating if task's status can be set to close.</returns>
        public bool IsTaskAllowedToBeClosed(Task task)
        {
            bool allowedToBeClosed = false;

            if (task == null)
            {
                throw new Exception("Task cannot be null.");
            }         

            try
            {
                allowedToBeClosed = _dalMediator.GetTaskProcessor().IsTaskAllowedToBeClosed(task.Id);            
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return allowedToBeClosed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool IsTaskAllowedToBeRejected(Task task, PermissionType? pt)
        {
            if (task == null)
            {
                throw new Exception("Task cannot be null.");
            }

            if (!pt.HasValue)
            {
                pt = GetTaskPermission(task);
            }

            return (!task.Instantiated && !IsTaskPlanned(task) && task.Status == DTO.Task.StatusEnum.OPEN &&
                (pt.Value == PermissionType.Assignee || (pt.Value == PermissionType.Both && task.Owner.Id != _profile.Id)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool IsTaskAllowedToBeDeleted(Task task, PermissionType? pt)
        {
            bool allowedToBeDeleted = false;

            if (task == null)
            {
                throw new Exception("Task cannot be null.");
            }

            if (!pt.HasValue)
            {
                pt = GetTaskPermission(task);
            }

            if (task.Instantiated || (pt.Value != PermissionType.Both && pt.Value != PermissionType.Owner))
            {
                return false;
            }

            try
            {
                allowedToBeDeleted = _dalMediator.GetTaskProcessor().IsTaskAllowedToBeDeleted(task.Id);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            return allowedToBeDeleted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool IsTaskPlanned(Task task)
        {
            return _dalMediator.GetTaskProcessor().IsTaskPlanned(task);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        public void RejectTask(Task task)
        {
            if (task == null)
            {
                throw new Exception("Task cannot be blank.");
            }

            try
            {
                task.Assigned = task.Owner;
                task.Status = Task.StatusEnum.REJECTED;
                ValidateTask(task);
                _dalMediator.GetTaskProcessor().SaveTask(task, _profile, false, true);

                Alert(task.Owner, "Task Rejected", "Task \"" + task.Title + "\" was rejected by " + _profile.DisplayName,
                    DTO.Alert.AlertEnum.TASK, task.Id);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Accesses the ImportTasksFromExcel(FileStream F) from the task processor.
        /// </summary>
        /// <returns> A boolean indicating success or failure</returns>
        public List<DTO.Task> ImportTasksFromExcel(string filename)//, List<List<string>> errors)
        {
            List<DTO.Task> tasks = new List<Task>();

            try
            {
                tasks = _dalMediator.GetTaskProcessor().ImportFromExcel(filename, _profile);//, errors);
                _logger.LogInfo(tasks.Count + " tasks imported from Excel file: " + filename);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="favorites"></param>
        public void SaveFavorites(List<Favorite> favorites)
        {
            try
            {
                foreach (Favorite fav in favorites)
                {
                    ValidateFavorite(fav);
                }

                _dalMediator.GetTaskProcessor().SaveFavorites(favorites, _profile);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskIDs"></param>
        public void SaveTasksAsFavorites(List<long> taskIDs)
        {
            if (taskIDs.Count == 0) return;

            try
            {
                _dalMediator.GetTaskProcessor().SaveTasksAsFavorites(taskIDs, _profile);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fav"></param>
        public void ValidateFavorite(Favorite fav)
        {
            string errors = string.Empty;

            if (fav.Title == string.Empty)
            {
                errors = "Favorite titles can not be blank<br />";
            }

            if (fav.Template && (fav.Program == null || fav.TaskType == null ||
                (fav.Complexity == null && fav.Estimate <= 0) || (fav.Complexity != null && fav.Complexity.Id == 0)))
            {
                if (fav.Complexity == null)
                {
                    errors += "Favorite " + fav.Title + " must have Task Type, Program, and R/E fields set to be a template<br />";
                }
                else
                {
                    errors += "Favorite " + fav.Title + " must have Task Type, Program, and Complexity fields set to be a template<br />";
                }
            }

            if (errors != string.Empty)
            {
                throw new Exception(errors);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        public void SaveTask(Task task)
        {
            DTO.Task old = task.Id > 0 ? _dalMediator.GetTaskProcessor().GetTask(task.Id, false) : null;

            try
            {
                ValidateTask(task);
                _dalMediator.GetTaskProcessor().SaveTask(task, _profile, false, true);
                _logger.LogComment(task, "Task for " + _profile.DisplayName + " saved");
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }

            bool ownerAlerted = false;
            bool assigneeAlerted = false;

            // If owner changes notify the new owner.

            if (task.Owner != null && task.Owner.Id != _profile.Id &&
                (old == null || old.Owner == null || old.Owner.Id != task.Owner.Id))
            {
                ownerAlerted = true;
                Alert(task.Owner, "Task Transferred",
                    "Ownership of task \"" + task.Title + "\" was transferred to you from " + _profile.DisplayName,
                    DTO.Alert.AlertEnum.TASK, task.Id);
            }

            // If assignee changes notify the new assignee and owner

            if (task.Assigned != null &&
                (old == null || old.Assigned == null || old.Assigned.Id != task.Assigned.Id))
            {
                if (task.Assigned.Id != _profile.Id)
                {
                    assigneeAlerted = true;
                    Alert(task.Assigned, "Task Assigned",
                        "Task \"" + task.Title + "\" was assigned to you by " + _profile.DisplayName,
                        DTO.Alert.AlertEnum.TASK, task.Id);
                }

                if (!ownerAlerted && task.Owner.Id != task.Assigned.Id && task.Owner.Id != _profile.Id)
                {
                    ownerAlerted = true;
                    Alert(task.Owner, "Task Reassigned",
                        "Task \"" + task.Title + "\" was reassigned to " + task.Assigned.DisplayName + " by " + _profile.DisplayName,
                        DTO.Alert.AlertEnum.TASK, task.Id);
                }
            }

            // if no reassignement, notify owner and assignee of updates

            if (!ownerAlerted && task.Owner != null && task.Owner.Id != _profile.Id)
            {
                Alert(task.Owner, "Task Updated", "The task \"" + task.Title + "\" was modified by " + _profile.DisplayName,
                    DTO.Alert.AlertEnum.TASK, task.Id);
            }

            if (!assigneeAlerted && task.Assigned != null && task.Assigned.Id != _profile.Id &&
                (task.Owner == null || task.Owner.Id != task.Assigned.Id))
            {
                Alert(task.Assigned, "Task Updated", "The task \"" + task.Title + "\" was modified by " + _profile.DisplayName,
                    DTO.Alert.AlertEnum.TASK, task.Id);
            }
        }

        /// <summary>
        /// Accesses the SaveTaskList(List of Task listTask) from the task processor.
        /// </summary>
        /// <param name="listTask"></param>
        /// <returns> A boolean indicating success or failure</returns>
        public void SaveImportedTaskList(List<Task> listTask)
        {
            try
            {
                if (listTask.Count == 0)
                {
                    throw new Exception("List Task cannot be empty");
                }

                foreach (Task task in listTask)
                {
                    ValidateTask(task);
                }

                _dalMediator.GetTaskProcessor().SaveTaskList(listTask, _profile);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskIDs"></param>
        public void RejectTasks(List<long> taskIDs)
        {
            try
            {
                List<Task> rejectTasks = _dalMediator.GetTaskProcessor().RejectTasks(taskIDs);

                foreach (Task task in rejectTasks)
                {
                    Alert(task.Owner, "Task Rejected", "Task \"" + task.Title + "\" was rejected by " + _profile.DisplayName,
                        DTO.Alert.AlertEnum.TASK, task.Id);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Accesses the SaveWeeklyPlan(WeeklyPlan weeklyPlan) from the task processor.
        /// </summary>
        /// <param name="weeklyPlan"></param>
        /// <param name="updateLeave"></param>
        /// <param name="updateTaskIDs"></param>
        /// <param name="deleteTaskIDs"></param>
        /// <param name="deleteBarrierIDs"></param>
        /// <param name="approvingPlan"></param>
        /// <returns> A boolean indicating success or failure</returns>
        public void SaveWeeklyPlan(
            WeeklyPlan weeklyPlan,
            bool approvingPlan,
            bool updateLeave,
            List<long> updateTaskIDs,
            List<long> deleteTaskIDs,
            List<long> deleteBarrierIDs
        )
        {
            try
            {
                ValidateWeeklyPlan(weeklyPlan, approvingPlan, deleteTaskIDs, deleteBarrierIDs);
                _dalMediator.GetTaskProcessor().SaveWeeklyPlan(weeklyPlan, updateLeave, updateTaskIDs, deleteTaskIDs, deleteBarrierIDs, _profile);
                _logger.LogComment(weeklyPlan, "Weekly Plan for " + weeklyPlan.Profile.DisplayName + " saved");
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        protected bool ValidateTeam(Team team)
        {
            if (team == null)
            {
                throw new Exception("Team cannot be empty");
            }

            if (team.Name.Trim().Length == 0)
            {
                throw new Exception("Team Name cannot be empty");
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        protected bool ValidateProfile(Profile profile)
        {
            if (profile == null || profile.Id == 0)
            {
                throw new Exception("Profile cannot be empty");
            }

            if (profile.Username.Trim().Length == 0)
            {
                throw new Exception("UserName cannot be empty");
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected void ValidateTask(Task task)
        {
            if (task == null)
            {
                throw new Exception("Task cannot be blank.");
            }

            if (task.Title.Trim().Length == 0)
            {
                throw new Exception("Task Title cannot be empty " + task.Title);
            }

            if (task.Owner == null || task.Owner.Id == 0)
            {
                throw new Exception("Task Owner must be specified");
            }

            if (task.Program != null && task.Program.Id == 0)
            {
                throw new Exception("Specified program is not valid");
            }

            if (task.Id > 0)
            {
                DTO.Task old = _dalMediator.GetTaskProcessor().GetTask(task.Id, false);

                if (old != null)
                {
                    PermissionType pt = GetTaskPermission(old);

                    if (pt == PermissionType.None)
                    {
                        throw new Exception("You do not have access to modify this task");
                    }

                    if (task.Instantiated &&
                        ((old.Assigned != null && old.Assigned.Id != task.Assigned.Id) ||
                         old.Title != task.Title ||
                         (old.Program != null && old.Program.Title != task.Program.Title) ||
                         old.WBS != task.WBS ||
                         old.Hours != task.Hours ||
                         old.Estimate != task.Estimate ||
                         old.ExitCriteria != task.ExitCriteria ||
                         old.OwnerComments != task.OwnerComments ||
                         (old.TaskType != null && old.TaskType.Id != task.TaskType.Id)))
                    {
                        throw new Exception("You can't modify an instantiated task");
                    }

                    // Owner can only change owner of root task

                    if (old.Owner.Id != task.Owner.Id &&
                        pt != PermissionType.Owner && pt != PermissionType.Both)
                    {
                        throw new Exception("Only the task owner can reassign ownership");
                    }
                }
            }

            // If the task status is changed to complete and this is a parent
            // make sure all the children are complete or OBE.

            if (task.Status == Task.StatusEnum.COMPLETED && !VerifyChildrenAreComplete(task))
            {
                throw new Exception("All child tasks must be Completed or OBE in order to mark parent task Completed");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private bool VerifyChildrenAreComplete(DTO.Task task)
        {
            bool complete = true;

            for (int i = 0; i < task.Children.Count; i++)
            {
                if (task.Children[i].Status != Task.StatusEnum.COMPLETED &&
                    task.Children[i].Status != Task.StatusEnum.OBE)
                {
                    complete = false;
                }
                else
                {
                    complete = VerifyChildrenAreComplete(task.Children[i]);
                }
            }

            return complete;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weeklyPlan"></param>
        /// <param name="approvingPlan"></param>
        /// <param name="deleteTaskIDs"></param>
        /// <param name="deleteBarrierIDs"></param>
        protected void ValidateWeeklyPlan(
            WeeklyPlan weeklyPlan,
            bool approvingPlan,
            List<long> deleteTaskIDs,
            List<long> deleteBarrierIDs
        )
        {
            string errors = string.Empty;
            List<WeeklyTask> wtList = new List<WeeklyTask>();
            List<long> wtIDs = new List<long>();
            List<double> plans = new List<double>();
            List<double> actuals = new List<double>();
            bool planApprovalCheck = (weeklyPlan.State == WeeklyPlan.StatusEnum.PLAN_READY ||
                    (approvingPlan && weeklyPlan.State == WeeklyPlan.StatusEnum.PLAN_APPROVED));

            bool logApprovalCheck = (weeklyPlan.State == WeeklyPlan.StatusEnum.LOG_READY ||
                    weeklyPlan.State == WeeklyPlan.StatusEnum.LOG_APPROVED);

            wtList.AddRange(weeklyPlan.WeeklyTasks);
            wtIDs = wtList.Select(x => x.Id).ToList();

            if (planApprovalCheck || logApprovalCheck)
            {
                WeeklyPlan currPlan = GetWeeklyPlan(weeklyPlan.Id, true);

                if (currPlan != null)
                {
                    foreach (WeeklyTask wt in currPlan.WeeklyTasks)
                    {
                        if (!wtIDs.Contains(wt.Id) && !deleteTaskIDs.Contains(wt.Id))
                        {
                            wtList.Add(wt);
                        }
                    }
                }
            }

            for (int i = 0; i < 7; i++)
            {
                plans.Add(0);
                actuals.Add(0);
            }

            foreach (WeeklyTask wt in wtList)
            {
                double planSum = wt.PlanHours.Values.Sum();
                double actualSum = wt.ActualHours.Values.Sum();
                double barrierSum = 0;

                if (wt.Task.Status == Task.StatusEnum.HOLD)
                {
                    if (planSum > 0)
                    {
                        errors += "Task " + wt.Task.Title + " is on hold and can not have hours planned against it<br>";
                    }
                }
                else if (planApprovalCheck && wt.UnplannedCode == null && planSum == 0 && wt.PlanHours.Count > 0)
                {
                    errors += "Task " + wt.Task.Title + " does not have any hours planned<br>";
                }
                else if (logApprovalCheck && wt.UnplannedCode != null && actualSum == 0)
                {
                    errors += "Unplanned task " + wt.Task.Title + " has no hours logged against it<br>";
                }

                if (wt.Task.Status != Task.StatusEnum.OPEN && wt.Task.Status != Task.StatusEnum.HOLD &&
                    (weeklyPlan.State == WeeklyPlan.StatusEnum.NEW || weeklyPlan.State == WeeklyPlan.StatusEnum.PLAN_READY))
                {
                    errors += "Task " + wt.Task.Title + " can not be included in a plan because it is not open<br>";
                }

                if (!wt.Task.BaseTask)
                {
                    errors += "Task " + wt.Task.Title + " can not be included in a plan/log because it is not a base task<br>";
                }

                foreach (WeeklyBarrier wb in wt.Barriers)
                {
                    if (!deleteBarrierIDs.Contains(wb.Id))
                    {
                        double sum = wb.Hours.Values.Sum();

                        if (logApprovalCheck && sum == 0)
                        {
                            errors += "Barrier on task " + wt.Task.Title + " has no hours logged against it<br>";
                        }
                        else if (wb.BarrierType == WeeklyBarrier.BarriersEnum.EFFICIENCY)
                        {
                            barrierSum += sum;

                            foreach (int day in wb.Hours.Keys)
                            {
                                if (wb.Hours[day] > 0 &&
                                   (!wt.ActualHours.ContainsKey(day) || wb.Hours[day] > wt.ActualHours[day]))
                                {
                                    errors += "Barrier on task " + wt.Task.Title + " has more hours logged on " +
                                        weeklyPlan.WeekEnding.AddDays(day - 6).ToString("M/d") + " than actual hours on the task<br>";
                                }
                            }
                        }
                    }
                }

                if (actualSum > 0 && wt.Id >= 0)
                {
                    int compDay = 7;
                    double spent = actualSum + _dalMediator.GetTaskProcessor().GetTaskTotalSpent(wt.Task.Id, wt.Id);
                    double barrier = barrierSum + _dalMediator.GetTaskProcessor().GetTaskTotalBarriersHours(
                        wt.Task.Id, WeeklyBarrier.BarriersEnum.EFFICIENCY, wt.Barriers);

                    decimal exceed = decimal.Round(Convert.ToDecimal(spent - barrier - wt.Task.Estimate), 1);

                    if (exceed > 0)
                    {
                        errors += "Hours logged against task " + wt.Task.Title + " have exceeded its R/E. You must log at least " +
                            exceed.ToString() + " more hour(s) of efficiency barrier time against the task<br>";
                    }

                    if (wt.Task.Status == DTO.Task.StatusEnum.HOLD && wt.Task.OnHoldDate.HasValue)
                    {
                        compDay = 5 - ((TimeSpan)(weeklyPlan.WeekEnding - wt.Task.OnHoldDate.Value.Date)).Days;
                    }
                    else if (wt.Task.Status == DTO.Task.StatusEnum.OBE && wt.Task.CompletedDate.HasValue)
                    {
                        compDay = 5 - ((TimeSpan)(weeklyPlan.WeekEnding - wt.Task.CompletedDate.Value.Date)).Days;
                    }
                    else if (wt.Task.Status == DTO.Task.StatusEnum.COMPLETED)
                    {
                        if (wt.ActualDayComplete != -1)
                        {
                            compDay = wt.ActualDayComplete + 1;
                        }
                        else if (wt.Task.CompletedDate.HasValue &&
                            wt.Task.CompletedDate.Value.CompareTo(weeklyPlan.WeekEnding.AddDays(-6)) < 0)
                        {
                            compDay = 0;
                        }
                    }

                    bool past = false;
                    int day = compDay;

                    while (!past && day < 7)
                    {
                        if (wt.ActualHours.ContainsKey(day) && wt.ActualHours[day] > 0)
                        {
                            past = true;
                            errors += "Task " + wt.Task.Title + " can not have hours logged against it past " +
                                weeklyPlan.WeekEnding.AddDays(compDay - 6).ToShortDateString() + "<br>";
                        }

                        day++;
                    }
                }

                foreach (int day in wt.PlanHours.Keys)
                {
                    plans[day] += wt.PlanHours[day];
                }

                foreach (int day in wt.ActualHours.Keys)
                {
                    actuals[day] += wt.ActualHours[day];
                }
            }

            if (weeklyPlan.LeavePlanned.HasValue)
            {
                if (planApprovalCheck)
                {
                    if (weeklyPlan.LeavePlanned.Value && weeklyPlan.LeavePlanHours.Values.Sum() == 0)
                    {
                        errors += "No leave hours planned<br>";
                    }
                }
                else if (logApprovalCheck && !weeklyPlan.LeavePlanned.Value && weeklyPlan.LeaveActualHours.Values.Sum() == 0)
                {
                    errors += "No hours logged against Unplanned Leave<br>";
                }
            }

            for (int i = 0; i < 7; i++)
            {
                if (plans[i] > 24)
                {
                    errors += "Can not plan more than 24 hours in a day<br>";
                }

                if (actuals[i] > 24)
                {
                    errors += "Can not log more than 24 hours of actuals in a day<br>";
                }
            }

            if (errors != string.Empty)
            {
                throw new Exception(errors);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        public void DeleteTask(DTO.Task task)
        {
            if (task == null || task.Id == 0)
            {
                throw new Exception("Task is not defined");
            }

            if (task.Owner.Id != _profile.Id)
            {
                throw new Exception("You must be the task owner to delete the task");
            }

            if (task.Instantiated)
            {
                throw new Exception("You can't delete an instantiated task");
            }

            try
            {
                _dalMediator.GetTaskProcessor().DeleteTask(task.Id);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw e;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskIDs"></param>
        public void DeleteTasks(List<long> taskIDs)
        {
            try
            {
                _dalMediator.GetTaskProcessor().DeleteTasks(taskIDs);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw e;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public PermissionType GetTaskPermission(Task task)
        {
            if (task == null)
            {
                return PermissionType.None;
            }

            bool assigneeAdmin = false;
            bool ownerAdmin = false;

            if (task.Assigned != null)
            {
                if (task.Assigned.Id == _profile.Id)
                {
                    assigneeAdmin = true;
                }
                else
                {
                    DTO.Team assigneeTeam = BLL.AdminManager.GetInstance().GetTeam(task.Assigned, "MEMBER");
                    assigneeAdmin = BLL.AdminManager.GetInstance().IsTeamAdmin(assigneeTeam);
                }
            }

            if (task.Owner.Id == _profile.Id)
            {
                ownerAdmin = true;
            }
            else
            {
                DTO.Team ownerTeam = BLL.AdminManager.GetInstance().GetTeam(task.Owner, "MEMBER");
                ownerAdmin = BLL.AdminManager.GetInstance().IsTeamAdmin(ownerTeam);
            }

            if (assigneeAdmin && ownerAdmin)
            {
                return PermissionType.Both;
            }
            else if (assigneeAdmin)
            {
                return PermissionType.Assignee;
            }
            else if (ownerAdmin)
            {
                return PermissionType.Owner;
            }
            else
            {
                return PermissionType.None;
            }
        }
    }
}
