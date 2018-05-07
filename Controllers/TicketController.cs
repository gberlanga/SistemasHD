using SistemasHD.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.Services;

namespace SistemasHD.Controllers
{
    public class TicketController : Controller
    {
        private SistemasHDContext _db = new SistemasHDContext();
        //=================Start Index========================//
        public ActionResult Index(int filter)
        {
            var __EMAIL = GetEmail();
            User user = _db.Users.SingleOrDefault(u => u.Email.Contains(__EMAIL));
            if (user.Role.Equals(1)) return RedirectToAction("IndexAdmin", new { filter = 0 });
            List<Ticket> tickets = _db.Tickets.Where(t => t.ReceiverEmail.Equals(user.Email)).ToList();
            if(filter.Equals(0)) tickets = tickets.Where(t => t.Status != "Cerrado").ToList();
            else
            {
                string status = _db.Statuses.Find(filter).Name;
                tickets = tickets.Where(t => t.Status.Equals(status)).ToList();
            }
            var ticketsorted = tickets.OrderBy(s => s.Id).ToList();
            return View(ticketsorted);
        }

        public ActionResult IndexAdmin(int filter)
        {
            var __EMAIL = GetEmail();
            User user = _db.Users.SingleOrDefault(u => u.Email.Contains(__EMAIL));
            if(user.Role != 1) return RedirectToAction("Index", new { filter = 0 });
            List<Ticket> tickets;
            if (filter.Equals(0)) tickets = _db.Tickets.Where(t => t.Status != "Cerrado").ToList();
            else
            {
                string status = _db.Statuses.Find(filter).Name;
                tickets = _db.Tickets.Where(t => t.Status.Equals(status)).ToList();
            }
            var ticketsorted = tickets.OrderBy(s => s.Id).ToList();
            return View(ticketsorted);
        }

        public ActionResult IndexMyTickets(int filter)
        {
            var __EMAIL = GetEmail();
            User user = _db.Users.SingleOrDefault(u => u.Email.Contains(__EMAIL));
            if (user.Role != 1) return RedirectToAction("Index", new { filter = 0 });
            List<Ticket> tickets = _db.Tickets.Where(t => t.ReceiverEmail.Equals(user.Email)).ToList();
            if(filter.Equals(0)) tickets = tickets.Where(t => t.Status != "Cerrado").ToList();
            else
            {
                string status = _db.Statuses.Find(filter).Name;
                tickets = tickets.Where(t => t.Status.Equals(status)).ToList();
            }
            var ticketsorted = tickets.OrderBy(s => s.Id).ToList();
            return View(ticketsorted);
        }

        public ActionResult IndexTicketsByMe(int filter)
        {
            var __EMAIL = GetEmail();
            User user = _db.Users.SingleOrDefault(u => u.Email.Contains(__EMAIL));
            List<Ticket> tickets = _db.Tickets.Where(t => t.SenderEmail.Equals(user.Email)).ToList();
            if (filter.Equals(0)) tickets = tickets.Where(t => t.Status != "Cerrado").ToList();
            else
            {
                string status = _db.Statuses.Find(filter).Name;
                tickets = tickets.Where(t => t.Status.Equals(status)).ToList();
            }
            var ticketsorted = tickets.OrderBy(s => s.Id).ToList();
            return View(ticketsorted);
        }

        //=================End Index==========================//

        //=============Start GET/POST METHODS=================//

       //Gets
        public ActionResult New()
        {
            List<User> users = _db.Users.ToList();
            ViewBag.User = new SelectList(users, "Email", "Name");
            ViewBag.Category = Categories();
            ViewBag.Module = Modules();
            ViewBag.Company = Companies();
            ViewBag.Contpaq = ModulesContpaq();
            ViewBag.Portal = CompaniesPortal();
            return View();
        }

        public ActionResult PendInfo(int id)
        {
            ViewData["Task"] = _db.Tickets.Find(id);
            return View();
        }

        public ActionResult PendVoBo(int id)
        {
            ViewData["Task"] = _db.Tickets.Find(id);
            return View();
        }

        public ActionResult DesarrolloEK(int id)
        {
            ViewData["Task"] = _db.Tickets.Find(id);
            return View();
        }

        public ActionResult PendProv(int id)
        {
            ViewData["Task"] = _db.Tickets.Find(id);
            return View();
        }

        public ActionResult PendUser(int id)
        {
            ViewData["Task"] = _db.Tickets.Find(id);
            return View();
        }

        public ActionResult SendInfo(int id)
        {
            ViewData["Task"] = _db.Tickets.Find(id);
            return View();
        }

        public ActionResult ReturnTicket(int id)
        {
            ViewData["Task"] = _db.Tickets.Find(id);
            return View();
        }

