using System.Linq;
using CmsData;
using CmsData.Registration;
using CmsData.Codes;
using UtilityExtensions;

namespace CmsWeb.Models
{
    public class ManagePledgesModel
    {
        public int pid { get; set; }
        public int orgid { get; set; }
        private Person _Person;
        public Person person
        {
            get
            {
                if (_Person == null)
                    _Person = DbUtil.Db.LoadPersonById(pid);
                return _Person;
            }
        }
        private Organization _organization;
        public Organization Organization
        {
            get
            {
                if (_organization == null)
                    _organization = DbUtil.Db.Organizations.Single(d => d.OrganizationId == orgid);
                return _organization;
            }
        }
        public decimal? pledge { get; set; }

        public ManagePledgesModel()
        {

        }
        public class PledgeInfo
        {
            public decimal Pledged { get; set; }
            public decimal Given { get; set; }
        }

        private Settings setting;
        public Settings Setting
        {
            get
            {
                return setting ?? (setting = new Settings(Organization.RegSetting, DbUtil.Db, orgid));
            }
        }
        private bool? usebootstrap;
        public bool UseBootstrap
        {
            get
            {
                if (!usebootstrap.HasValue)
                    usebootstrap = Setting.UseBootstrap;
                return usebootstrap.Value;
            }
        }
        public PledgeInfo GetPledgeInfo()
        {
            var RRTypes = new int[] { 6, 7 };
            var q = from c in DbUtil.Db.Contributions
                    where c.FundId == Setting.DonationFundId
                    where c.PeopleId == pid
                    where !RRTypes.Contains(c.ContributionTypeId)
                    group c by pid into g
                    select new PledgeInfo
                    {
                        Pledged = g.Where(c => c.ContributionTypeId == ContributionTypeCode.Pledge).Sum(c => c.ContributionAmount) ?? 0,
                        Given = g.Where(c => c.ContributionTypeId != ContributionTypeCode.Pledge).Sum(c => c.ContributionAmount) ?? 0,
                    };
            return q.SingleOrDefault() ?? new PledgeInfo { Given = 0m, Pledged = 0m };
        }
        public ManagePledgesModel(int pid, int orgid)
        {
            this.pid = pid;
            this.orgid = orgid;
        }
	    public void Confirm()
	    {
	        var staff = DbUtil.Db.StaffPeopleForOrg(orgid);

	        var desc = "{0}; {1}; {2}, {3} {4}".Fmt(
	            person.Name,
	            person.PrimaryAddress,
	            person.PrimaryCity,
	            person.PrimaryState,
	            person.PrimaryZip);

	        person.PostUnattendedContribution(DbUtil.Db,
	            pledge ?? 0,
	            Setting.DonationFundId,
	            desc, pledge: true);

	        var pi = GetPledgeInfo();
	        var body = Setting.Body;
	        body = body.Replace("{amt}", pi.Pledged.ToString("N2"), ignoreCase: true)
	            .Replace("{org}", Organization.OrganizationName, ignoreCase: true)
	            .Replace("{first}", person.PreferredName, ignoreCase: true);
	        DbUtil.Db.EmailRedacted(staff[0].FromEmail, person, Setting.Subject, body);

	        DbUtil.Db.Email(person.FromEmail, staff, "Online Pledge",
	            @"{0} made a pledge to {1}".Fmt(person.Name, Organization.OrganizationName));
	    }
    }
}
