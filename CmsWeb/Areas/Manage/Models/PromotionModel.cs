﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Web;
using CmsData;
using UtilityExtensions;
using System.Web.Mvc;
using CMSPresenter;
using System.Collections;

namespace CMSWeb.Models
{
    public class PromotionModel
    {
        private int? _PromotionId;
        public int? PromotionId
        {
            get
            {
                if (_PromotionId != null)
                    return _PromotionId;
                _PromotionId = DbUtil.Db.UserPreference("PromotionId", "0").ToInt2();
                return _PromotionId;
            }
            set
            {
                _PromotionId = value;
                DbUtil.Db.SetUserPreference("PromotionId", value);
            }
        }

        private Promotion _Promotion;
        public Promotion Promotion
        {
            get
            {
                if (_Promotion == null)
                {
                    _Promotion = DbUtil.Db.Promotions.SingleOrDefault(p => p.Id == PromotionId);
                    if (_Promotion == null)
                        return new Promotion { FromDivId = 0 };
                }
                return _Promotion;
            }
        }
        private int? _ScheduleId;
        public int? ScheduleId
        {
            get
            {
                if (_ScheduleId != null)
                    return _ScheduleId;
                _ScheduleId = DbUtil.Db.UserPreference("ScheduleId", "0").ToInt2();
                return _ScheduleId;
            }
            set
            {
                _ScheduleId = value;
                DbUtil.Db.SetUserPreference("ScheduleId", value);
            }
        }

        public int TargetClassId { get; set; }

        private bool? _FilterUnassigned;
        public bool FilterUnassigned
        {
            get
            {
                if (_FilterUnassigned != null)
                    return _FilterUnassigned.Value;
                _FilterUnassigned = DbUtil.Db.UserPreference("FilterUnassigned", "false").ToBool();
                return _FilterUnassigned.Value;
            }
            set
            {
                _FilterUnassigned = value;
                DbUtil.Db.SetUserPreference("FilterUnassigned", value);
            }
        }

        private bool? _NormalMembersOnly;
        public bool NormalMembersOnly
        {
            get
            {
                if (_NormalMembersOnly != null)
                    return _NormalMembersOnly.Value;
                _NormalMembersOnly = DbUtil.Db.UserPreference("NormalMembersOnly", "false").ToBool();
                return _NormalMembersOnly.Value;
            }
            set
            {
                _NormalMembersOnly = value;
                DbUtil.Db.SetUserPreference("NormalMembersOnly", value);
            }
        }

