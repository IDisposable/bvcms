using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using CmsWeb.Areas.Finance.Models.Report;
using CmsData;
using CmsData.Classes.QuickBooks;
using CmsWeb.Code;
using Dapper;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using UtilityExtensions;
using CmsWeb.Models;
using TableStyles = OfficeOpenXml.Table.TableStyles;

namespace CmsWeb.Areas.Finance.Controllers
{
    [Authorize(Roles = "Finance")]
    [RouteArea("Finance", AreaPrefix = "FinanceReports"), Route("{action}/{id?}")]
    public class FinanceReportsController : CmsStaffController
    {
        public ActionResult ContributionYears(int id)
        {
            var m = new ContributionModel(id);
            return View(m);
        }

        public ActionResult ContributionStatement(int id, DateTime fromDate, DateTime toDate, int typ)
        {
            DbUtil.LogActivity("Contribution Statement for ({0})".Fmt(id));
            return new ContributionStatementResult
            {
                PeopleId = id,
                FromDate = fromDate,
                ToDate = toDate,
                typ = typ
            };
        }

        private DynamicParameters DonorTotalSummaryParameters(DonorTotalSummaryOptionsModel m, bool useMedianMin = false)
        {
            var p = new DynamicParameters();
            p.Add("@enddt", m.StartDate);
            p.Add("@years", m.NumberOfYears);
            if(useMedianMin)
                p.Add("@medianMin", m.MinimumMedianTotal);
            p.Add("@fund", m.Fund.Value.ToInt());
            p.Add("@campus", m.Campus.Value.ToInt());
            return p;
        }
        [HttpGet]
        public EpplusResult DonorTotalSummary(DonorTotalSummaryOptionsModel m)
        {
            var ep = new ExcelPackage();
            var cn = new SqlConnection(Util.ConnectionString);

            var rd = cn.ExecuteReader("dbo.DonorTotalSummary", DonorTotalSummaryParameters(m, useMedianMin: true), commandType: CommandType.StoredProcedure, commandTimeout: 1200);
            ep.AddSheet(rd, "MemberNon");

            rd = cn.ExecuteReader("dbo.DonorTotalSummaryBySize", DonorTotalSummaryParameters(m), commandType: CommandType.StoredProcedure, commandTimeout: 1200);
            ep.AddSheet(rd, "BySize");

            rd = cn.ExecuteReader("dbo.DonorTotalSummaryByAge", DonorTotalSummaryParameters(m), commandType: CommandType.StoredProcedure, commandTimeout: 1200);
            ep.AddSheet(rd, "ByAge");

            return new EpplusResult(ep, "DonorTotalSummary.xlsx");
        }
        [HttpGet, Route("~/PledgeFulfillment2/{fundid1:int}/{fundid2:int}")]
        public EpplusResult PledgeFulfillment2(int fundid1, int fundid2)
        {
            var ep = new ExcelPackage();
            var cn = new SqlConnection(Util.ConnectionString);
            cn.Open();

            var rd = cn.ExecuteReader("dbo.PledgeFulfillment2", new { fundid1, fundid2, }, 
                commandTimeout: 1200, commandType: CommandType.StoredProcedure);
            ep.AddSheet(rd, "Pledges");
            return new EpplusResult(ep, "PledgeFulfillment2.xlsx");
        }

        [HttpGet]
        public ActionResult DonorTotalSummaryOptions()
        {
            var m = new DonorTotalSummaryOptionsModel
            {
                StartDate = DateTime.Today,
                NumberOfYears = 5,
                MinimumMedianTotal = 100,
                Campus = new CodeInfo("Campus0"),
                Fund = new CodeInfo("Fund"),
            };
            return View(m);
        }
        [HttpGet]
        public ActionResult DonorTotalsByRange()
        {
            var m = new TotalsByFundModel();
            return View(m);
        }

        [HttpPost]
        public ActionResult DonorTotalsByRangeResults(TotalsByFundModel m)
        {
            return View(m);
        }

        [HttpGet]
        public ActionResult TotalsByFund()
        {
            var m = new TotalsByFundModel();
            return View(m);
        }

        [HttpPost]
        public ActionResult TotalsByFundResults(TotalsByFundModel m)
        {
            if (m.IncludeBundleType)
                return View("TotalsByFundResults2", m);
            return View(m);
        }

        public ActionResult PledgeReport()
        {
            var fd = DateTime.Parse("1/1/1900");
            var td = DateTime.Parse("1/1/2099");
            var q = from r in DbUtil.Db.PledgeReport(fd, td, 0)
                    orderby r.FundId descending 
                    select r;
            return View(q);
        }

        public ActionResult ManagedGiving()
        {
            var q = from rg in DbUtil.Db.ManagedGivings.ToList()
                    orderby rg.NextDate
                    select rg;
            return View(q);
        }

