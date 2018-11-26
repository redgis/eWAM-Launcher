using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eWamLauncher
{
   public class SubTask
   {
      public enum eTaskTypes
      {
         Download,
         Extract
      }

      public SubTask(eTaskTypes taskType, string source, string target, string description, int workAmount, SubTask prerequisite = null)
      {
         this.description = description;
         this.prerequisite = prerequisite;
         this.taskType = taskType;
         this.source = source;
         this.target = target;
         this.completed = false;
         this.workAmount = workAmount;
      }

      public string description;
      public SubTask prerequisite;
      public eTaskTypes taskType;
      public string source;
      public string target;
      public bool completed;
      public int workAmount;
   }
}
