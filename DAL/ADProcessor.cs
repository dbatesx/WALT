using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WALT.DTO;
using System.Data.SqlClient;
using System.Diagnostics;
using System.DirectoryServices;
using System.Configuration;
using System.Data.Entity;
using System.Data.Linq;

namespace WALT.DAL
{
    class ADProcessor : Processor
    {
        public ADProcessor(Mediator mediator)
            : base(mediator)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public ADEntry CreateADEntryDTO(string domain, DirectoryEntry entry)
        {
            ADEntry ad = new ADEntry();

            try
            {
                ad.Domain = domain.ToUpper();
                ad.Username = entry.Properties["samaccountname"].Value.ToString().ToLower();
                ad.DisplayName = entry.Properties["displayname"].Value as string;
                ad.EmployeeID = entry.Properties["employeeid"].Value as string;
                ad.OrgCode = entry.Properties["department"].Value as string;
                string manager = entry.Properties["manager"].Value as string;

                if (!string.IsNullOrEmpty(manager))
                {
                    string[] parts2 = entry.Properties["manager"].Value.ToString().Split(',');
                    string[] parts3 = parts2[0].Split('=');
                    ad.Manager = parts3[1];
                }
            }
            catch
            {
                return null;
            }

            return ad;
        }

    }
}
