using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        // Property to interact with the user store
        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        // Property to manage website login
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await UserManager.FindAsync(model.Email, model.Password);
                if (user != null)
                {
                    if (user.EmailConfirmed)
                    {
                        // Create claim
                        ClaimsIdentity claim = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                        // Delete auth cookies
                        AuthenticationManager.SignOut();
                        // Create auth cookies
                        AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = true }, claim);
                        // Redirect to the main page
                        return RedirectToAction("Tasks", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("CustomErr", "Please, confirm your email");
                    }
                }
                else
                {
                    ModelState.AddModelError("CustomErr", "Wrong email or password");
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegistrationModel model)
        {
            if (ModelState.IsValid)
            {
                // Create new user
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Generate token for email confirmation
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // Confirmation link
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // Send message to user's email account
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Hello " + user.FirstName + "! Thank you for registration. " +
                        "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return View("DisplayEmail");
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return View("Error");
            
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        public async Task<ActionResult> EditName()
        {
            // Find current user and return his name in View
            ApplicationUser user = await UserManager.FindByEmailAsync(User.Identity.Name);
            if (user != null)
            {
                EditModel model = new EditModel { FirstName = user.FirstName };
                return View(model);
            }

            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditName(EditModel model)
        {
            // Find current user and update his new name
            ApplicationUser user = await UserManager.FindByEmailAsync(User.Identity.Name);
            if (user != null)
            {
                user.FirstName = model.FirstName;
                IdentityResult result = await UserManager.UpdateAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("Tasks", "Home");
                else
                    ModelState.AddModelError("EditName", "Something went wrong");
            }
            else
            {
                ModelState.AddModelError("EditName", "User wasn't found");
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Delete()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed()
        {
            // Delete account and logout
            ApplicationUser user = await UserManager.FindByEmailAsync(User.Identity.Name);
            if (user != null)
            {
                IdentityResult result = await UserManager.DeleteAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("Logout", "Account");
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            // Delete auth cookies
            AuthenticationManager.SignOut();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                // Find current user and reset password
                ApplicationUser user = await UserManager.FindByEmailAsync(User.Identity.Name);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                    return View("ChangePassword");

                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                IdentityResult result = await UserManager.ResetPasswordAsync(user.Id, code, model.Password);
                if (result.Succeeded)
                    return RedirectToAction("ChangePasswordConfirmation", "Account");
            }

            return View(model);
        }

        public ActionResult ChangePasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                // Find current user
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                    return View("ForgotPasswordConfirmation");

                // Generate token for password reset
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // Reset link
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                // Send message to user's email account
                await UserManager.SendEmailAsync(user.Id, "Reset Password",
                    "Hello! You can reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Find current user
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
                return RedirectToAction("ResetPasswordConfirmation", "Account");

            // Reset password
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation", "Account");

            ModelState.AddModelError("ResetPass", "Oops, something went wrong");
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}