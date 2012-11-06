using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace WALT.UIL.DataSources
{
    public class TaskListDataSource : ObjectDataSource
    {
        int _count;
        DTO.Task.ColumnEnum? _sort;
        DTO.Task.StatusEnum? _status;
        DTO.Profile _assigned;
        DTO.Profile _owner;
        bool _ascending;
        bool _recursive;
        Dictionary<DTO.Task.ColumnEnum, string> _filters;
        private List<DTO.Task> _taskList;

        public TaskListDataSource(DTO.Profile assigned, DTO.Profile owner, DTO.Task.StatusEnum? status,
            DTO.Task.ColumnEnum? sort, bool ascending, bool recursive, Dictionary<DTO.Task.ColumnEnum, string> filters)
            : this(assigned, owner, status, sort, ascending, recursive)
        {
            _filters = filters;
        }

        public TaskListDataSource(DTO.Profile assigned, DTO.Profile owner, DTO.Task.StatusEnum? status, 
            DTO.Task.ColumnEnum? sort, bool ascending, bool recursive)
        {
            this.TypeName = "WALT.UIL.DataSources.TaskListDataSource";
            this.DataObjectTypeName = "WALT.DTO.Task";
            this.SelectMethod = "SelectRows";
            this.SelectCountMethod = "GetCount";
            this.StartRowIndexParameterName = "start";
            this.MaximumRowsParameterName = "size";
            this.EnablePaging = true;
            this.ObjectCreating += new ObjectDataSourceObjectEventHandler(TaskListDataSource_OnObjectCreating);

            _assigned = assigned;
            _owner = owner;
            _sort = sort;
            _status = status;
            _ascending = ascending;
            _recursive = recursive;
        }

        public int GetCount()
        {
            return _count;
        }

        public List<DTO.Task> SelectRows(int start, int size)
        {
            _taskList = BLL.TaskManager.GetInstance().GetTaskList(
                _assigned, _owner, _status, _sort, _ascending, start, size, _recursive, ref _count, _filters);

            return _taskList;
        }

        void TaskListDataSource_OnObjectCreating(Object sender, ObjectDataSourceEventArgs e)
        {
            e.ObjectInstance = this;
        }

        public DTO.Task GetTask(int idx)
        {
            if (_taskList != null && _taskList.Count > idx)
            {
                return _taskList[idx];
            }

            return null;
        }
    }
}