        public ActionResult Comment(int id)
        {
            ViewData["Task"] = _db.Tickets.Find(id);
            return View();
        }

        public ActionResult Commit(int id)
        {
            ViewData["Task"] = _db.Tickets.Find(id);
            return View();
        }

        public ActionResult Detail(int id)
        {
            var task = _db.Tickets.Find(id);
            var currentEmail = GetEmail();
            User receiver = _db.Users.SingleOrDefault(u => u.Email == task.ReceiverEmail);
            User sender = _db.Users.SingleOrDefault(u => u.Email == task.SenderEmail);
            User current = _db.Users.SingleOrDefault(u => u.Email.Contains(currentEmail));
            ViewBag.StatusList = new SelectList(_db.Statuses, "Id", "Name");
            ViewData["Task"] = task;
            ViewData["Receiver"] = receiver;
            ViewData["Sender"] = sender;
            ViewData["Current"] = current;
            List<Message> messages = _db.Messages.Where(m => m.IdTicket.Equals(id)).ToList();
            var sortedList = messages.OrderBy(o => o.SendDate).ToList();
            sortedList.Reverse();
            return View(sortedList);
        }

        public ActionResult ChangeUser(int id)
        {
            List<User> users = _db.Users.ToList();
            ViewBag.User = new SelectList(users, "Email", "Name");
            ViewData["Task"] = _db.Tickets.Find(id);
            return View();
        }

        //Posts
        [HttpPost, ActionName("New")]
        public ActionResult New([Bind(Include = "Category, Module, Description, Company")] Ticket ticket, HttpPostedFileBase file, string Contpaq, string Portal, string User, string proposal) 
        {
            if (ModelState.IsValid)
            {
                var now = GetDate();
                ticket.StartDate = now;
                ticket.LastUpdate = now;
                var email = GetEmail();
                ticket.SenderEmail = _db.Users.SingleOrDefault(u => u.Email.Contains(email)).Email;
                ticket.ReceiverEmail = User;
                ticket.Company = ticket.Category.Equals("Portal") ? Portal : ticket.Company;
                ticket.Module = ticket.Category.Equals("Contpaq") ? Contpaq : ticket.Module;
                ticket.Company = ticket.Category != "Portal" && ticket.Category != "ENKONTROL" ? "Sin Compañia" : ticket.Company;
                ticket.Module = ticket.Category != "Contpaq" && ticket.Category != "ENKONTROL" ? "Sin Modulo" : ticket.Module;
                ticket.Attendance = false;
                ticket.Status = _db.Statuses.Find(1).Name;
                if(proposal != "")
                {
                    ticket.ProposalDate = StringToDateTime(proposal);
                }
                _db.Tickets.Add(ticket);
                _db.SaveChanges();
                AddFileNew(ticket.Id, file);
                SendMail(ticket.Id);
                return RedirectToAction("Index", new { filter = 0 });

                // add file case
            }
            return RedirectToAction("New");
        }

        [HttpPost, ActionName("PendInfo")]
        public ActionResult PendInfo([Bind(Include = "Detail")] int id, Message message, HttpPostedFileBase file)
        {
            string NAME = GetUserName();
            User currentUser = _db.Users.SingleOrDefault(u => u.Name.Equals(NAME));
            Ticket ticket = _db.Tickets.Find(id);
            message.IdTicket = id;
            message.SendDate = GetDate();
            message.SenderEmail = currentUser.Email;
            message.SubStatus = "Pendiente de Informacion";
            var status = _db.Statuses.Find(2);
            ticket.Status = status.Name;
            _db.Messages.Add(message);
            _db.SaveChanges();
            AddFileMessage(message.Id, file);
            SendMessageMail(message.Id, 0);
            return RedirectToAction("Detail", new { ticket.Id });
        }

        [HttpPost, ActionName("PendVoBo")]
        public ActionResult PendVoBo([Bind(Include = "Detail")] int id, Message message, HttpPostedFileBase file)
        {
            string NAME = GetUserName();
            User currentUser = _db.Users.SingleOrDefault(u => u.Name.Equals(NAME));
            Ticket ticket = _db.Tickets.Find(id);
            message.IdTicket = id;
            message.SendDate = GetDate();
            message.SenderEmail = currentUser.Email;
            message.SubStatus = "Pendiente de Visto Bueno";
            var status = _db.Statuses.Find(3);
            ticket.Status = status.Name;
            _db.Messages.Add(message);
            _db.SaveChanges();
            AddFileMessage(message.Id, file);
            SendMessageMail(message.Id, 1);
            return RedirectToAction("Detail", new { ticket.Id });
        }

