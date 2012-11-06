using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WALT.BLL;
using WALT.DTO;

namespace WALT.Test
{
    /// <summary>
    /// Summary description for ProfileManagerTest
    /// </summary>
    [TestClass]
    public class ProfileManagerTest
    {
        public ProfileManagerTest()
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

        [TestMethod()]
        public void GetADEntryTest()
        {
            Profile p = ProfileManager.GetInstance().GetProfile();
            ADEntry e = ProfileManager.GetInstance().GetADEntry(p);
            Assert.AreEqual(p.Username, e.Username);
        }

        [TestMethod()]
        public void GetADEntryListTest()
        {
            List<DTO.ADEntry> entries = ProfileManager.GetInstance().GetADEntryList("displayname", "*Jones*");
        }

        [TestMethod()]
        public void SyncADTest()
        {
            BLL.ProfileManager.GetInstance().SyncAD();
        }

        [TestMethod()]
        public void SaveProfileTest()
        {
            Profile p = ProfileManager.GetInstance().GetProfile();
            BLL.ProfileManager.GetInstance().SaveProfile(p);
        }

        [TestMethod()]
        public void SavePreferenceTest()
        {
            Profile p = ProfileManager.GetInstance().GetProfile();
            BLL.ProfileManager.GetInstance().SavePreference("Test2", "abc");
        }
    }
}
