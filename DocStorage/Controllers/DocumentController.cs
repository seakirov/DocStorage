using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DocStorage.Models;
using NHibernate;
using NHibernate.Criterion;

namespace DocStorage.Controllers
{
    public class DocumentController : Controller
    {

        [Authorize]
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int page = 1)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                
                ViewBag.CurrentSort = sortOrder;

                ViewBag.DocNameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewBag.CreatedSortParm = sortOrder == "Date" ? "date_desc" : "Date";
                ViewBag.UserSortParm = sortOrder == "User" ? "user_desc" : "User";

                if (searchString != null)
                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewBag.CurrentFilter = searchString;


                int pageSize = 3;

                var users = session.QueryOver<User>().List<User>();

                ICriterion critSearch = null;

                if (!String.IsNullOrEmpty(searchString))
                {
                    critSearch = Expression.Like("DocName", "%" + searchString + "%");
                }


                Order critOrder;
                switch (sortOrder)
                {
                    case "name_desc":
                        critOrder = Order.Desc("DocName");
                        break;
                    case "Date":
                        critOrder = Order.Asc("Created");
                        break;
                    case "date_desc":
                        critOrder = Order.Desc("Created");
                        break;
                    case "User":
                        critOrder = Order.Asc("User");
                        break;
                    case "user_desc":
                        critOrder = Order.Desc("User");
                        break;
                    default:
                        critOrder = Order.Asc("DocName");
                        break;
                }

                IList<Document> documents;

                if (critSearch != null)
                    documents = session.CreateCriteria<Document>().Add(critSearch).AddOrder(critOrder).List<Document>();
                else
                    documents = session.CreateCriteria<Document>().AddOrder(critOrder).List<Document>();

                IEnumerable<Document> documentsPerPages = documents.Skip((page - 1) * pageSize).Take(pageSize);

                PageInfo pageInfo = new PageInfo(){
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalItems = documents.Count
                };
                DocumentsWithPages dvp = new DocumentsWithPages()
                {
                    PageInfo = pageInfo,
                    Documents = documentsPerPages
                };

                return View(dvp);
            }

        }

        [Authorize]
        public ActionResult View(int id)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var document = session.Get<Document>(id);
                WordDocHelper.Open(document.Path);
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Create(Document document)
        {
            try
            {
                String UserName = HttpContext.User.Identity.Name;
               
                using (ISession session = NHibernateHelper.OpenSession())
                {
                    User creator = session.QueryOver<User>()
                            .Where(q => q.UserName == UserName)
                            .SingleOrDefault<User>();

                    if (String.IsNullOrEmpty(document.DocName))
                    {
                        ModelState.AddModelError("DocName", "Название документа должно быть заполнено!");
                    }

                    if (ModelState.IsValid)
                    {
                        var doc = WordDocHelper.Create(UserName, document.DocName);
                     
                        using (ITransaction transaction = session.BeginTransaction())
                        {
                            DataModelMethodsExecutor.AddDocument(session, doc.DocName, doc.Created, doc.Path, creator.UserId);

                            session.Flush();
                            transaction.Commit();
                        }

                        return RedirectToAction("Index");
                    }                 
                }

                return View(document);
            }
            catch (Exception ex)
            {
                return View(ex.StackTrace);
            }
        }

        [Authorize]
        public ActionResult Upload()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase upload)
        {
            if (upload != null)
            {
                String UserName = HttpContext.User.Identity.Name;

                string docName = Path.GetFileName(upload.FileName);
                if (Path.GetFileNameWithoutExtension(upload.FileName).Length > 30)
                {
                    docName = Path.GetFileNameWithoutExtension(upload.FileName).Substring(0, 30) + "..." + Path.GetExtension(upload.FileName);
                }

                string filePath = Server.MapPath(string.Format("../Storage/{0}", DateTime.Now.ToString("yyyyMMddhhmmss")));
                
                WordDocHelper.CheckDirectory(filePath);

                using (ISession session = NHibernateHelper.OpenSession())
                {
                    User creator = session.QueryOver<User>()
                        .Where(q => q.UserName == UserName)
                        .SingleOrDefault<User>();

                    var fullPath = filePath + "\\" + docName;

                    if (ModelState.IsValid)
                    {

                        using (ITransaction transaction = session.BeginTransaction())
                        {
                            DataModelMethodsExecutor.AddDocument(session, docName, DateTime.Now, fullPath, creator.UserId);

                            session.Flush();
                            transaction.Commit();
                        }

                        upload.SaveAs(fullPath);

                        return RedirectToAction("Index");
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Выберите файл для загрузки!");
            }
            return View();
        }

        [Authorize]
        public ActionResult Delete(int id)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var document = session.Get<Document>(id);
                var users = session.QueryOver<User>().List<User>();
                return View(document);
            }
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult PostDelete(int id)
        {
            try
            {

                Document doc;

                using (ISession session = NHibernateHelper.OpenSession())
                {

                    doc = session.Get<Document>(id);

                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        if (doc.User.UserName != HttpContext.User.Identity.Name)
                        {
                            ModelState.AddModelError("", "Вы не можете удалить чужой документ!");
                        }

                        if (ModelState.IsValid)
                        {
                            session.Delete(doc);
                            transaction.Commit();

                            WordDocHelper.Delete(doc.Path);

                            return RedirectToAction("Index");
                        }
                    }
                }

                return View(doc);
            }
            catch (Exception ex)
            {
                return View(ex.StackTrace);
            }
        }
    }
}