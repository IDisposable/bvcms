using System;
using System.Collections.Generic;
using System.Linq;
using CmsData;
using UtilityExtensions;

namespace CmsWeb.Models
{
    public partial class OnlineRegModel
    {
        public static IQueryable<Organization> UserSelectClasses(Organization masterorg)
        {
            if (!masterorg.OrgPickList.HasValue())
                return DbUtil.Db.Organizations.Where(oo => false);
            var cklist = masterorg.OrgPickList.Split(',').Select(oo => oo.ToInt()).ToList();

            var q = from o in DbUtil.Db.Organizations
                    where cklist.Contains(o.OrganizationId)
                    select o;
            return q;
        }
        public static List<Organization> OrderedClasses(Organization masterorg)
        {
            if (masterorg == null)
                throw new Exception("masterorg is null in OrderedClasses");
            var cklist = masterorg.OrgPickList.Split(',').Select(oo => oo.ToInt()).ToList();
            var list = UserSelectClasses(masterorg).ToList();
            var d = new Dictionary<int, int>();
            var n = 0;
            foreach (var i in cklist)
                d.Add(n++, i);
            list = (from o in list
                    join i in d on o.OrganizationId equals i.Value into j
                    from i in j
                    orderby i.Key
                    select o).ToList();
            return list;
        }

        public class ClassInfo
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public bool selected { get; set; }
            public bool filled { get; set; }
        }
        public IEnumerable<ClassInfo> Classes(int? cid)
        {
            return Classes(masterorg, cid ?? 0);
        }
        public static List<ClassInfo> Classes(Organization masterorg, int id)
        {
            var q = from o in OrderedClasses(masterorg)
                    let hasroom = (o.ClassFilled ?? false) == false && ((o.Limit ?? 0) == 0 || o.Limit > o.RegLimitCount(DbUtil.Db))
                    where o.RegistrationTypeId > 0
                    select new ClassInfo
                    {
                        Id = o.OrganizationId,
                        Text = ClassName(o),
                        selected = o.OrganizationId == id,
                        filled = !hasroom
                    };
            var list = q.ToList();
            if (list.Count == 1)
                return list;
            list.Insert(0, new ClassInfo { Text = "Registration Options", Id = 0});
            return list;
        }
        private static string ClassName(CmsData.Organization o)
        {
            var lead = o.LeaderName;
            if (lead.HasValue())
                lead = ": " + lead;
            var loc = o.Location;
            if (loc.HasValue())
                loc = " ({0})".Fmt(loc);
            var dt1 = o.FirstMeetingDate;
            var dt2 = o.LastMeetingDate;
            var dt = "";
            if (dt1.HasValue && dt2.HasValue)
                dt = ", {0:MMM d}-{1:MMM d}".Fmt(dt1, dt2);
            else if (dt1.HasValue)
                dt = ", {0:MMM d}".Fmt(dt1);

            return o.OrganizationName + lead + dt + loc;
        }
    }
}
