using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DocStorage.Models;
using NHibernate;

namespace DocStorage.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login login, string ReturnUrl = "")
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var user = session.QueryOver<User>()
                    .Where(q => q.UserName == login.Username)
                    .And(q => q.Password == login.Password)
                    .SingleOrDefault<User>();

                if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(user.UserName, login.RememberMe);
                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Document");
                    }
                }
                else
                {                   
                    ModelState.AddModelError("", "Неправильный логин или пароль!");     
                }

            }
            ModelState.Remove("Password");

            return View();
        }

        [AllowAnonymous]
        public ActionResult CreateUsers()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost, ActionName("CreateUsers")]
        public ActionResult PostCreateUsers()
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSession())
                {
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        DataModelMethodsExecutor.DropDbAndCreateUsers(session);
                        transaction.Commit();
                    }
                }

                System.IO.DirectoryInfo di = new DirectoryInfo(HttpContext.Server.MapPath("../Storage"));

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch (Exception ex)
            {
                return View(ex.StackTrace);
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}
