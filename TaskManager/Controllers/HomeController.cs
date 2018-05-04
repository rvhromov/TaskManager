using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        // Property to interact with the user store
        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        ApplicationContext context = new ApplicationContext();

        [AllowAnonymous]
        public ActionResult Index()
        {
            // If user is authenticated, show the list of his tasks
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Tasks");

            return View();
        }

        public ActionResult Tasks()
        {
            IEnumerable<TaskModel> tasks = new List<TaskModel>();
            string userid = User.Identity.GetUserId();

            // Get all user's tasks and sort them by date
            if (!string.IsNullOrEmpty(userid))
            {
                tasks = context.Tasks.Where(t => t.ApplicationUserId == userid);
                return View(tasks.OrderBy(t => t.Date));
            }

            return View("Login", "Account");
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TaskModel model)
        {
            if (ModelState.IsValid)
            {
                // Create new task and update list of already existing tasks
                ApplicationUser user = await UserManager.FindByEmailAsync(User.Identity.Name);
                user.Tasks.Add(model);
                IdentityResult result = await UserManager.UpdateAsync(user);

                if (result.Succeeded)
                    return RedirectToAction("Tasks", "Home");
                else
                    ModelState.AddModelError("CreateTask", "Unable to create task");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> EditTask(int? id)
        {
            if (id == null)
                return HttpNotFound();

            // Find particular task by id and return it in View
            TaskModel task = await context.Tasks.FindAsync(id);
            if (task == null)
                return HttpNotFound();

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditTask(TaskModel task)
        {
            // Save edited task
            context.Entry(task).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return RedirectToAction("Tasks");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteTask(int id)
        {
            // Find particular task by id and remove it
            TaskModel task = await context.Tasks.FindAsync(id);
            if (task == null)
                return HttpNotFound();

            context.Tasks.Remove(task);
            await context.SaveChangesAsync();

            return RedirectToAction("Tasks");
        }
    }
}