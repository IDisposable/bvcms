using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CmsData;
using CmsData.Codes;
using CmsData.Registration;
using CmsWeb.Models;
using UtilityExtensions;

namespace CmsWeb.Areas.OnlineReg.Controllers
{
    public partial class OnlineRegController
    {
        // reached by the paylink in the confirmation email
        // which is produced in EnrollAndConfirm
        [HttpGet]
        public ActionResult PayAmtDue(string q)
        {
            Response.NoCache();

            if (!q.HasValue())
                return Message("unknown");
            var id = Util.Decrypt(q).ToInt2();
            var qq = from t in DbUtil.Db.Transactions
                     where t.OriginalId == id || t.Id == id
                     orderby t.Id descending
                     select t;
            var ti = qq.FirstOrDefault();

            var org = DbUtil.Db.LoadOrganizationById(ti.OrgId);
            var amtdue = PaymentForm.AmountDueTrans(DbUtil.Db, ti);
            if (amtdue == 0)
                return Message("no outstanding transaction");

#if DEBUG
            ti.Testing = true;
            if (!ti.Address.HasValue())
            {
                ti.Address = "235 Riveredge";
                ti.City = "Cordova";
                ti.Zip = "38018";
                ti.State = "TN";
            }
#endif
            var pf = PaymentForm.CreatePaymentFormForBalanceDue(ti, amtdue);

            SetHeaders(pf.OrgId ?? 0);

            if (OnlineRegModel.GetTransactionGateway() != "serviceu")
                return View("Payment/Process", pf);


            ViewBag.TranId = ti.Id;
            var pm = new PaymentModel
            {
                NameOnAccount = pf.FullName(),
                Address = pf.Address,
                Amount = pf.Amtdue,
                City = pf.City,
                Email = pf.Email,
                Phone = pf.Phone.FmtFone(),
                State = pf.State,
                PostalCode = pf.Zip,
                testing = pf.testing,
                PostbackURL = DbUtil.Db.ServerLink("/OnlineReg/ConfirmServiceU/" + id),
                Misc2 = pf.Description,
                Misc1 = pf.FullName(),
                _URL = pf.URL,
                _timeout = new PaymentForm().TimeOut,
            };

            return View("PayAmtDue/ServiceU", pm);
        }

