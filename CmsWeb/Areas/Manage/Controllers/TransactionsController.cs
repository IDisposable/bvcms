using System;
using System.Linq;
using System.Web.Mvc;
using CmsData;
using CmsData.Finance;
using CmsWeb.Models;
using UtilityExtensions;

namespace CmsWeb.Areas.Manage.Controllers
{
    [Authorize(Roles = "Edit, ManageTransactions")]
    [RouteArea("Manage", AreaPrefix = "Transactions"), Route("{action}/{id?}")]
    public class TransactionsController : CmsStaffController
    {
        [HttpGet]
        [Route("~/Transactions")]
        [Route("~/Transactions/{id:int}")]
        public ActionResult Index(int? id, int? goerid = null, int? senderid = null)
        {
            var m = new TransactionsModel(id) {GoerId = goerid, SenderId = senderid};
            return View(m);
        }

        [HttpPost]
        public ActionResult Report(DateTime? sdt, DateTime? edt)
        {
            var m = new TransactionsModel
            {
                startdt = sdt,
                enddt = edt,
                usebatchdates = true,
            };
            return View(m);
        }

        [HttpPost]
        public ActionResult ReportByDescription(DateTime? sdt, DateTime? edt)
        {
            var m = new TransactionsModel
            {
                startdt = sdt,
                enddt = edt,
                usebatchdates = true,
            };
            return View(m);
        }

        [HttpPost]
        public ActionResult ReportByBatchDescription(DateTime? sdt, DateTime? edt)
        {
            var m = new TransactionsModel
            {
                startdt = sdt,
                enddt = edt,
                usebatchdates = true,
            };
            return View(m);
        }

        [HttpPost]
        public ActionResult List(TransactionsModel m)
        {
            UpdateModel(m.Pager);
            return View(m);
        }

        [HttpPost]
        public ActionResult Export(TransactionsModel m)
        {
            return m.ToExcel();
        }

        [HttpGet]
        [Authorize(Roles = "Finance")]
        public ActionResult RunRecurringGiving()
        {
            var count = ManagedGiving.DoAllGiving(DbUtil.Db);
            return Content(count.ToString());
        }

        [HttpPost]
        [Authorize(Roles = "Developer")]
        public ActionResult SetParent(int id, int parid, TransactionsModel m)
        {
            var t = DbUtil.Db.Transactions.SingleOrDefault(tt => tt.Id == id);
            if (t == null)
                return Content("notran");
            t.OriginalId = parid;
            DbUtil.Db.SubmitChanges();
            return View("List", m);
        }

        [HttpPost]
        [Authorize(Roles = "ManageTransactions,Finance")]
        public ActionResult CreditVoid(int id, string type, decimal? amt, TransactionsModel m)
        {
            var t = DbUtil.Db.Transactions.SingleOrDefault(tt => tt.Id == id);
            if (t == null)
                return Content("notran");

            var qq = from tt in DbUtil.Db.Transactions
                where tt.OriginalId == id || tt.Id == id
                orderby tt.Id descending
                select tt;
            var t0 = qq.First();
            var gw = DbUtil.Db.Gateway(t.Testing ?? false);
            TransactionResponse resp = null;
            var re = t.TransactionId;
            if (re.Contains("(testing"))
                re = re.Substring(0, re.IndexOf("(testing)"));
            if (type == "Void")
            {
                resp = gw.VoidCreditCardTransaction(re);
                if (!resp.Approved)
                    resp = gw.VoidCheckTransaction(re);

                t.Voided = resp.Approved;
                amt = t.Amt;
            }
            else
            {
                if (t.PaymentType == PaymentType.Ach)
                    resp = gw.RefundCheck(re, amt ?? 0);
                else if (t.PaymentType == PaymentType.CreditCard)
                    resp = gw.RefundCreditCard(re, amt ?? 0);

                t.Credited = resp.Approved;
            }

            if (!resp.Approved)
                return Content("error: " + resp.Message);

            var transaction = new Transaction
            {
                TransactionId = resp.TransactionId + (t.Testing == true ? "(testing)" : ""),
                First = t.First,
                MiddleInitial = t.MiddleInitial,
                Last = t.Last,
                Suffix = t.Suffix,
                Amt = -amt,
                Amtdue = t0.Amtdue + amt,
                Approved = true,
                AuthCode = t.AuthCode,
                Message = t.Message,
                Donate = -t.Donate,
                Regfees = -t.Regfees,
                TransactionDate = DateTime.Now,
                TransactionGateway = t.TransactionGateway,
                Testing = t.Testing,
                Description = t.Description,
                OrgId = t.OrgId,
                OriginalId = t.OriginalId,
                Participants = t.Participants,
                Financeonly = t.Financeonly,
                PaymentType = t.PaymentType,
                LastFourCC = t.LastFourCC,
                LastFourACH = t.LastFourACH
            };

            DbUtil.Db.Transactions.InsertOnSubmit(transaction);
            DbUtil.Db.SubmitChanges();
            Util.SendMsg(Util.SysFromEmail, Util.Host,
                Util.TryGetMailAddress(DbUtil.Db.StaffEmailForOrg(transaction.OrgId ?? 0)),
                "Void/Credit Transaction Type: " + type,
                @"<table>
<tr><td>Name</td><td>{0}</td></tr>
<tr><td>Email</td><td>{1}</td></tr>
<tr><td>Address</td><td>{2}</td></tr>
<tr><td>Phone</td><td>{3}</td></tr>
<tr><th colspan=""2"">Transaction Info</th></tr>
<tr><td>Description</td><td>{4}</td></tr>
<tr><td>Amount</td><td>{5:N2}</td></tr>
<tr><td>Date</td><td>{6}</td></tr>
<tr><td>TranIds</td><td>Org: {7} {8}, Curr: {9} {10}</td></tr>
<tr><td>User</td><td>{11}</td></tr>
</table>".Fmt(Transaction.FullName(t), t.Emails, t.Address, t.Phone,
                    t.Description,
                    -amt,
                    t.TransactionDate.Value.FormatDateTm(),
                    t.Id,
                    t.TransactionId,
                    transaction.Id,
                    transaction.TransactionId,
                    Util.UserFullName
                    ), Util.EmailAddressListFromString(DbUtil.Db.StaffEmailForOrg(transaction.OrgId ?? 0)),
                0, 0);
            DbUtil.LogActivity("CreditVoid for " + t.TransactionId);