        private int[] _Selected;
        public int[] selected
        {
            get
            {
                if (_Selected == null)
                    _Selected = new int[0];
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }

        private string _Sort = "Mixed";
        public string Sort
        {
            get { return _Sort; }
            set { _Sort = value; }
        }
        private string _Dir = "asc";
        public string Dir
        {
            get { return _Dir; }
            set { _Dir = value; }
        }

        public IEnumerable<PromoteInfo> FetchStudents()
        {
            var fromdiv = Promotion.FromDivId;
            var todiv = Promotion.ToDivId;

            var q = from om in DbUtil.Db.OrganizationMembers
                    where om.Organization.DivOrgs.Any(d => d.DivId == fromdiv)
                        && om.Organization.ScheduleId == ScheduleId
                    where (om.Pending ?? false) == false
                    where !NormalMembersOnly || om.MemberTypeId == (int)OrganizationMember.MemberTypeCode.Member
                    let pc = DbUtil.Db.OrganizationMembers.FirstOrDefault(op =>
                       op.Pending == true
                       && op.PeopleId == om.PeopleId
                       && op.Organization.DivOrgs.Any(dd => dd.DivId == todiv)
                       && op.Organization.ScheduleId == ScheduleId)
                    let pt = pc.Organization.OrganizationMembers.FirstOrDefault(om2 =>
                        om2.Pending == true
                        && om2.MemberTypeId == (int)OrganizationMember.MemberTypeCode.Teacher)
                    where !FilterUnassigned || pc == null
                    select new PromoteInfo
                    {
                        IsSelected = selected.Contains(om.PeopleId),
                        PeopleId = om.PeopleId,
                        Name = om.Person.Name,
                        Name2 = om.Person.Name2,
                        AttendPct = om.AttendPct,
                        BDay = om.Person.BirthDay,
                        BMon = om.Person.BirthMonth,
                        BYear = om.Person.BirthYear,
                        CurrClassId = om.OrganizationId,
                        CurrOrgName = om.Organization.OrganizationName,
                        CurrLeader = om.Organization.LeaderName,
                        CurrLoc = om.Organization.Location,
                        Gender = om.Person.GenderId == 1 ? "M" : "F",
                        PendingClassId = pc == null ? (int?)null : pc.OrganizationId,
                        PendingOrgName = pc == null ? "" : pc.Organization.OrganizationName,
                        PendingLeader = pc == null ? "" : (pt != null ? pt.Person.Name : pc.Organization.LeaderName),
                        PendingLoc = pc == null ? "" : pc.Organization.Location,
                        Hash = om.Person.HashNum.Value,
                    };
            if (Dir == "asc")
                switch (Sort)
                {
                    case "Mixed":
                        q = q.OrderBy(i => i.Hash);
                        break;
                    case "Name":
                        q = q.OrderBy(i => i.Name2);
                        break;
                    case "CurrentClass":
                        q = q.OrderBy(i => i.CurrOrgName);
                        break;
                    case "PendingClass":
                        q = q.OrderBy(i => i.PendingOrgName);
                        break;
                    case "Attendance":
                        q = q.OrderBy(i => i.AttendPct);
                        break;
                    case "Gender":
                        q = q.OrderBy(i => i.Gender);
                        break;
                    case "Birthday":
                        q = from i in q
                            orderby i.BYear, i.BMon, i.BDay
                            select i;
                        break;
                }
            else
                switch (Sort)
                {
                    case "Mixed":
                        q = q.OrderByDescending(i => i.Hash);
                        break;
                    case "Name":
                        q = q.OrderByDescending(i => i.Name2);
                        break;
                    case "CurrentClass":
                        q = q.OrderByDescending(i => i.CurrOrgName);
                        break;
                    case "PendingClass":
                        q = q.OrderByDescending(i => i.PendingOrgName);
                        break;
                    case "Attendance":
                        q = q.OrderByDescending(i => i.AttendPct);
                        break;
                    case "Gender":
                        q = q.OrderByDescending(i => i.Gender);
                        break;
                    case "Birthday":
                        q = from i in q
                            orderby i.BYear descending, i.BMon descending, i.BDay descending
                            select i;
                        break;
                }

            return q;
        }

        public void AssignPending()
        {
            var t = DbUtil.Db.Organizations.SingleOrDefault(o => o.OrganizationId == TargetClassId);
            var fromdiv = Promotion.FromDivId;
            var todiv = Promotion.ToDivId;

            foreach (var pid in selected)
            {
                var q = from om in DbUtil.Db.OrganizationMembers
                        where om.Pending == true
                        where om.PeopleId == pid
                        where om.Organization.DivOrgs.Any(dd => dd.DivId == todiv)
                        where om.Organization.ScheduleId == ScheduleId
                        select om;
                foreach (var pc in q)
                {
                    pc.Drop();
                    DbUtil.Db.SubmitChanges();
                }
                OrganizationMember.InsertOrgMembers(
                    t.OrganizationId,
                    pid,
                    (int)OrganizationMember.MemberTypeCode.Member,
                    Util.Now,
                    null,
                    true);
            }
        }
        public IEnumerable Export()
        {
            var fromdiv = Promotion.FromDivId;
            var todiv = Promotion.ToDivId;
            var q = from om in DbUtil.Db.OrganizationMembers
                    where om.Organization.DivOrgs.Any(d => d.DivId == fromdiv)
                    where (om.Pending ?? false) == false
                    where om.MemberTypeId == (int)OrganizationMember.MemberTypeCode.Member
                    let pc = DbUtil.Db.OrganizationMembers.FirstOrDefault(op =>
                       op.Pending == true
                       && op.PeopleId == om.PeopleId
                       && op.Organization.DivOrgs.Any(dd => dd.DivId == todiv))
                    let tm = pc.Organization.SchedTime.Value
                    let pt = pc.Organization.OrganizationMembers.FirstOrDefault(om2 => 
                        om2.Pending == true 
                        && om2.MemberTypeId == (int)OrganizationMember.MemberTypeCode.Teacher)
                    where pc != null
                    select new
                    {
                        PeopleId = om.PeopleId,
                        Title = om.Person.TitleCode,
                        FirstName = om.Person.PreferredName,
                        LastName = om.Person.LastName,
                        Address = om.Person.PrimaryAddress,
                        Address2 = om.Person.PrimaryAddress2,
                        City = om.Person.PrimaryCity,
                        State = om.Person.PrimaryState,
                        Zip = om.Person.PrimaryZip.FmtZip(),
                        Email = om.Person.EmailAddress,
                        MemberType = om.MemberType.Description,
                        Parent = om.Person.Family.HeadOfHousehold.Name,
                        Parent2 = om.Person.Family.HeadOfHouseholdSpouse.Name,
                        Location = string.IsNullOrEmpty(pc.Organization.PendingLoc) ? pc.Organization.Location : pc.Organization.PendingLoc,
                        Leader = pt != null ? pt.Person.Name : pc.Organization.LeaderName,
                        OrgName = pc.Organization.OrganizationName,
                        Schedule = tm.Hour + ":" + tm.Minute.ToString().PadLeft(2, '0'),
                    };
            return q;
        }
        public IEnumerable<SelectListItem> Promotions()
        {
            var q = from p in DbUtil.Db.Promotions
                    orderby p.Sort, p.Description
                    select new SelectListItem
                    {
                        Text = p.Description,
                        Value = p.Id.ToString(),
                    };
            var list = q.ToList();
            list.Insert(0, new SelectListItem { Text = "(Select Promotion)", Value = "0", Selected = true });
            return list;
        }
        public IEnumerable<SelectListItem> Schedules()
        {
            var q = from o in DbUtil.Db.Organizations
                    where o.DivOrgs.Any(dd => dd.DivId == Promotion.FromDivId)
                    group o by new { o.ScheduleId, o.MeetingTime } into g
                    orderby g.Key.ScheduleId
                    select new SelectListItem
                    {
                        Value = g.Key.ScheduleId.ToString(),
                        Text = "{0:dddd h:mm tt}".Fmt(g.Key.MeetingTime)
                    };

            var list = q.ToList();
            if (list.Count == 0)
                list.Insert(0, new SelectListItem { Text = "(Select Promotion First)", Value = "0", Selected = true });
            else
                list.Insert(0, new SelectListItem { Text = "(Select Schedule)", Value = "0", Selected = true });
            return list;
        }
        public IEnumerable<SelectListItem> TargetClasses()
        {
            var todiv = Promotion.ToDivId;
            var q = from o in DbUtil.Db.Organizations
                    where o.DivOrgs.Any(dd => dd.DivId == todiv)
                    where o.ScheduleId == ScheduleId
                    where o.OrganizationStatusId == (int)CmsData.Organization.OrgStatusCode.Active
                    orderby o.OrganizationName
                    let pt = o.OrganizationMembers.FirstOrDefault(om2 =>
                        om2.Pending == true
                        && om2.MemberTypeId == (int)OrganizationMember.MemberTypeCode.Teacher)
                    select new SelectListItem
                    {
                        Text = CmsData.Organization.FormatOrgName(o.OrganizationName, pt != null ? pt.Person.Name : o.LeaderName, o.Location),
                        Value = o.OrganizationId.ToString(),
                    };
            return q;
        }
    }
}