        private static void ConfirmDuePaidTransaction(Transaction ti, string transactionId, bool sendmail)
        {
            var Db = DbUtil.Db;
            var org = Db.LoadOrganizationById(ti.OrgId);
            ti.TransactionId = transactionId;
            if (ti.Testing == true && !ti.TransactionId.Contains("(testing)"))
                ti.TransactionId += "(testing)";

            var amt = ti.Amt;
            foreach (var pi in ti.OriginalTrans.TransactionPeople)
            {
                var p = Db.LoadPersonById(pi.PeopleId);
                if (p != null)
                {
                    var om = Db.OrganizationMembers.SingleOrDefault(m => m.OrganizationId == ti.OrgId && m.PeopleId == pi.PeopleId);
                    if (om == null)
                        continue;
                    Db.SubmitChanges();
                    if (org.IsMissionTrip == true)
                    {
                        Db.GoerSenderAmounts.InsertOnSubmit(
                            new GoerSenderAmount 
                            {
                                Amount = ti.Amt,
                                GoerId = pi.PeopleId,
                                Created = DateTime.Now,
                                OrgId = org.OrganizationId,
                                SupporterId = pi.PeopleId,
                            });
                        var setting = new Settings(org.RegSetting, Db, org.OrganizationId);
                        var fund = setting.DonationFundId;
                        p.PostUnattendedContribution(Db, ti.Amt ?? 0, fund, 
                            "SupportMissionTrip: org={0}; goer={1}".Fmt(org.OrganizationId, pi.PeopleId), typecode: BundleTypeCode.Online);
                    }
                    var pay = amt;
                    if (org.IsMissionTrip != true)
                        ti.Amtdue = PaymentForm.AmountDueTrans(Db, ti);

                    var sb = new StringBuilder();
                    sb.AppendFormat("{0:g} ----------\n", Util.Now);
                    sb.AppendFormat("{0:c} ({1} id) transaction amount\n", ti.Amt, ti.Id);
                    sb.AppendFormat("{0:c} applied to this registrant\n", pay);
                    sb.AppendFormat("{0:c} total due all registrants\n", ti.Amtdue);

                    om.AddToMemberData(sb.ToString());
                    var reg = p.RecRegs.Single();
                    reg.AddToComments(sb.ToString());
                    reg.AddToComments("{0} ({1})".Fmt(org.OrganizationName, org.OrganizationId));

                    amt -= pay;
                }
                else
                    Db.Email(Db.StaffEmailForOrg(org.OrganizationId),
                        Db.PeopleFromPidString(org.NotifyIds),
                        "missing person on payment due",
                        "Cannot find {0} ({1}), payment due completed of {2:c} but no record".Fmt(pi.Person.Name, pi.PeopleId, pi.Amt));
            }
            Db.SubmitChanges();
            var names = string.Join(", ", ti.OriginalTrans.TransactionPeople.Select(i => i.Person.Name).ToArray());

            var pid = ti.FirstTransactionPeopleId();
            var p0 = Db.LoadPersonById(pid);
//todo: should we be sending to all TransactionPeople?
            if (sendmail)
            {
                if (p0 == null)
                    Util.SendMsg(Util.SysFromEmail, Util.Host, Util.TryGetMailAddress(Db.StaffEmailForOrg(org.OrganizationId)),
                        "Payment confirmation", "Thank you for paying {0:c} for {1}.<br/>Your balance is {2:c}<br/>{3}".Fmt(
                                ti.Amt, ti.Description, ti.Amtdue, names), 
                        Util.ToMailAddressList(Util.FirstAddress(ti.Emails)), 0, pid);
                else
                {
                    Db.Email(Db.StaffEmailForOrg(org.OrganizationId), p0, Util.ToMailAddressList(ti.Emails), 
                        "Payment confirmation", "Thank you for paying {0:c} for {1}.<br/>Your balance is {2:c}<br/>{3}".Fmt(
                                ti.Amt, ti.Description, ti.Amtdue, names), false);
                    Db.Email(p0.FromEmail,
                        Db.PeopleFromPidString(org.NotifyIds),
                        "payment received for " + ti.Description,
                        "{0} paid {1:c} for {2}, balance of {3:c}\n({4})".Fmt(
                            Transaction.FullName(ti), ti.Amt, ti.Description, ti.Amtdue, names));
                }
            }
        }

        public ActionResult ConfirmDuePaid(int? id, string transactionId, decimal amount)
        {
            if (!id.HasValue)
                return View("Unknown");
            if (!transactionId.HasValue())
                return Message("error no transaction");

            var ti = DbUtil.Db.Transactions.SingleOrDefault(tt => tt.Id == id);
            if (ti == null)
                return Message("no pending transaction");
#if DEBUG
            ti.Testing = true;
#endif
            if (OnlineRegModel.GetTransactionGateway() == "serviceu")
                ti = PaymentForm.CreateTransaction(DbUtil.Db, ti, amount);
            ConfirmDuePaidTransaction(ti, transactionId, sendmail: true);
            ViewBag.amtdue = PaymentForm.AmountDueTrans(DbUtil.Db, ti).ToString("C");
            SetHeaders(ti.OrgId ?? 0);
            return View("PayAmtDue/Confirm", ti);
        }

        [HttpGet]
        public ActionResult PayDueTest(string q)
        {
            if (!q.HasValue())
                return Message("unknown");
            var id = Util.Decrypt(q);
            var ed = DbUtil.Db.ExtraDatas.SingleOrDefault(e => e.Id == id.ToInt());
            if (ed == null)
                return Message("no outstanding transaction");
            return Content(ed.Data);
        }
    }
}
