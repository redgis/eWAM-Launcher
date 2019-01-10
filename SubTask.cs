using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eWamLauncher
{
   /// <summary>
   /// Describes a "task", in the context of downloading packages. See PackageDownloadInfo class.
   /// </summary>
   public class SubTask
   {
      /// <summary>
      /// Task type (e.g. extract or download)
      /// </summary>
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

      /// <summary>
      /// task description
      /// </summary>
      public string description;

      /// <summary>
      /// other task required to be completed before this one can be processed. e.g. an "extract" task 
      /// first requires the completion of the "download" task of the file to extract.
      /// </summary>
      public SubTask prerequisite;

      /// <summary>
      /// Task type (e.g. extract or download)
      /// </summary>
      public eTaskTypes taskType;

      /// <summary>
      /// Source path for the file to be processed
      /// </summary>
      public string source;

      /// <summary>
      /// Destination path for the file to be processed
      /// </summary>
      public string target;

      /// <summary>
      /// Completion status of the task
      /// </summary>
      public bool completed;

      /// <summary>
      /// Work amount for the task (used to evaluate the percentage of a package download)
      /// </summary>
      public int workAmount;
   }
}