        [HttpPost, ActionName("DesarrolloEK")]
        public ActionResult DesarrolloEK([Bind(Include = "Detail")] int id, Message message, HttpPostedFileBase file)
        {
            string NAME = GetUserName();
            User currentUser = _db.Users.SingleOrDefault(u => u.Name.Equals(NAME));
            Ticket ticket = _db.Tickets.Find(id);
            message.IdTicket = id;
            message.SendDate = GetDate();
            message.SenderEmail = currentUser.Email;
            message.SubStatus = "Desarrollo de Enkontrol";
            var status = _db.Statuses.Find(4);
            ticket.Status = status.Name;
            _db.Messages.Add(message);
            _db.SaveChanges();
            AddFileMessage(message.Id, file);
            SendMessageMail(message.Id, 2);
            return RedirectToAction("Detail", new { ticket.Id });
        }

        [HttpPost, ActionName("PendProv")]
        public ActionResult PendProv([Bind(Include = "Detail")] int id, Message message, HttpPostedFileBase file)
        {
            string NAME = GetUserName();
            User currentUser = _db.Users.SingleOrDefault(u => u.Name.Equals(NAME));
            Ticket ticket = _db.Tickets.Find(id);
            message.IdTicket = id;
            message.SendDate = GetDate();
            message.SenderEmail = currentUser.Email;
            message.SubStatus = "Pendiente de Proveedor";
            var status = _db.Statuses.Find(5);
            ticket.Status = status.Name;
            _db.Messages.Add(message);
            _db.SaveChanges();
            AddFileMessage(message.Id, file);
            SendMessageMail(message.Id, 3);
            return RedirectToAction("Detail", new { ticket.Id });
        }

        [HttpPost, ActionName("PendUser")]
        public ActionResult PendUser([Bind(Include = "Detail")] int id, Message message, HttpPostedFileBase file)
        {
            string NAME = GetUserName();
            User currentUser = _db.Users.SingleOrDefault(u => u.Name.Equals(NAME));
            Ticket ticket = _db.Tickets.Find(id);
            message.IdTicket = id;
            message.SendDate = GetDate();
            message.SenderEmail = currentUser.Email;
            message.SubStatus = "Pendiente por Usuario";
            var status = _db.Statuses.Find(6);
            ticket.Status = status.Name;
            _db.Messages.Add(message);
            _db.SaveChanges();
            AddFileMessage(message.Id, file);
            SendMessageMail(message.Id, 4);
            return RedirectToAction("Detail", new { ticket.Id });
        }

        [HttpPost, ActionName("SendInfo")]
        public ActionResult SendInfo([Bind(Include = "Detail")] int id, Message message, HttpPostedFileBase file)
        {
            string NAME = GetUserName();
            User currentUser = _db.Users.SingleOrDefault(u => u.Name.Equals(NAME));
            Ticket ticket = _db.Tickets.Find(id);
            message.IdTicket = id;
            message.SendDate = GetDate();
            message.SenderEmail = currentUser.Email;
            message.SubStatus = "Envio de Informacion";
            var status = _db.Statuses.Find(1);
            ticket.Status = status.Name;
            _db.Messages.Add(message);
            _db.SaveChanges();
            AddFileMessage(message.Id, file);
            SendMessageMail(message.Id, 5);
            return RedirectToAction("Detail", new { ticket.Id });
        }

        [HttpPost, ActionName("ReturnTicket")]
        public ActionResult ReturnTicket([Bind(Include = "Detail")] int id, Message message, HttpPostedFileBase file)
        {
            string NAME = GetUserName();
            User currentUser = _db.Users.SingleOrDefault(u => u.Name.Equals(NAME));
            Ticket ticket = _db.Tickets.Find(id);
            message.IdTicket = id;
            message.SendDate = GetDate();
            message.SenderEmail = currentUser.Email;
            message.SubStatus = "Retorno de Ticket";
            var status = _db.Statuses.Find(1);
            ticket.Status = status.Name;
            _db.Messages.Add(message);
            _db.SaveChanges();
            AddFileMessage(message.Id, file);
            SendMessageMail(message.Id, 6);
            return RedirectToAction("Detail", new { ticket.Id });
        }

        [HttpPost, ActionName("Comment")]
        public ActionResult Comment([Bind(Include = "Detail")] int id, Message message, HttpPostedFileBase file)
        {
            string NAME = GetUserName();
            User currentUser = _db.Users.SingleOrDefault(u => u.Name.Equals(NAME));
            Ticket ticket = _db.Tickets.Find(id);
            message.IdTicket = id;
            message.SendDate = GetDate();
            message.SenderEmail = currentUser.Email;
            message.SubStatus = "Comentario";
            _db.Messages.Add(message);
            _db.SaveChanges();
            AddFileMessage(message.Id, file);
            return RedirectToAction("Detail", new { ticket.Id });
        }

