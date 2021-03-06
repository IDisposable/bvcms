using System;
using System.Web.Mvc;
using UtilityExtensions;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace CmsWeb.Models
{
    public class ContributionsExcelResult
    {
        public DateTime Dt1 { get; set; }
        public DateTime Dt2 { get; set; }
        public int fundid { get; set; }
    	public int campusid { get; set; }
    	public int Online { get; set; }
        public string TaxDedNonTax { get; set; }
        public bool IncUnclosedBundles { get; set; }

        public EpplusResult ToExcel(string type)
        {
            bool? nontaxdeductible = null; // both
            switch (TaxDedNonTax)
            {
                case "TaxDed":
                    nontaxdeductible = false;
                    break;
                case "NonTaxDed":
                    nontaxdeductible = true;
                    break;
            }
            switch (type)
            {
                case "donorfundtotals":
    				return ExportPeople.ExcelDonorFundTotals(Dt1, Dt2, fundid, campusid, false, nontaxdeductible, IncUnclosedBundles)
                        .ToExcel("DonorFundTotals.xlsx");
                case "donortotals":
                    return ExportPeople.ExcelDonorTotals(Dt1, Dt2, campusid, false, nontaxdeductible, IncUnclosedBundles)
                        .ToExcel("DonorTotals.xlsx");
                case "donordetails":
                    return ExportPeople.DonorDetails(Dt1, Dt2, fundid, campusid, false, nontaxdeductible, IncUnclosedBundles)
                        .ToExcel("DonorDetails.xlsx");
            }
            return null;
        }
    }
}