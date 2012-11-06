using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WALT.DTO;

namespace WALT.DAL
{
    public class Mediator
    {
        private Database _database;
        private ProfileProcessor _profileProcessor;
        private AdminProcessor _adminProcessor;
        private TaskProcessor _taskProcessor;
        private ReportProcessor _reportProcessor;

        public Mediator()
        {
            _database = new Database();
            _profileProcessor = new ProfileProcessor(this);
            _adminProcessor = new AdminProcessor(this);
            _taskProcessor = new TaskProcessor(this);
            _reportProcessor = new ReportProcessor(this);
        }

        public void Shutdown()
        {
            _database.Close();
        }

        public Database GetDatabase()
        {
            return _database;
        }

        public TaskProcessor GetTaskProcessor()
        {
            return _taskProcessor;
        }

        public AdminProcessor GetAdminProcessor()
        {
            return _adminProcessor;
        }

        public ProfileProcessor GetProfileProcessor()
        {
            return _profileProcessor;
        }

        public ReportProcessor GetReportProcessor()
        {
            return _reportProcessor;
        }

        public void Refresh()
        {
            _database.Refresh();
        }

        public bool Done()
        {            
            return _database.Done();
        }

        public int GetNumDatabaseConnections()
        {
            return _database.GetNumConnections();
        }
    }
}
