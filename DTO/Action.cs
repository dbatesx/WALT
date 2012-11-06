using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.DTO
{

     [Serializable()]
    public enum Action
    {
        ROLE_MANAGE,
        PROFILE_MANAGE,
        TASK_MANAGE,
        TEAM_MANAGE,
        REPORT_MANAGE,
        METADATA_MANAGE,
        DIRECTORATE_ADMIN,
        DIRECTORATE_MANAGE,
        SYSTEM_MANAGE
    }
}
