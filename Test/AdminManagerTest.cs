using WALT.BLL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WALT.DTO;
using System.Collections.Generic;
using System.Data;
using System;

namespace WALT.Test
{
    
    
    /// <summary>
    ///This is a test class for AdminManagerTest and is intended
    ///to contain all AdminManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AdminManagerTest
    {
        public AdminManagerTest()
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
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod()]
        public void GetProgramListTest()
        {
            List<Program> programs = AdminManager.GetInstance().GetProgramList();

            if (programs.Count == 0)
            {
                throw new AssertFailedException("Program list is empty");
            }
        }

        [TestMethod()]
        public void GetProfileListTest()
        {
            List<DTO.Profile> profiles = ProfileManager.GetInstance().GetProfileList();

            if (profiles.Count == 0)
            {
                throw new AssertFailedException("Profile list is empty");
            }
        }

        [TestMethod()]
        public void SaveAlertTest()
        {
            Alert alert = new Alert();
            alert.Message = "Test alert message; delete * from profiles;";
            alert.Profile = AdminManager.GetInstance().GetProfile();
            alert.Acknowledged = false;
            ProfileManager.GetInstance().SaveAlert(alert);
        }

        [TestMethod()]
        public void AcknowledgeAlertTest()
        {
            List<Alert> alerts = ProfileManager.GetInstance().GetAlertList(false);

            foreach (Alert a in alerts)
            {
                a.Acknowledged = true;
                ProfileManager.GetInstance().SaveAlert(a);
            }
        }

        [TestMethod()]
        public void SaveRoleTest()
        {
            Role r = new Role();
            r.Title = "Role1";
            r.Description = "Description";
            r.Actions = new List<DTO.Action>();
            r.Actions.Add(DTO.Action.PROFILE_MANAGE);
            r.Actions.Add(DTO.Action.REPORT_MANAGE);
            AdminManager.GetInstance().SaveRole(r);
        }

        [TestMethod()]
        public void SaveProgramTest()
        {
            Program p = new Program();
            p.Title = "Program1";
            p.Active = true;
            AdminManager.GetInstance().SaveProgram(p);

            p.Title = "Program1 Updated";
            AdminManager.GetInstance().SaveProgram(p);
        }

        [TestMethod()]
        public void GetDirectorateListTest()
        {
            List<Directorate> items = AdminManager.GetInstance().GetDirectorateList();

            foreach (Directorate d in items)
            {
                if (d.Teams.Count == 0)
                {
                }
            }
        }

        [TestMethod()]
        public void SaveTeamTest()
        {
            Team t = new Team();
            t.Name = "Team1 " + System.DateTime.Now;
            t.ComplexityBased = false;
            t.Active = true;
            t.Owner = ProfileManager.GetInstance().GetProfile();
            t.Members = new List<Profile>();
            t.Admins = new List<Profile>();

            List<DTO.Profile> profiles = ProfileManager.GetInstance().GetProfileList();

            foreach (DTO.Profile p in profiles)
            {
                t.Members.Add(p);
            }

            AdminManager.GetInstance().SaveTeam(t);

            t.ComplexityBased = true;

            AdminManager.GetInstance().SaveTeam(t);
        }

        [TestMethod()]
        public void GetBarrierListTest()
        {
            System.Console.WriteLine(DateTime.Now);
            List<Barrier> items = BLL.AdminManager.GetInstance().GetBarrierList();
            System.Console.WriteLine(DateTime.Now);
        }

        [TestMethod()]
        public void AddMemberToTeamTest()
        {
            DTO.Team team = BLL.AdminManager.GetInstance().GetTeam("SW Apps");
            DTO.Profile p = BLL.AdminManager.GetInstance().GetProfile();
            BLL.AdminManager.GetInstance().RemoveTeamMember(team, p);
            BLL.AdminManager.GetInstance().AddTeamMember(team, p);
        }

        [TestMethod()]
        public void RemoveTeamMemberTest()
        {
            DTO.Team team = BLL.AdminManager.GetInstance().GetTeam("SW Apps");
            DTO.Profile p = BLL.AdminManager.GetInstance().GetProfile();
            BLL.AdminManager.GetInstance().RemoveTeamMember(team, p);
        }

        [TestMethod()]
        public void AddAdminToTeamTest()
        {
            DTO.Team team = BLL.AdminManager.GetInstance().GetTeam("SW Apps");
            DTO.Profile p = BLL.AdminManager.GetInstance().GetProfile();
            BLL.AdminManager.GetInstance().RemoveTeamBackupALM(team, p);
            BLL.AdminManager.GetInstance().AddTeamBackupALM(team, p);
        }

        [TestMethod()]
        public void RemoveTeamAdminTest()
        {
            DTO.Team team = BLL.AdminManager.GetInstance().GetTeam("SW Apps");
            DTO.Profile p = BLL.AdminManager.GetInstance().GetProfile();
            BLL.AdminManager.GetInstance().RemoveTeamBackupALM(team, p);
        }

        [TestMethod()]
        public void SaveBarrierTest()
        {
            DTO.Team team = BLL.AdminManager.GetInstance().GetTeam("Technical Operations East");
            List<DTO.Barrier> items = BLL.AdminManager.GetInstance().GetBarrierList(team, false, null);
            DTO.Barrier b = new DTO.Barrier();
            b.Code = "TB1";
            b.Description = "This is a test barrier";
            b.Title = "TestBarrier";
            b.ParentId = items[0].Id;
            BLL.AdminManager.GetInstance().SaveBarrier(team, b, false);
        }

        [TestMethod()]
        public void ChangeTeamOwnerTest()
        {
            DTO.Team team = BLL.AdminManager.GetInstance().GetTeam("SW Apps");
            DTO.Profile p = BLL.ProfileManager.GetInstance().GetProfile("DULLES\\blessingc");
            team.Owner = p;
            AdminManager.GetInstance().SaveTeam(team);
        }
    }
}
