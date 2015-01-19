﻿using System.Linq;
using System.Web.Mvc;
using CmsData;
using CmsWeb.Areas.Org.Models;
using CmsWeb.Code;
using CmsWeb.Models;
using UtilityExtensions;

namespace CmsWeb.Controllers
{
    [RouteArea("Dialog", AreaPrefix="NewMeeting"), Route("{action}/{id?}")]
    public class NewMeetingController : CmsStaffController
    {
        [HttpPost, Route("~/NewMeeting/{id:int}")]
        public ActionResult Index(int id, bool forMeeting)
        {
            var oi = new OrganizationModel() { Id = id };
            var m = new NewMeetingInfo()
            {
                MeetingDate = forMeeting ? oi.PrevMeetingDate : oi.NextMeetingDate,
                Schedule = new CodeInfo(0, forMeeting ? oi.SchedulesPrev() : oi.SchedulesNext()),
                AttendCredit = new CodeInfo(0, oi.AttendCreditList()),
            };
            return View(m);
        }
        [HttpPost]
        public ActionResult Create(NewMeetingInfo model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);
            var organization = DbUtil.Db.LoadOrganizationById(Util2.CurrentOrganization.Id);
            if (organization == null)
                return Content("error: no org");
            var mt = DbUtil.Db.Meetings.SingleOrDefault(m => m.MeetingDate == model.MeetingDate
                    && m.OrganizationId == organization.OrganizationId);

            if (mt != null)
                return Redirect("/Meeting/" + mt.MeetingId);

            mt = new Meeting
            {
                CreatedDate = Util.Now,
                CreatedBy = Util.UserId1,
                OrganizationId = organization.OrganizationId,
                GroupMeetingFlag = model.ByGroup,
                Location = organization.Location,
                MeetingDate = model.MeetingDate,
                AttendCreditId = model.AttendCredit.Value.ToInt()
            };
            DbUtil.Db.Meetings.InsertOnSubmit(mt);
            DbUtil.Db.SubmitChanges();
            DbUtil.LogActivity("Creating new meeting for {0}".Fmt(organization.OrganizationName));
            return Redirect("/Meeting/" + mt.MeetingId);
        }
    }
}