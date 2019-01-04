using ProjectManager.DataAccessLayer;
using ProjectManager.InterfaceLayer;
using System.Collections.ObjectModel;
using System.Linq;

namespace ProjectManager.BusinessLayer
{
    public class TaskBL : ITaskBL
    {
        private readonly ProjectManagerEntities _projectManager;

        public TaskBL()
        {
            _projectManager = new ProjectManagerEntities();
        }

        public TaskBL(ProjectManagerEntities projectManager)
        {
            _projectManager = projectManager;
        }

        public Collection<CommonEntities.ParentTasks> GetParentTasks()
        {

            Collection<CommonEntities.ParentTasks> taskCollection = new Collection<CommonEntities.ParentTasks>();
            _projectManager.ParentTasks1
                .Select(u => new CommonEntities.ParentTasks()
                {
                    ParentTaskID = u.ParentTaskID,
                    ParentTask = u.ParentTask
                }).ToList()
               .ForEach(y => taskCollection.Add(y));

            return taskCollection;
        }

        public Collection<CommonEntities.Tasks> GetTasks(int projectID)
        {

            Collection<CommonEntities.Tasks> taskCollection = new Collection<CommonEntities.Tasks>();

            _projectManager.Tasks1.Where(c => c.ParentTaskID == null && c.ProjectID == projectID)
                .Select(s => new CommonEntities.Tasks
                {
                    Task = s.Task,
                    ProjectID = s.ProjectID,
                    Project = _projectManager.Projects1.Where(u => u.ProjectID == s.ProjectID)
                    .Select(u => u.Project).FirstOrDefault(),
                    ParentTask = "",
                    Priority = s.Priority,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    TaskID = s.TaskID,
                    Status = s.Status,
                    UserID = _projectManager.Users1.Where(u => u.TaskID == s.TaskID)
                    .Select(u => u.UserID).FirstOrDefault(),
                    UserName = _projectManager.Users1.Where(u => u.TaskID == s.TaskID)
                    .Select(u => u.FirstName + " " + u.LastName).FirstOrDefault()
                }).ToList()
                    .ForEach(x => taskCollection.Add(x));

            _projectManager.Tasks1.Where(c => c.ParentTaskID != null && c.ProjectID == projectID)
                .Join(_projectManager.ParentTasks1, f => f.ParentTaskID, s => s.ParentTaskID,
                (f, s) => new CommonEntities.Tasks
                {
                    Task = f.Task,
                    ProjectID = f.ProjectID,
                    Project = _projectManager.Projects1.Where(u => u.ProjectID == f.ProjectID)
                    .Select(u => u.Project).FirstOrDefault(),
                    ParentTask = s.ParentTask,
                    Priority = f.Priority,
                    StartDate = f.StartDate,
                    EndDate = f.EndDate,
                    ParentTaskID = s.ParentTaskID,
                    TaskID = f.TaskID,
                    Status = f.Status,
                    UserID = _projectManager.Users1.Where(u => u.TaskID == f.TaskID)
                    .Select(u => u.UserID).FirstOrDefault(),
                    UserName = _projectManager.Users1.Where(u => u.TaskID == f.TaskID)
                    .Select(u => u.FirstName + " " + u.LastName).FirstOrDefault()
                }).ToList()
                 .ForEach(x => taskCollection.Add(x));

            return taskCollection;
        }


        public void AddTask(CommonEntities.Tasks task)
        {
            Tasks tk = new Tasks
            {
                Task = task.Task,
                ProjectID = task.ProjectID,
                Priority = task.Priority,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                Status = false
            };
            if (task.ParentTaskID == 0)
            {
                tk.ParentTaskID = null;
            }
            else
            {
                tk.ParentTaskID = task.ParentTaskID;
            }


            _projectManager.Tasks1.Add(tk);
            _projectManager.SaveChanges();
            var taskId = tk.TaskID;
            var ur = _projectManager.Users1.Where(x => x.UserID == task.UserID).FirstOrDefault();
            if (ur != null)
            {
                ur.TaskID = taskId;
                _projectManager.SaveChanges();
            }
        }

        public void UpdateTask(CommonEntities.Tasks task)
        {
            var tk = _projectManager.Tasks1.Where(x => x.TaskID == task.TaskID).FirstOrDefault();

            if (tk != null)
            {
                tk.Task = task.Task;
                tk.ProjectID = task.ProjectID;
                tk.Priority = task.Priority;
                tk.StartDate = task.StartDate;
                tk.EndDate = task.EndDate;
                if (task.ParentTaskID == 0)
                {
                    tk.ParentTaskID = null;
                }
                else
                {
                    tk.ParentTaskID = task.ParentTaskID;
                }

                _projectManager.SaveChanges();
                var ur = _projectManager.Users1.Where(x => x.UserID == task.UserID).FirstOrDefault();
                if (ur != null)
                {
                    ur.TaskID = tk.TaskID;
                    _projectManager.SaveChanges();
                }
            }
        }

        public void EndTask(CommonEntities.Tasks task)
        {
            var tk = _projectManager.Tasks1.Where(x => x.TaskID == task.TaskID).FirstOrDefault();

            if (tk != null)
            {
                tk.Status = true;
                _projectManager.SaveChanges();
            }
        }

        public void AddParentTask(CommonEntities.ParentTasks pTask)
        {
            ParentTasks tk = new ParentTasks
            {
                ParentTask = pTask.ParentTask
            };

            _projectManager.ParentTasks1.Add(tk);
            _projectManager.SaveChanges();
        }
    }
}
