using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace WALT.Test
{
    /// <summary>
    /// Summary description for TaskManagerTest
    /// </summary>
    [TestClass]
    public class TaskManagerTest
    {
        public TaskManagerTest()
        {
            BLL.Manager.Init();
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void PopulateTaskTableTest()
        {
            DataPopulator p = new DataPopulator();
            p.GenerateTasks(BLL.ProfileManager.GetInstance().GetProfile("DULLES\\siegela"), 10);
        }

        [TestMethod]
        public void SelectTaskListTest()
        {
            Debug.WriteLine("SelectTaskListTest");
            Debug.WriteLine("Test start = " + DateTime.Now);
            List<DTO.Task> items = BLL.TaskManager.GetInstance().GetTaskList(
                BLL.TaskManager.GetInstance().GetProfile());
            Debug.WriteLine("Test stop = " + DateTime.Now);
        }

        [TestMethod]
        public void SelectFilteredTaskListTest()
        {
            DTO.Profile p = BLL.ProfileManager.GetInstance().GetProfile();
            Dictionary<DTO.Task.ColumnEnum, string> filters = new Dictionary<DTO.Task.ColumnEnum, string>();
            filters.Add(DTO.Task.ColumnEnum.TITLE, "Grid");
            int count = 0;
            List<DTO.Task> tasks = BLL.TaskManager.GetInstance().GetTaskList(
                p, null, null, DTO.Task.ColumnEnum.ASSIGNED, false, 0, 0, false, ref count, filters);
            Debug.WriteLine("Number of filtered items returned = " + tasks.Count);
        }

        [TestMethod]
        public void GetTaskListTest()
        {
            List<DTO.Profile> profiles = BLL.ProfileManager.GetInstance().GetProfileList();

            foreach (DTO.Profile p in profiles)
            {
                List<DTO.Task> items = BLL.TaskManager.GetInstance().GetTaskList(p);

                int size = 0;

                foreach (DTO.Task t in items)
                {
                    size += t.GetSize();
                }

                Debug.WriteLine(p.DisplayName + ": " + items.Count() + " items = " + size + " bytes");
            }
        }

        [TestMethod]
        public void DeleteTask()
        {
            DTO.Task task = BLL.TaskManager.GetInstance().GetTask(581, false);
            BLL.TaskManager.GetInstance().DeleteTask(task);
        }
    }
}
