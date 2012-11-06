using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WALT.BLL;
using WALT.DTO;
using System.Xml;
//using System.Data;
//using System;


namespace WALT.Test
{
    [TestClass]
    public class ReportManagerTest
    {
        public ReportManagerTest()
        {
            BLL.Manager.Init();
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
        public void TestMethod1()
        {
        }

        /// <summary>
        ///A test for ReportManager Constructor
        ///</summary>
        [TestMethod()]
        public void ReportManagerConstructorTest()
        {
            ReportManager target = new ReportManager();
        }

        /// <summary>
        ///A test for AddMissingWeeks
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void AddMissingWeeksTest()
        {
            ReportManager_Accessor target = new ReportManager_Accessor(); // TODO: Initialize to an appropriate value
            List<DateTime> weekEnding = new List<DateTime>();

            DateTime newWeekEnding = new DateTime(2000, 01, 02);
            weekEnding.Add(newWeekEnding);
            weekEnding.Add(newWeekEnding.AddDays(7));
            weekEnding.Add(newWeekEnding.AddDays(14));
            weekEnding.Add(newWeekEnding.AddDays(21));
            weekEnding.Add(newWeekEnding.AddDays(28));

            List<OperatingReportGraph> profilePlans = new List<OperatingReportGraph>();

            // Add a plan for each week except the last.
            for (int i = 0; i < 4; i++)
            {
                OperatingReportGraph graph = new OperatingReportGraph();
                profilePlans.Add(graph);
                graph.dividend = i;
                graph.divisor = i + 1;
                graph.percentBase = 100;
                graph.percentGoal = 100;
                graph.weekAverage = i / (i + 1);
                graph.WeekEnding = newWeekEnding.AddDays(7 * i);
            }

            Report spec = new Report();
            spec.Description = "New report for test.";
            spec.IsMonthly = false;

            // Attempt to add that missing week.
            target.AddMissingWeeks(weekEnding, profilePlans, spec);

            // Check if the missing week was added or not.
            var found = from plan in profilePlans
                        where plan.WeekEnding == newWeekEnding.AddDays(28)
                        select plan;

            if (found.Count() < 1)
            {
                Assert.Fail("AddMissingWeeksTest: Week was not found in result.");
            }


            // Check monthly report
            weekEnding.Clear();
            weekEnding.Add(newWeekEnding);
            weekEnding.Add(newWeekEnding.AddMonths(1));
            weekEnding.Add(newWeekEnding.AddMonths(2));
            weekEnding.Add(newWeekEnding.AddMonths(3));
            weekEnding.Add(newWeekEnding.AddMonths(4));
            weekEnding.Add(newWeekEnding.AddMonths(5));

            profilePlans = new List<OperatingReportGraph>();

            // Add a plan for month week except the last.
            for (int i = 0; i < 6; i++)
            {
                OperatingReportGraph graph = new OperatingReportGraph();
                profilePlans.Add(graph);
                graph.dividend = i;
                graph.divisor = i + 1;
                graph.percentBase = 100;
                graph.percentGoal = 100;
                graph.weekAverage = i / (i + 1);
                graph.WeekEnding = newWeekEnding.AddMonths(1 * i);
            }

            spec = new Report();
            spec.Description = "New monthly report for test.";
            spec.IsMonthly = true;

            // Attempt to add that missing month.
            target.AddMissingWeeks(weekEnding, profilePlans, spec);

            // Check if the missing week was added or not.
            var month = from plan in profilePlans
                        where plan.WeekEnding == newWeekEnding.AddMonths(5)
                        select plan;

            if (month.Count() < 1)
            {
                Assert.Fail("AddMissingWeeksTest: Month was not found in result.");
            }

            //Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for AddMissingWeeks
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void AddMissingWeeksTest1()
        {
            ReportManager_Accessor target = new ReportManager_Accessor(); // TODO: Initialize to an appropriate value
            List<DateTime> weekEnding = new List<DateTime>();

            DateTime newWeekEnding = new DateTime(2000, 01, 02);
            weekEnding.Add(newWeekEnding);
            weekEnding.Add(newWeekEnding.AddDays(7));
            weekEnding.Add(newWeekEnding.AddDays(14));
            weekEnding.Add(newWeekEnding.AddDays(21));
            weekEnding.Add(newWeekEnding.AddDays(28));

            List<OperatingReportTable> profilePlans = new List<OperatingReportTable>();

            // Add a plan for each week except the last.
            for (int i = 0; i < 4; i++)
            {
                OperatingReportTable table = new OperatingReportTable();
                profilePlans.Add(table);
                table.dividend = i;
                table.divisor = i + 1;
                table.WeekEnding = newWeekEnding.AddDays(7 * i);
            }

            Report spec = new Report();
            spec.Description = "New report for test.";
            spec.IsMonthly = false;
            spec.Type = Report.TypeEnum.OPERATING_LOAD_TABLE;

            // Attempt to add that missing week.
            target.AddMissingWeeks(weekEnding, profilePlans, spec);

            // Check if the missing week was added or not.
            var found = from plan in profilePlans
                        where plan.WeekEnding == newWeekEnding.AddDays(28)
                        select plan;

            if (found.Count() < 1)
            {
                Assert.Fail("AddMissingWeeksTest1: Week was not found in result.");
            }


            // Check monthly report
            weekEnding.Clear();
            weekEnding.Add(newWeekEnding);
            weekEnding.Add(newWeekEnding.AddMonths(1));
            weekEnding.Add(newWeekEnding.AddMonths(2));
            weekEnding.Add(newWeekEnding.AddMonths(3));
            weekEnding.Add(newWeekEnding.AddMonths(4));
            weekEnding.Add(newWeekEnding.AddMonths(5));

            profilePlans = new List<OperatingReportTable>();

            // Add a plan for month week except the last.
            for (int i = 0; i < 6; i++)
            {
                OperatingReportTable table = new OperatingReportTable();
                profilePlans.Add(table);
                table.dividend = i;
                table.divisor = i + 1;
                table.WeekEnding = newWeekEnding.AddMonths(1 * i);
            }

            spec = new Report();
            spec.Description = "New monthly report for test.";
            spec.IsMonthly = true;
            spec.Type = Report.TypeEnum.OPERATING_LOAD_TABLE;

            // Attempt to add that missing month.
            target.AddMissingWeeks(weekEnding, profilePlans, spec);

            // Check if the missing week was added or not.
            var month = from plan in profilePlans
                        where plan.WeekEnding == newWeekEnding.AddMonths(5)
                        select plan;

            if (month.Count() < 1)
            {
                Assert.Fail("AddMissingWeeksTest1: Month was not found in result.");
            }

            //Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for BuildFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void BuildFilterTest()
        {
            ReportManager_Accessor target = new ReportManager_Accessor();
            XmlNode node = null; 
            //ReportFilter expected = new ReportFilter(); // TODO: Initialize to an appropriate value
            ReportFilter actual;
            actual = target.BuildFilter(node);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for DeleteFilterGroup
        ///</summary>
        [TestMethod()]
        public void DeleteFilterGroupTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            ReportGroup g = new ReportGroup();

            g.Id = 1;
            g.Description = "Test group";
            g.Directorates = new List<DTO.Directorate>();
            g.Name = "Test Group Name";
            g.Owner = BLL.AdminManager.GetInstance().GetProfile();
            g.Profiles = new List<DTO.Profile>();
            g.Public = false;
            g.Teams = new List<DTO.Team>();
            g.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            target.DeleteFilterGroup(g);
            //Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for DeleteReport
        ///</summary>
        [TestMethod()]
        public void DeleteReportTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report report = new Report();

            report.Id = 1;
            report.Description = "Test ParetoReportTable.";
            report.FromDate = new DateTime(2000, 01, 02);

            report.Group = new DTO.ReportGroup();
            report.Group.Id = 1;
            report.Group.Description = "Test ParetoReportTable group";
            report.Group.Directorates = new List<DTO.Directorate>();
            report.Group.Name = "Test ParetoReportTable Group Name"; ;
            report.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            report.Group.Profiles = new List<DTO.Profile>();
            report.Group.Public = false;
            report.Group.Teams = new List<DTO.Team>();
            report.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            report.IsMonthly = false;
            report.Owner = BLL.AdminManager.GetInstance().GetProfile();
            report.PercentBase = 100;
            report.PercentGoal = 100;
            report.Public = false;
            report.Title = "Test ParetoReportTable";
            report.ToDate = report.FromDate.Value.AddDays(7);
            report.Type = DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_RAW;

            target.DeleteReport(report);
            //Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for DownloadParetoReportDelayBarrier
        ///</summary>
        [TestMethod()]
        public void DownloadParetoReportDelayBarrierTest()
        {
            ReportManager target = ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test ParetoReportTable.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name";;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_RAW;

            List<ParetoReportTable> expected = new List<ParetoReportTable>();
            ParetoReportTable reportTable = new ParetoReportTable();
            expected.Add(reportTable);
            //reportTable.

            List<ParetoReportTable> actual;
            actual = target.DownloadParetoReportBarriers(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for DownloadParetoReportEfficiencyBarrier
        ///</summary>
        [TestMethod()]
        public void DownloadParetoReportEfficiencyBarrierTest()
        {
            ReportManager target = ReportManager.GetInstance();
            Report spec = new Report();

            spec.Description = "Test ParetoReportTable.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_RAW;

            List<ParetoReportTable> expected = new List<ParetoReportTable>();
            ParetoReportTable reportTable = new ParetoReportTable();
            expected.Add(reportTable);
            //reportTable.

            List<ParetoReportTable> actual;
            actual = target.DownloadParetoReportBarriers(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for DownloadParetoReportUnplannedCode
        ///</summary>
        [TestMethod()]
        public void DownloadParetoReportUnplannedCodeTest()
        {
            ReportManager target = ReportManager.GetInstance();
            Report spec = new Report();
           
            spec.Description = "Test ParetoReportTable.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.PARETO_UNPLANNED_RAW;

            List<ParetoReportTable> expected = new List<ParetoReportTable>();
            ParetoReportTable reportTable = new ParetoReportTable();
            expected.Add(reportTable);
            //reportTable.

            List<ParetoReportTable> actual;
            actual = target.DownloadParetoReportUnplannedCode(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for FilterToSQL
        ///</summary>
        [TestMethod()]
        public void FilterToSQLTest()
        {
            ReportManager target = new ReportManager(); // TODO: Initialize to an appropriate value
            ReportFilter filter = new ReportFilter(); // TODO: Initialize to an appropriate value
            //string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.FilterToSQL(filter);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for FilterToXML
        ///</summary>
        [TestMethod()]
        public void FilterToXMLTest()
        {
            ReportManager target = new ReportManager(); // TODO: Initialize to an appropriate value
            ReportFilter filter = new ReportFilter(); // TODO: Initialize to an appropriate value
            //string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.FilterToXML(filter);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetALTs
        ///</summary>
        [TestMethod()]
        public void GetALTsTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            //List<Team> expected = null; // TODO: Initialize to an appropriate value
            List<Team> actual;
            actual = target.GetALTs();
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetDirectorates
        ///</summary>
        [TestMethod()]
        public void GetDirectoratesTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            List<string> expected = new List<string>(); // TODO: Initialize to an appropriate value

            expected.Add(String.Empty);

            List<string> actual;
            actual = target.GetDirectorates();

            if(0 != String.Compare(actual.First(), expected.First()))
            {
                Assert.Fail("GetDirectoratesTest failed; First is not empty.");
            }
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetExemptProfileListByProfile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void GetExemptProfileListByProfileTest()
        {
            ReportManager_Accessor target = new ReportManager_Accessor(); // TODO: Initialize to an appropriate value
            long p = 0; // TODO: Initialize to an appropriate value
            List<Profile> expected = new List<Profile>(); // TODO: Initialize to an appropriate value
            List<Profile> actual;
            actual = target.GetExemptProfileListByProfile(p);

            if (actual.Count() != 0)
            {
                Assert.Fail("GetExemptProfileListByProfileTest: The item count should be 0.");
            }

            p = 16;
            actual = target.GetExemptProfileListByProfile(p);

            if (actual.Count() != 1)
            {
                Assert.Fail("GetExemptProfileListByProfileTest: The item count should be 1.");
            }
            
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetExemptProfileListByTeam
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void GetExemptProfileListByTeamTest()
        {
            ReportManager_Accessor target = new ReportManager_Accessor(); // TODO: Initialize to an appropriate value
            long t = 0; // TODO: Initialize to an appropriate value
            List<Profile> expected = new List<Profile>(); // TODO: Initialize to an appropriate value
            List<Profile> actual;
            actual = target.GetExemptProfileListByTeam(t);

            if (actual.Count() != 0)
            {
                Assert.Fail("GetExemptProfileListByTeamTest: The item count should be 0.");
            }

            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetInstance
        ///</summary>
        [TestMethod()]
        public void GetInstanceTest()
        {
            ReportManager expected = null; // TODO: Initialize to an appropriate value
            ReportManager actual;
            actual = ReportManager.GetInstance();

            if (actual == expected)
            {
                Assert.Fail("GetInstanceTest: Actual should not be null. Did not get ReportManager instance.");
            }

            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetOperatingReportEfficiencyBarrierTime
        ///</summary>
        [TestMethod()]
        public void GetOperatingReportEfficiencyBarrierTimeTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetOperatingReportEfficiencyBarrierTimeTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.OPERATING_BARRIER_TABLE;

            //List<OperatingReportTable> expected = null; // TODO: Initialize to an appropriate value
            List<OperatingReportTable> actual;

            actual = target.GetOperatingReport(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetOperatingReportEfficiencyBarrierTimeGraph
        ///</summary>
        [TestMethod()]
        public void GetOperatingReportEfficiencyBarrierTimeGraphTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetOperatingReportEfficiencyBarrierTimeGraphTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.OPERATING_BARRIER_GRAPH;
            
            //List<OperatingReportGraph> expected = null; // TODO: Initialize to an appropriate value
            List<OperatingReportGraph> actual;

            actual = target.GetOperatingReportGraph(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetOperatingReportLoadList
        ///</summary>
        [TestMethod()]
        public void GetOperatingReportLoadListTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetOperatingReportLoadListTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.OPERATING_LOAD_TABLE;

            //List<OperatingReportTable> expected = null; // TODO: Initialize to an appropriate value
            List<OperatingReportTable> actual;

            actual = target.GetOperatingReport(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetOperatingReportLoadListGraph
        ///</summary>
        [TestMethod()]
        public void GetOperatingReportLoadListGraphTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetOperatingReportLoadListGraphTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.OPERATING_LOAD_GRAPH;

            //List<OperatingReportGraph> expected = null; // TODO: Initialize to an appropriate value
            List<OperatingReportGraph> actual;

            actual = target.GetOperatingReportGraph(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetOperatingReportPlanAdherence
        ///</summary>
        [TestMethod()]
        public void GetOperatingReportPlanAdherenceTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetOperatingReportPlanAdherenceTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.OPERATING_ADHERENCE_TABLE;

            //List<OperatingReportTable> expected = null; // TODO: Initialize to an appropriate value
            List<OperatingReportTable> actual;

            actual = target.GetOperatingReport(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetOperatingReportPlanAdherenceGraph
        ///</summary>
        [TestMethod()]
        public void GetOperatingReportPlanAdherenceGraphTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetOperatingReportPlanAdherenceGraphTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.OPERATING_ADHERENCE_GRAPH;

            //List<OperatingReportGraph> expected = null; // TODO: Initialize to an appropriate value
            List<OperatingReportGraph> actual;

            actual = target.GetOperatingReportGraph(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetOperatingReportPlanAttainment
        ///</summary>
        [TestMethod()]
        public void GetOperatingReportPlanAttainmentTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetOperatingReportPlanAttainmentTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.OPERATING_ATTAINMENT_TABLE;

            //List<OperatingReportTable> expected = null; // TODO: Initialize to an appropriate value
            List<OperatingReportTable> actual;
            
            actual = target.GetOperatingReport(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetOperatingReportPlanAttainmentGraph
        ///</summary>
        [TestMethod()]
        public void GetOperatingReportPlanAttainmentGraphTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetOperatingReportPlanAttainmentGraphTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.OPERATING_ATTAINMENT_GRAPH;

            //List<OperatingReportGraph> expected = null; // TODO: Initialize to an appropriate value
            List<OperatingReportGraph> actual;

            actual = target.GetOperatingReportGraph(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetOperatingReportProductivity
        ///</summary>
        [TestMethod()]
        public void GetOperatingReportProductivityTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetOperatingReportProductivityTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.OPERATING_PRODUCTIVITY_TABLE;

            //List<OperatingReportTable> expected = null; // TODO: Initialize to an appropriate value
            List<OperatingReportTable> actual;

            actual = target.GetOperatingReport(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetOperatingReportProductivityGraph
        ///</summary>
        [TestMethod()]
        public void GetOperatingReportProductivityGraphTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetOperatingReportProductivityGraphTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.OPERATING_PRODUCTIVITY_GRAPH;

            //List<OperatingReportGraph> expected = null; // TODO: Initialize to an appropriate value
            List<OperatingReportGraph> actual;

            actual = target.GetOperatingReportGraph(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetOperatingReportUnplanned
        ///</summary>
        [TestMethod()]
        public void GetOperatingReportUnplannedTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetOperatingReportUnplannedTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.OPERATING_UNPLANNED_TABLE;

            //List<OperatingReportTable> expected = null; // TODO: Initialize to an appropriate value
            List<OperatingReportTable> actual;

            actual = target.GetOperatingReport(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetOperatingReportUnplannedGraph
        ///</summary>
        [TestMethod()]
        public void GetOperatingReportUnplannedGraphTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetOperatingReportUnplannedGraphTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.OPERATING_UNPLANNED_GRAPH;

            //List<OperatingReportGraph> expected = null; // TODO: Initialize to an appropriate value
            List<OperatingReportGraph> actual;

            actual = target.GetOperatingReportGraph(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetParetoReportEfficiencyBarrierListCodeList
        ///</summary>
        [TestMethod()]
        public void GetParetoReportEfficiencyBarrierListCodeListTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetParetoReportEfficiencyBarrierListCodeListTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.PARETO_EFFICIENCY_BARRIERS_RAW;

            //List<ParetoReportEfficiencyBarriers_codeList> expected = null; // TODO: Initialize to an appropriate value
            List<ParetoReportBarriers_codeList> actual;

            actual = target.GetParetoReportBarriers(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetParetoReportTaskDelayBarrierList
        ///</summary>
        [TestMethod()]
        public void GetParetoReportTaskDelayBarrierListTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetParetoReportTaskDelayBarrierListTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_RAW;

            //List<ParetoReportTaskDelayBarriers_codeList> expected = null; // TODO: Initialize to an appropriate value
            List<ParetoReportBarriers_codeList> actual;

            actual = target.GetParetoReportBarriers(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetParetoReportTaskDelayBarrierList_OrderByBarrierHours
        ///</summary>
        [TestMethod()]
        public void GetParetoReportTaskDelayBarrierList_OrderByBarrierHoursTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetParetoReportTaskDelayBarrierList_OrderByBarrierHoursTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_RAW;

            //List<ParetoReportTaskDelayBarriers_codeList> expected = null; // TODO: Initialize to an appropriate value
            List<ParetoReportBarriers_codeList> actual;

            actual = target.GetParetoReportBarriers(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetParetoReportTaskDelayBarrierList_OrderByWeekEnding
        ///</summary>
        [TestMethod()]
        public void GetParetoReportTaskDelayBarrierList_OrderByWeekEndingTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetParetoReportTaskDelayBarrierList_OrderByWeekEndingTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.PARETO_DELAY_BARRIERS_RAW;

            //List<ParetoReportTaskDelayBarriers_codeList> expected = null; // TODO: Initialize to an appropriate value
            List<ParetoReportBarriers_codeList> actual;

            actual = target.GetParetoReportBarriers(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetParetoReportUnplannedCodeList
        ///</summary>
        [TestMethod()]
        public void GetParetoReportUnplannedCodeListTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetParetoReportUnplannedCodeListTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.PARETO_UNPLANNED_RAW;

            //List<ParetoReportUnplanned_codeList> expected = null; // TODO: Initialize to an appropriate value
            List<ParetoReportUnplanned_codeList> actual;

            actual = target.GetParetoReportUnplannedCodeList(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetParticipationReportParticipation
        ///</summary>
        [TestMethod()]
        public void GetParticipationReportParticipationTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            spec.Description = "Test GetParticipationReportParticipationTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.LOG_PARTICIPATION;

            //List<ParticipationReportParticipation> expected = null; // TODO: Initialize to an appropriate value
            List<ParticipationReport> actual;

            actual = target.GetParticipationReport(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetPrivateReportGroups
        ///</summary>
        [TestMethod()]
        public void GetPrivateReportGroupsTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            //List<ReportGroup> expected = null; // TODO: Initialize to an appropriate value
            List<ReportGroup> actual;
            actual = target.GetPrivateReportGroups();
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetPublicReportGroups
        ///</summary>
        [TestMethod()]
        public void GetPublicReportGroupsTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            //List<ReportGroup> expected = null; // TODO: Initialize to an appropriate value
            List<ReportGroup> actual;
            actual = target.GetPublicReportGroups();
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetReportGroups
        ///</summary>
        [TestMethod()]
        public void GetReportGroupsTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            //List<ReportGroup> expected = null; // TODO: Initialize to an appropriate value
            List<ReportGroup> actual;
            actual = target.GetReportGroups();
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetReportList
        ///</summary>
        [TestMethod()]
        public void GetReportListTest()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            //List<Report> expected = null; // TODO: Initialize to an appropriate value
            List<Report> actual;
            actual = target.GetReportList();
            
            //if(!actual.Count() < 0)
            //{
            //}
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetReportTeams
        ///</summary>
        [TestMethod()]
        public void GetReportTeamsTest1()
        {
            ReportManager target = BLL.ReportManager.GetInstance();
            Report spec = new Report();
            ///List<Team> expected = null; // TODO: Initialize to an appropriate value
            List<Team> actual;

            spec.Description = String.Empty;
            spec.FromDate = DateTime.Today;

            spec.Group = BLL.ReportManager.GetInstance().GetReportGroup(1);

            spec.IsMonthly = false;
            spec.Owner = new Profile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Faux report for testing.";
            spec.ToDate = DateTime.Today;
            spec.Type = DTO.Report.TypeEnum.LOG_PARTICIPATION;

            // Handle null report group?
            actual = target.GetReportTeams(spec.Group);

            spec.Group = new ReportGroup();
            spec.Group.Description = String.Empty;
            spec.Group.Directorates = new List<Directorate>();
            
            Directorate d = new Directorate();
            d.Name = "Faux Directorate";
            spec.Group.Directorates.Add(d);

            Team t = new Team();
            t.Name = "Faux Team";
            spec.Group.Teams.Add(t);

            Profile p = new Profile();
            p.DisplayName = "Faux Profile";
            spec.Group.Profiles.Add(p);

            actual = target.GetReportTeams(spec.Group);

            if (actual.Count != 3)
            {
                //Assert.Fail("GetReportTeamsTest1: Function did not return three teams.");
            }

            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetSummaryReportBarrier
        ///</summary>
        [TestMethod()]
        public void GetSummaryReportBarrierTest()
        {
            ReportManager target = new ReportManager();
            Report spec = new Report();
            spec.Description = "Test GetParticipationReportParticipationTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = spec.Group = BLL.ReportManager.GetInstance().GetReportGroup(1);

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.SUMMARY_EFFICIENCY_BARRIER;

            //List<SummaryReportTable> expected = null; // TODO: Initialize to an appropriate value
            List<SummaryReportTable> actual;
            actual = target.GetSummaryReportBarrier(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetSummaryReportUnplanned
        ///</summary>
        [TestMethod()]
        public void GetSummaryReportUnplannedTest()
        {
            ReportManager target = new ReportManager(); // TODO: Initialize to an appropriate value
            Report spec = new Report();
            spec.Description = "Test GetParticipationReportParticipationTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = spec.Group = BLL.ReportManager.GetInstance().GetReportGroup(1);

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.SUMMARY_UNPLANNED;

            //List<SummaryReportTable> expected = null; // TODO: Initialize to an appropriate value
            List<SummaryReportTable> actual;
            actual = target.GetSummaryReportUnplanned(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetSunday
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void GetSundayTest()
        {
            ReportManager_Accessor target = new ReportManager_Accessor(); // TODO: Initialize to an appropriate value
            Nullable<DateTime> dateTime = new DateTime(2012, 06, 15);
            DateTime expected = new DateTime(2012, 06, 17); // TODO: Initialize to an appropriate value
            DateTime actual;
            actual = target.GetWeekEnding(dateTime);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetTeamOwner
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void GetTeamOwnerTest()
        {
            ReportManager_Accessor target = new ReportManager_Accessor();
            long teamId = 1;
            long expected = -1;
            long actual;
            actual = target.GetTeamOwner(teamId);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetUserGroupTypes
        ///</summary>
        [TestMethod()]
        public void GetUserGroupTypesTest()
        {
            ReportManager target = new ReportManager(); // TODO: Initialize to an appropriate value
            //List<string> expected = null; // TODO: Initialize to an appropriate value
            List<string> actual;
            actual = target.GetUserGroupTypes();
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetWaltTeamInformation
        ///</summary>
        [TestMethod()]
        public void GetWaltTeamInformationTest()
        {
            ReportManager target = new ReportManager(); // TODO: Initialize to an appropriate value
            Report spec = new Report();
            spec.Description = "Test GetParticipationReportParticipationTest.";
            spec.FromDate = new DateTime(2000, 01, 02);

            spec.Group = new DTO.ReportGroup();
            spec.Group.Description = "Test ParetoReportTable group";
            spec.Group.Directorates = new List<DTO.Directorate>();
            spec.Group.Name = "Test ParetoReportTable Group Name"; ;
            spec.Group.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.Group.Profiles = new List<DTO.Profile>();
            spec.Group.Public = false;
            spec.Group.Teams = new List<DTO.Team>();
            spec.Group.Teams.Add(BLL.AdminManager.GetInstance().GetTeam(BLL.AdminManager.GetInstance().GetProfile()));

            spec.IsMonthly = false;
            spec.Owner = BLL.AdminManager.GetInstance().GetProfile();
            spec.PercentBase = 100;
            spec.PercentGoal = 100;
            spec.Public = false;
            spec.Title = "Test ParetoReportTable";
            spec.ToDate = spec.FromDate.Value.AddDays(7);
            spec.Type = DTO.Report.TypeEnum.TEAM_INFO;

            //List<waltTeamInformation> expected = null; // TODO: Initialize to an appropriate value
            List<waltTeamInformation> actual;
            actual = target.GetWaltTeamInformation(spec);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetWeeklyPlanListByProfile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void GetWeeklyPlanListByProfileTest()
        {
            ReportManager_Accessor target = new ReportManager_Accessor(); // TODO: Initialize to an appropriate value
            long p = 7;
            DateTime start = DateTime.Today.AddDays(-7);
            DateTime end = DateTime.Today;
            //List<WeeklyPlan> expected = null; // TODO: Initialize to an appropriate value
            List<WeeklyPlan> actual;
            actual = target.GetWeeklyPlanListByProfile(p, start, end);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetWeeklyPlanListByTeam
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void GetWeeklyPlanListByTeamTest()
        {
            ReportManager_Accessor target = new ReportManager_Accessor(); // TODO: Initialize to an appropriate value
            long teamId = 27; // TODO: Initialize to an appropriate value
            DateTime start = DateTime.Today.AddDays(-7);
            DateTime end = DateTime.Today;
            //List<WeeklyPlan> expected = null; // TODO: Initialize to an appropriate value
            List<WeeklyPlan> actual;
            actual = target.GetWeeklyPlanListByTeam(teamId, start, end);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsActivityLogManager
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void IsActivityLogManagerTest()
        {
            ReportManager_Accessor target = new ReportManager_Accessor();
            long team_id = 27;
            //bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsActivityLogManager(team_id);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsAllowedDeletePublicReport
        ///</summary>
        [TestMethod()]
        public void IsAllowedDeletePublicReportTest()
        {
            ReportManager target = new ReportManager();
            //bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsAllowedDeletePublicReport();
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsAllowedDeletePublicReportGroupFilter
        ///</summary>
        [TestMethod()]
        public void IsAllowedDeletePublicReportGroupFilterTest()
        {
            ReportManager target = new ReportManager(); // TODO: Initialize to an appropriate value
            //bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsAllowedDeletePublicReportGroupFilter();
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsAllowedSavePublicGroupFilter
        ///</summary>
        [TestMethod()]
        public void IsAllowedSavePublicGroupFilterTest()
        {
            ReportManager target = new ReportManager(); // TODO: Initialize to an appropriate value
            //bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsAllowedSavePublicGroupFilter();
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsAllowedSavePublicReport
        ///</summary>
        [TestMethod()]
        public void IsAllowedSavePublicReportTest()
        {
            ReportManager target = new ReportManager(); // TODO: Initialize to an appropriate value
            //bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsAllowedSavePublicReport();
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsAllowedViewIcs
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void IsAllowedViewIcsTest()
        {
            ReportManager_Accessor target = new ReportManager_Accessor();
            Team team = BLL.AdminManager.GetInstance().GetTeam(27, true);

            //bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsAllowedViewIcs(team);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsDirectorateManager
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void IsDirectorateManagerTest()
        {
            ReportManager_Accessor target = new ReportManager_Accessor();
            long directorateTeamId = 10;
            //bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsDirectorateManager(directorateTeamId);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsReportGroupPublic
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void IsReportGroupPublicTest()
        {
            ReportManager_Accessor target = new ReportManager_Accessor();
            long id = 1;
            //bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsReportGroupPublic(id);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsReportPublic
        ///</summary>
        [TestMethod()]
        [DeploymentItem("WALT.BLL.dll")]
        public void IsReportPublicTest()
        {
            ReportManager_Accessor target = new ReportManager_Accessor();
            long id = 1;
            //bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsReportPublic(id);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for SaveReport
        ///</summary>
        [TestMethod()]
        public void SaveReportTest()
        {
            ReportManager target = new ReportManager();
            Report report = null; // TODO: Initialize to an appropriate value
            target.SaveReport(report);
            //Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SaveReportGroup
        ///</summary>
        [TestMethod()]
        public void SaveReportGroupTest()
        {
            ReportManager target = new ReportManager();
            ReportGroup g = null; // TODO: Initialize to an appropriate value
            target.SaveReportGroup(g);
            //Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for XMLtoFilter
        ///</summary>
        [TestMethod()]
        public void XMLtoFilterTest()
        {
            ReportManager target = new ReportManager();
            string xml = string.Empty; // TODO: Initialize to an appropriate value
            //ReportFilter expected = new ReportFilter(); // TODO: Initialize to an appropriate value
            ReportFilter actual;
            actual = target.XMLtoFilter(xml);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for XMLtoSQL
        ///</summary>
        [TestMethod()]
        public void XMLtoSQLTest()
        {
            ReportManager target = new ReportManager();
            string xml = string.Empty; // TODO: Initialize to an appropriate value
            //string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.XMLtoSQL(xml);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
