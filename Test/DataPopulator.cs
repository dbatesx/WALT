using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WALT.Test
{
    class DataPopulator
    {
        public void GenerateTasks(DTO.Profile p, long num_tasks)
        {
            DTO.Team team = BLL.AdminManager.GetInstance().GetTeam("SW Apps");
            List<DTO.TaskType> types = BLL.AdminManager.GetInstance().GetTaskTypeList(team, false, false, null);
            List<DTO.Program> programs = BLL.AdminManager.GetInstance().GetProgramList();

            for (long i = 0; i < num_tasks; i++)
            {
                DTO.Task t = new DTO.Task();
                t.Owner = p;
                t.Title = "Task_" + i;
                t.Hours = 4;
                t.StartDate = DateTime.Now;
                t.TaskType = types[0];
                t.Status = WALT.DTO.Task.StatusEnum.OPEN;
                t.DueDate = DateTime.Now;
                t.CompletedDate = DateTime.Now;
                t.Program = programs[1];
                BLL.TaskManager.GetInstance().SaveTask(t);
            }
        }
    }
}