        [HttpPost, ActionName("Commit")]
        public ActionResult Commit(int id, string commitment)
        {
            if (commitment != "")
            {
                Ticket ticket = _db.Tickets.Find(id);
                ticket.CommitmentDate = StringToDateTime(commitment);
                _db.SaveChanges();
                return RedirectToAction("Index", new { filter = 0 });
            }
                return RedirectToAction("Index", new { filter = 0 });
        }

        [HttpPost,ActionName("ChangeUser")]
        public ActionResult ChangeUser(int id, string User)
        {
            Ticket ticket = _db.Tickets.Find(id);
            ticket.ReceiverEmail = User;
            _db.SaveChanges();
            return RedirectToAction("Detail", new { ticket.Id});
        }

        //==============End GET/POST METHODS==================//


        //==============Start PUBLIC METHODS==================//
        public string GetEmail()
        {
            string email = User.Identity.Name;
            int index = email.IndexOf('\\');
            return email.Substring(index + 1);
        }

        public List<SelectListItem> Categories()
        {
            List<SelectListItem> Categories = new List<SelectListItem>();
            //Add Categories items!!! (Eg. Categories.Add(new SelectListItem{ Text = "", Value = "" }));
            Categories.Add(new SelectListItem { Text = "ENKONTROL", Value = "ENKONTROL" });
            Categories.Add(new SelectListItem { Text = "Equipo de Computo", Value = "Equipo de Computo" });
            Categories.Add(new SelectListItem { Text = "Correos", Value = "Correos" });
            Categories.Add(new SelectListItem { Text = "Internet/Teléfonia", Value = "Internet/Teléfonia" });
            Categories.Add(new SelectListItem { Text = "Impresoras", Value = "Impresoras" });
            Categories.Add(new SelectListItem { Text = "Software", Value = "Software" });
            Categories.Add(new SelectListItem { Text = "Audio/Video", Value = "Audio/Video" });
            Categories.Add(new SelectListItem { Text = "PowerBI", Value = "PowerBI" });
            Categories.Add(new SelectListItem { Text = "Reportes", Value = "Reportes" });
            Categories.Add(new SelectListItem { Text = "Contpaq", Value = "Contpaq" });
            Categories.Add(new SelectListItem { Text = "Portal", Value = "Portal" });
            Categories.Add(new SelectListItem { Text = "Infofin", Value = "Infofin" });
            Categories.Add(new SelectListItem { Text = "Antilavado", Value = "Antilavado" });
            Categories.Add(new SelectListItem { Text = "Desarrollo", Value = "Desarrollo" });
            return Categories;
        }

        public List<SelectListItem> Modules()
        {
            List<SelectListItem> Modules = new List<SelectListItem>();
            Modules.Add(new SelectListItem { Text = "Bancos", Value = "Bancos" });
            Modules.Add(new SelectListItem { Text = "Clientes", Value = "Clientes" });
            Modules.Add(new SelectListItem { Text = "Compras", Value = "Compras" });
            Modules.Add(new SelectListItem { Text = "Contabilidad", Value = "Contabilidad" });
            Modules.Add(new SelectListItem { Text = "GIS", Value = "GIS" });
            Modules.Add(new SelectListItem { Text = "Inventarios", Value = "Inventarios" });
            Modules.Add(new SelectListItem { Text = "Precios Unitarios", Value = "Precios Unitarios" });
            Modules.Add(new SelectListItem { Text = "Proveedores", Value = "Proveedores" });
            Modules.Add(new SelectListItem { Text = "Sembrados", Value = "Sembrados" });
            Modules.Add(new SelectListItem { Text = "Vivienda", Value = "Vivienda" });
            Modules.Add(new SelectListItem { Text = "Creditos Puente", Value = "Creditos Puente" });
            Modules.Add(new SelectListItem { Text = "Renta", Value = "Renta" });
            Modules.Add(new SelectListItem { Text = "Otros", Value = "Otros" });

            return Modules;
        }