        [HttpGet]
        public ActionResult ManageGiving2(int id)
        {
            var m = new ManageGivingModel(id);
            m.testing = true;
            var body = ViewExtensions2.RenderPartialViewToString(this, "ManageGiving2", m);
            return Content(body);
        }

        [HttpPost]
        public ActionResult ToQuickBooks(TotalsByFundModel m)
        {
            List<int> lFunds = new List<int>();
            List<QBJournalEntryLine> qbjel = new List<QBJournalEntryLine>();

            var entries = m.TotalsByFund();

            QuickBooksHelper qbh = new QuickBooksHelper();

            foreach (var item in entries)
            {
                if (item.QBSynced > 0) continue;

                var accts = (from e in DbUtil.Db.ContributionFunds
                             where e.FundId == item.FundId
                             select e).Single();

                if (accts.QBAssetAccount > 0 && accts.QBIncomeAccount > 0)
                {
                    QBJournalEntryLine jelCredit = new QBJournalEntryLine();

                    jelCredit.sDescrition = item.FundName;
                    jelCredit.dAmount = item.Total ?? 0;
                    jelCredit.sAccountID = accts.QBIncomeAccount.ToString();
                    jelCredit.bCredit = true;

                    QBJournalEntryLine jelDebit = new QBJournalEntryLine(jelCredit);

                    jelDebit.sAccountID = accts.QBAssetAccount.ToString();
                    jelDebit.bCredit = false;

                    qbjel.Add(jelCredit);
                    qbjel.Add(jelDebit);

                    lFunds.Add(item.FundId ?? 0);
                }
            }

            int iJournalID = qbh.CommitJournalEntries("Bundle from BVCMS", qbjel);

            if (iJournalID > 0)
            {
                string sStart = m.Dt1.Value.ToString("u");
                string sEnd = m.Dt2.Value.AddHours(23).AddMinutes(59).AddSeconds(59).ToString("u");

                sStart = sStart.Substring(0, sStart.Length - 1);
                sEnd = sEnd.Substring(0, sEnd.Length - 1);

                string sFundList = string.Join(",", lFunds.ToArray());
                string sUpdate = "UPDATE dbo.Contribution SET QBSyncID = " + iJournalID + " WHERE FundId IN (" +
                                 sFundList + ") AND ContributionDate BETWEEN '" + sStart + "' and '" + sEnd + "'";

                DbUtil.Db.ExecuteCommand(sUpdate);
            }

            return View("TotalsByFund", m);
        }

        public ActionResult PledgeFulfillments(int id)
        {
            var q = DbUtil.Db.PledgeFulfillment(id)
                .OrderByDescending(vv => vv.PledgeAmt)
                .ThenByDescending(vv => vv.TotalGiven).ToList();
            var count = q.Count;

            if(count == 0)
                return Message("No Pledges to Report");

            var cols = DbUtil.Db.Mapping.MappingSource.GetModel(typeof(CMSDataContext))
                .GetMetaType(typeof(CmsData.View.PledgeFulfillment)).DataMembers;

            var ep = new ExcelPackage();
            var ws = ep.Workbook.Worksheets.Add("Sheet1");

            ws.Cells["A2"].LoadFromCollection(q);

            var range = ws.Cells[1, 1, count + 1, cols.Count];
            var table = ws.Tables.Add(range, "Pledges");
            table.ShowTotal = true;
            table.ShowFilter = false;
            table.TableStyle = TableStyles.Light9;

            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            for (var i = 0; i < cols.Count; i++)
            {
                var col = i + 1;
                var name = cols[i].Name;
                table.Columns[i].Name = name;
                var colrange = ws.Cells[1, col, count + 2, col];
                switch (name)
                {
                    case "First":
                        table.Columns[i].TotalsRowLabel = "Total";
                        break;
                    case "Last":
                        table.Columns[i].TotalsRowFormula = @"CONCATENATE(""Count: "", SUBTOTAL(103,[Last]))";
                        break;
                    case "PledgeAmt":
                    case "TotalGiven":
                    case "Balance":
                        table.Columns[i].TotalsRowFormula = "SUBTOTAL(109,[{0}])".Fmt(name);
                        colrange.Style.Numberformat.Format = "#,##0.00;(#,##0.00)";
                        colrange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        ws.Column(col).Width = 12;
                        break;
                    case "PledgeDate":
                    case "LastDate":
                        colrange.Style.Numberformat.Format = "mm-dd-yy";
                        colrange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        ws.Column(col).Width = 12;
                        break;
                    case "Zip":
                    case "CreditGiverId":
                    case "SpouseId":
                    case "FamilyId":
                        colrange.Style.Numberformat.Format = "@";
                        colrange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        break;
                }
            }

            return new EpplusResult(ep, "PledgeFulfillment - {0}.xlsx".Fmt(id));
        }
    }
}