            return View("List", m);
        }

        [HttpPost]
        [Authorize(Roles = "ManageTransactions,Finance")]
        public ActionResult Adjust(int id, decimal amt, string desc, TransactionsModel m)
        {
            var qq = from tt in DbUtil.Db.Transactions
                where tt.OriginalId == id || tt.Id == id
                orderby tt.Id descending
                select tt;
            var t = qq.FirstOrDefault();

            if (t == null)
                return Content("notran");

            var t2 = new Transaction
            {
                TransactionId = "Adjustment",
                First = t.First,
                MiddleInitial = t.MiddleInitial,
                Last = t.Last,
                Suffix = t.Suffix,
                Amt = amt,
                Emails = t.Emails,
                Amtdue = t.Amtdue - amt,
                Approved = true,
                AuthCode = "",
                Message = desc,
                Donate = -t.Donate,
                Regfees = -t.Regfees,
                TransactionDate = DateTime.Now,
                TransactionGateway = t.TransactionGateway,
                Testing = t.Testing,
                Description = t.Description,
                OrgId = t.OrgId,
                OriginalId = t.OriginalId,
                Participants = t.Participants,
                Financeonly = t.Financeonly,
                PaymentType = t.PaymentType,
                LastFourCC = t.LastFourCC,
                LastFourACH = t.LastFourACH
            };

            DbUtil.Db.Transactions.InsertOnSubmit(t2);
            DbUtil.Db.SubmitChanges();
            Util.SendMsg(Util.SysFromEmail, Util.Host,
                Util.TryGetMailAddress(DbUtil.Db.StaffEmailForOrg(t2.OrgId ?? 0)),
                "Adjustment Transaction",
                @"<table>
<tr><td>Name</td><td>{0}</td></tr>
<tr><td>Email</td><td>{1}</td></tr>
<tr><td>Address</td><td>{2}</td></tr>
<tr><td>Phone</td><td>{3}</td></tr>
<tr><th colspan=""2"">Transaction Info</th></tr>
<tr><td>Description</td><td>{4}</td></tr>
<tr><td>Amount</td><td>{5:N2}</td></tr>
<tr><td>Date</td><td>{6}</td></tr>
<tr><td>TranIds</td><td>Org: {7} {8}, Curr: {9} {10}</td></tr>
</table>".Fmt(Transaction.FullName(t), t.Emails, t.Address, t.Phone,
                    t.Description,
                    t.Amt,
                    t.TransactionDate.Value.FormatDateTm(),
                    t.Id, t.TransactionId, t2.Id, t2.TransactionId
                    ), Util.EmailAddressListFromString(DbUtil.Db.StaffEmailForOrg(t2.OrgId ?? 0)),
                0, 0);
            DbUtil.LogActivity("Adjust for " + t.TransactionId);
            return View("List", m);
        }
    }
}