        public List<SelectListItem> Companies()
        {
            List<SelectListItem> Companies = new List<SelectListItem>();
            Companies.Add(new SelectListItem { Text = "01 Centro de Contrucciones Modernas S.A. de C.V.", Value = "01 Centro de Contrucciones Modernas S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "02 Parque Industrial Apodaca S.A. de C.V.", Value = "02 Parque Industrial Apodaca S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "03 Promotora R R S.A. de C.V.", Value = "03 Promotora R R S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "04 RegioValores S.A. de C.V.", Value = "04 RegioValores S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "05 Inmobiliaria Vide Regia S.A. de C.V.", Value = "05 Inmobiliaria Vide Regia S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "06 Servicios Terra Regia S.A. de C.V.", Value = "06 Servicios Terra Regia S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "07 Acciones Escorpion S.A. de C.V.", Value = "07 Acciones Escorpion S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "08 TerraValor S.A. de C.V.", Value = "08 TerraValor S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "09 Tenedora y Operadora Inmobiliaria S.A. de C.V.", Value = "09 Tenedora y Operadora Inmobiliaria S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "10 TerraRegia S.A. de C.V.", Value = "10 TerraRegia S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "11 Plaza Magnolia S.A. de C.V.", Value = "11 Plaza Magnolia S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "12 Centro Comercial Las Colinas S.A. de C.V.", Value = "12 Centro Comercial Las Colinas S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "13 Centro Comercial Diego Diaz S.A. de C.V.", Value = "13 Centro Comercial Diego Diaz S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "14 ViveRegio S.A. de C.V.", Value = "14 ViveRegio S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "15 Inmobiliaria Tierra Oro S.A. de C.V.", Value = "15 Inmobiliaria Tierra Oro S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "16 Banco Regional de Monterrey 85100530", Value = "16 Banco Regional de Monterrey 85100530" });
            Companies.Add(new SelectListItem { Text = "17 Banco Regional de Monterrey SA Fideicomiso 85100495", Value = "17 Banco Regional de Monterrey SA Fideicomiso 85100495" });
            Companies.Add(new SelectListItem { Text = "18 Terra Regia Capital S.A. de C.V.", Value = "18 Terra Regia Capital S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "19 Banco Regional de Monterrey Fideicomiso 85100614", Value = "19 Banco Regional de Monterrey Fideicomiso 85100614" });
            Companies.Add(new SelectListItem { Text = "20 Terra Regia Cumbres S.A. de C.V.", Value = "20 Terra Regia Cumbres S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "21 Banco Regional de Monterrey Fideicomiso 85100631 S.A.", Value = "21 Banco Regional de Monterrey Fideicomiso 85100631 S.A." });
            Companies.Add(new SelectListItem { Text = "22 Inmobiliaria Bazam 2010 S.A. de C.V.", Value = "22 Inmobiliaria Bazam 2010 S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "23 Operadora TSH S.A. de C.V.", Value = "23 Operadora TSH S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "24 Terra Regia D.I. S A P I DE C.V.", Value = "24 Terra Regia D.I. S A P I DE C.V." });
            Companies.Add(new SelectListItem { Text = "25 Operadora Terra Regia S.A. de C.V.", Value = "25 Operadora Terra Regia S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "26 Operadora ViveRegio S.A. de C.V.", Value = "26 Operadora ViveRegio S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "27 TR Cumbres S.A. de C.V.", Value = "27 TR Cumbres S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "28 Operadora Mil Cien S A P I DE C.V.", Value = "28 Operadora Mil Cien S A P I DE C.V." });
            Companies.Add(new SelectListItem { Text = "29 Casas y Desarrollos TR S.A. de C.V.", Value = "29 Casas y Desarrollos TR S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "30 Operadora Siete Hectareas", Value = "30 Operadora Siete Hectareas" });
            Companies.Add(new SelectListItem { Text = "31 Desarrolladora TerraSol S.A. de C.V.", Value = "31 Desarrolladora TerraSol S.A. de C.V." });
            Companies.Add(new SelectListItem { Text = "32 Dominio Cumbres S.A. de C.V.", Value = "32 Dominio Cumbres S.A. de C.V." });
            return Companies;
        }
        
        public List<SelectListItem> ModulesContpaq()
        {
            List<SelectListItem> Modules = new List<SelectListItem>();
            Modules.Add(new SelectListItem { Text = "Contabilidad", Value = "Contabilidad" });
            Modules.Add(new SelectListItem { Text = "Bancos", Value = "Bancos" });
            Modules.Add(new SelectListItem { Text = "Nomina", Value = "Nomina" });
            Modules.Add(new SelectListItem { Text = "Facturación", Value = "Facturación" });
            Modules.Add(new SelectListItem { Text = "XML en Linea", Value = "XML en Linea" });
            Modules.Add(new SelectListItem { Text = "Docmuentos Digitales (ADD)", Value = "Docmuentos Digitales (ADD)" });
            return Modules;
        }

        public List<SelectListItem> CompaniesPortal()
        {
            List<SelectListItem> Companies = new List<SelectListItem>();
            Companies.Add(new SelectListItem { Text = "TerraRegia", Value = "TerraRegia" });
            Companies.Add(new SelectListItem { Text = "VivaRegio", Value = "VivaRegio" });
            return Companies;
        }

        public ActionResult Close(int id)
        {
            Ticket ticket = _db.Tickets.Find(id);
            ticket.FinishDate = GetDate();
            ticket.Status = _db.Statuses.Find(7).Name;
            _db.SaveChanges();
            return RedirectToAction("Index", new { filter = 0 });
        }

        public string DaysUntil(int id)
        {
            Ticket ticket = _db.Tickets.Find(id);
            DateTime commitment;
            if (ticket.CommitmentDate == null) { return "Pendiente"; }
            else commitment = ticket.CommitmentDate ?? DateTime.Now; ;
            TimeSpan days;
            string daysUntil;
            if (ticket.Status.Equals("Cerrado"))
            {
                days = commitment.Subtract(ticket.FinishDate ?? DateTime.Now);
            }
            else
            {
                days = commitment.Subtract(GetDate());
            }
            int TotalDays = days.Days < 0 ? days.Days : days.Days + 1;
            if (TotalDays < -1) daysUntil = (TotalDays * -1) + " Dias Tarde";
            else if (TotalDays.Equals(-1)) daysUntil = (TotalDays * -1) + " Día Tarde";
            else if (TotalDays.Equals(1)) daysUntil = TotalDays + " Día Restante";
            else daysUntil = TotalDays + " Dias Restantes";
            return daysUntil;
        }

        public FileResult Download(int id)
        {
            if (getAttachment(id))
            {
                string fileName = _db.Tickets.Find(id).FilePath;
                string _path = Path.Combine("\\\\192.168.2.22\\SistemasHelpDeskFiles", fileName);
                byte[] fileBytes = System.IO.File.ReadAllBytes(_path);
                return File(fileBytes, MediaTypeNames.Application.Octet, fileName);
            }
            return null;

        }

        public FileResult DownloadFromMessage(int id)
        {
            if (getAttachmentFromMessage(id))
            {
                string fileName = _db.Messages.Find(id).FilePath;
                string _path = Path.Combine("\\\\192.168.2.22\\SistemasHelpDeskFiles", fileName);
                byte[] fileBytes = System.IO.File.ReadAllBytes(_path);
                return File(fileBytes, MediaTypeNames.Application.Octet, fileName);
            }
            return null;

        }

        public bool FileExists(int id)
        {
            return _db.Messages.Find(id).FilePath != null ? true : false;
        }

        public DateTime StringToDateTime(string s)
        {
            var month = s.Substring(0, 2);
            var day = s.Substring(3, 2);
            var year = s.Substring(6);
            return new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
        }

        public bool getAttachment(int id)
        {
            return _db.Tickets.Find(id).FilePath != null ? true : false;
        }

        public bool getAttachmentFromMessage(int id)
        {
            return _db.Messages.Find(id).FilePath != null ? true : false;
        }

        public string GetUserName()
        {
            string identity = User.Identity.Name;
            int index = identity.IndexOf('\\');
            var name = identity.Substring(index + 1);
            List<User> users = _db.Users.ToList();
            var user = _db.Users.SingleOrDefault(u => u.Email.Contains(name));
            return user.Name;
        }

        public DateTime GetDate()
        {
            DateTime utcTime = DateTime.UtcNow;
            TimeZoneInfo mty = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, mty);
        }

        public string GetDateFormat(DateTime dateTime)
        {
            string date = dateTime.Day + "/" + dateTime.Month + "/" + dateTime.Year;
            return date;
        }

        public string GetMessage(int id)
        {
            Message msg = _db.Messages.Find(id);
            var message = "<b>" + _db.Users.SingleOrDefault(u => u.Email == msg.SenderEmail).Name +
                "</b>: " + msg.Detail;
            return message;
        }

        public string GetName(string email)
        {
            return _db.Users.SingleOrDefault(u => u.Email.Equals(email)).Name;
        }

        public string GetStatus(int id)
        {
            Ticket ticket = _db.Tickets.Find(id);
            var statusName = ticket.Status;
            var statusId = _db.Statuses.SingleOrDefault(s => s.Name.Equals(statusName)).Id;
            var clss = "";
            var style = "";
            if (statusId.Equals(1))
            {
                clss = "badge badge-pill badge-warning";
                style = "color: #fff; background-color: #f0ad4e; border-color: #eea236;";
            }
            else if (statusId.Equals(2) || statusId.Equals(3))
            {
                clss = "badge badge-pill badge-primary";
                style = "color: #fff; background-color:  #337ab7; border-color: #2e6da4;";
            }
            else if (statusId.Equals(4) || statusId.Equals(5) || statusId.Equals(6))
            {
                clss = "badge badge-pill badge-success";
                style = "color: #fff; background-color:  #5cb85c; border-color: #4cae4c;";
            }
            else if (statusId.Equals(7))
            {
                clss = "badge badge-pill badge-danger";
                style = "color: #fff; background-color: #d9534f; border-color: #d43f3a;";
            }
            var status = "<span class = \"" + clss + "\" style = \"" + style + "\">" + ticket.Status+ "</span>";

            return status;
        }

        public string DateString(DateTime date)
        {
            return date.Day + "/" + date.Month + "/" + date.Year;
        }

        public void AddFileNew(int id, HttpPostedFileBase file)
        {
            Ticket ticket = _db.Tickets.Find(id);
            if (file == null) return;
            if(file != null && ValidExtension(Path.GetExtension(file.FileName)))
            {
                if(file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + ticket.Id + Path.GetExtension(file.FileName);
                    // get shared folder for IT Helpdesk
                    string _Path = Path.Combine("\\\\192.168.2.22\\SistemasHelpDeskFiles", _FileName);
                    file.SaveAs(_Path);
                    ticket.FilePath = _FileName;
                    _db.SaveChanges();
                    return;
                }
            }
        }

        public void AddFileMessage(int messageId, HttpPostedFileBase file)
        {
            Message message = _db.Messages.Find(messageId);
            if (file == null) return;
            if (file != null && ValidExtension(Path.GetExtension(file.FileName)))
            {
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + message.IdTicket + "_" + message.Id + Path.GetExtension(file.FileName);
                    // get shared folder for IT Helpdesk
                    string _Path = Path.Combine("\\\\192.168.2.22\\SistemasHelpDeskFiles", _FileName);
                    file.SaveAs(_Path);
                    message.FilePath = _FileName;
                    _db.SaveChanges();
                    return;
                }
            }
        }

        private bool ValidExtension(string ext)
        {
            ext = ext.ToLower();
            switch (ext)
            {
                case ".txt":
                case ".rtf":
                case ".doc":
                case ".docx":
                case ".ppt":
                case ".pptx":
                case ".xls":
                case ".xlsx":
                case ".xlsm":
                case ".pbx":
                case ".pdf":
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".zip":
                case ".rar":
                case ".xml":
                    return true;
                default:
                    return false;
            }
        }

        [WebMethod]
        [ScriptMethod]
        [HttpPost]
        public ActionResult UpdateStatus(string status, string id)
        {
            using (SistemasHDContext _db = new SistemasHDContext())
            {
                var now = GetDate();
                var newStatusNum = Int32.Parse(status);
                var newStatus = _db.Statuses.Find(newStatusNum).Name;
                int newId = Int32.Parse(id);
                Ticket ticket = _db.Tickets.Find(newId);
                //ticket.LastUpdate = now.DateTime;
                ticket.LastUpdate = now;
                if (newStatusNum.Equals(7))
                {
                    ticket.Status = newStatus;
                    ticket.FinishDate = now;
                    _db.SaveChanges();
                    return Json(new { success = true, message = "Cerrado Exitoso!" }, JsonRequestBehavior.AllowGet);
                }
                else if (newStatusNum.Equals(4))
                {
                    ticket.Status = newStatus;
                    _db.SaveChanges();
                    return Json(new { success = true, message = "Se envio a desarrollo EK" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ticket.Status = newStatus;
                    _db.SaveChanges();
                    return Json(new { success = true, message = "Cambio Exitoso!" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [WebMethod]
        [ScriptMethod]
        [HttpPost]
        public ActionResult ChangeUser2(string user, string id)
        {
            int newId = Int32.Parse(id);
            Ticket ticket = _db.Tickets.Find(newId);
            var newUser = user + "@terraregia.com";
            ticket.ReceiverEmail = newUser;
            _db.SaveChanges();
            return Json(new { success = true, message = "Cambio Exitoso!" }, JsonRequestBehavior.AllowGet);
        }

        //===============End PUBLIC METHODS===================//


        //===============Start MAILS CONFIG===================//
        public void SendMail(int id)
        {
            Ticket ticket = _db.Tickets.Find(id);

            MailMessage message = new MailMessage();
            message.From = new MailAddress("helpdesk@terraregia.com");

            message.To.Add(new MailAddress(ticket.ReceiverEmail));

            var module = ticket.Module.Equals(null) ? "Sin modulo" : ticket.Module;
            var company = ticket.Company.Equals(null) ? "Sin Compañia" : ticket.Company;

            message.Subject = "[NUEVA TAREA] asignada por: " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.SenderEmail)).Name;

            message.Body =
                  "<h1>Tarea# " + id + "</h1> <br>" +
                  "<p>Hola " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.ReceiverEmail)).Name + " se te asigno esta tarea.</p>" +
                  "<b>Categoria:</b> " + ticket.Category + "<br>" +
                  "<b>Modulo:</b> " + module + "<br>" +
                  "<b>Descripción:</b> " + ticket.Description + "<br>" +
                  "<b>En la compañia:</b> " + company;

            message.IsBodyHtml = true;

            //Attach File

            // init service
            SmtpClient client = new SmtpClient("smtp.office365.com", 25);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            // helpdesk credentials
            client.Credentials = new NetworkCredential("helpdesk@terraregia.com", "Terra%8&");
            client.Timeout = 25000;

            // sent mail
            client.Send(message);
        }

        public void SendMessageMail(int id, int filter)
        {
            Message message = _db.Messages.Find(id);

            Ticket ticket = _db.Tickets.Find(message.IdTicket);

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("helpdesk@terraregia.com");

            var module = ticket.Module.Equals(null) ? "Sin modulo" : ticket.Module;
            var company = ticket.Company.Equals(null) ? "Sin Compañia" : ticket.Company;

            //Pendiente de informacion
            if (filter.Equals(0))
            {
                msg.To.Add(new MailAddress(ticket.SenderEmail));
                msg.Subject = "[PENDIENTE DE INFORMACION] Ticket #" + ticket.Id + " : " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.ReceiverEmail)).Name;

                msg.Body =
                      "<h1>Tarea# " + ticket.Id + "</h1> <br>" +
                      "<p>Hola " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.SenderEmail)).Name + " Hace falta informacíon.</p>" +
                      "<b>Descripción:</b> " + message.Detail;
            }

            //Pendiente de Visto Bueno
            else if (filter.Equals(1))
            {
                msg.To.Add(new MailAddress(ticket.SenderEmail));
                msg.Subject = "[PENDIENTE DE VISTO BUENO] Ticket #" + ticket.Id + " : " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.ReceiverEmail)).Name;

                msg.Body =
                      "<h1>Tarea# " + ticket.Id + "</h1> <br>" +
                      "<p>Hola " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.SenderEmail)).Name + " Hace falta dar el Visto Bueno.</p>" +
                      "<b>Descripción:</b> " + message.Detail;
            }

            //Desarrollo de Enkontrol
            else if (filter.Equals(2))
            {
                msg.To.Add(new MailAddress(ticket.SenderEmail));
                msg.Subject = "[Desarrollo de Enkontrol] Ticket #" + ticket.Id + " : " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.ReceiverEmail)).Name;

                msg.Body =
                      "<h1>Tarea# " + ticket.Id + "</h1> <br>" +
                      "<p>Hola " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.SenderEmail)).Name + " Se envio a enkontrol.</p>" +
                      "<b>Descripción:</b> " + message.Detail;
            }

