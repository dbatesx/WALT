using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using WALT.DTO;

namespace WALT.UIL.DataSources
{
    public class AlertDataSource : ObjectDataSource
    {
        public enum AlertView { UNREAD, READ, SENT }

        private AlertView _view;
        private int _count;
        private Alert.ColumnEnum? _sort;
        private Dictionary<Alert.ColumnEnum, string> _filters;
        private bool _order;

        public AlertDataSource(int view, string sort, bool order)
        {
            this.TypeName = "WALT.UIL.DataSources.AlertDataSource";
            this.DataObjectTypeName = "WALT.DTO.Alert";
            this.SelectMethod = "SelectAlerts";
            this.SelectCountMethod = "GetCount";
            this.EnablePaging = true;
            this.ObjectCreating += new ObjectDataSourceObjectEventHandler(AlertDataSource_OnObjectCreating);

            _view = (AlertView)view;
            _sort = (Alert.ColumnEnum)(sort != string.Empty ? Enum.Parse(typeof(Alert.ColumnEnum), sort) : null);
            _order = order;
            _filters = new Dictionary<Alert.ColumnEnum, string>();
        }

        public void SetView(int view)
        {
            _view = (AlertView)view;
        }

        public void SetSort(string column, bool order)
        {
            _sort = (Alert.ColumnEnum)(column != string.Empty ? Enum.Parse(typeof(Alert.ColumnEnum), column) : null);
            _order = order;
        }

        public void AddFilter(Alert.ColumnEnum column, string filter)
        {
            _filters[column] = filter;
        }

        public void ClearFilters()
        {
            _filters.Clear();
        }

        public List<Alert> SelectAlerts(Int32 startRowIndex, Int32 maximumRows)
        {
            if (_view == AlertView.SENT)
            {
                return WALT.BLL.ProfileManager.GetInstance().GetAlertListByCreator(_sort, _order, startRowIndex, maximumRows, ref _count, _filters);
            }
            else
            {
                return WALT.BLL.ProfileManager.GetInstance().GetAlertList(
                    (_view == AlertView.READ), _sort, _order, startRowIndex, maximumRows, ref _count, _filters);
            }
        }

        public int GetCount()
        {
            return _count;
        }

        void AlertDataSource_OnObjectCreating(object sender, ObjectDataSourceEventArgs e)
        {
            e.ObjectInstance = this;
        }
    }
}