            //Pendiente de Proveedor
            else if (filter.Equals(3))
            {
                msg.To.Add(new MailAddress(ticket.SenderEmail));
                msg.Subject = "[PENDIENTE DE PROVEEDOR] Ticket #" + ticket.Id + " : " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.ReceiverEmail)).Name;

                msg.Body =
                      "<h1>Tarea# " + ticket.Id + "</h1> <br>" +
                      "<p>Hola " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.SenderEmail)).Name + " En espera al proveedor.</p>" +
                      "<b>Descripción:</b> " + message.Detail;
            }

            //Pendiente de Usuario
            else if (filter.Equals(4))
            {
                msg.To.Add(new MailAddress(ticket.SenderEmail));
                msg.Subject = "[PENDIENTE DE USUARIO] Ticket #" + ticket.Id + " : " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.ReceiverEmail)).Name;

                msg.Body =
                      "<h1>Tarea# " + ticket.Id + "</h1> <br>" +
                      "<p>Hola " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.SenderEmail)).Name + " En espera al Usuario.</p>" +
                      "<b>Descripción:</b> " + message.Detail;
            }

            //Envio de Informacion
            else if (filter.Equals(5))
            {
                msg.To.Add(new MailAddress(ticket.ReceiverEmail));
                msg.Subject = "[ENVIO DE INFORMACION] Ticket #" + ticket.Id + " : " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.SenderEmail)).Name;

                msg.Body =
                      "<h1>Tarea# " + ticket.Id + "</h1> <br>" +
                      "<p>Hola " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.ReceiverEmail)).Name + " Informacíon enviada.</p>" +
                      "<b>Descripción:</b> " + message.Detail;
            }

            //Retorno de ticket
            else if (filter.Equals(6))
            {
                msg.To.Add(new MailAddress(ticket.ReceiverEmail));
                msg.Subject = "[RETORNO] Ticket #" + ticket.Id + " : " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.SenderEmail)).Name;

                msg.Body =
                      "<h1>Tarea# " + ticket.Id + "</h1> <br>" +
                      "<p>Hola " + _db.Users.SingleOrDefault(u => u.Email.Equals(ticket.ReceiverEmail)).Name + " Tarea retornada.</p>" +
                      "<b>Descripción:</b> " + message.Detail;
            }


            msg.IsBodyHtml = true;



            //Attach File

            // init service
            SmtpClient client = new SmtpClient("smtp.office365.com", 25);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            // helpdesk credentials
            client.Credentials = new NetworkCredential("helpdesk@terraregia.com", "Terra%8&");
            client.Timeout = 25000;

            // sent mail
            client.Send(msg);
        }
        //================End MAILS CONFIG====================//
    }
}