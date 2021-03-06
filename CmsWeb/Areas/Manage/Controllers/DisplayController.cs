/* Author: David Carroll
 * Copyright (c) 2008, 2009 Bellevue Baptist Church 
 * Licensed under the GNU General Public License (GPL v2)
 * you may not use this code except in compliance with the License.
 * You may obtain a copy of the License at http://bvcms.codeplex.com/license 
 */
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Windows.Forms;
using CmsData;
using CmsData.Codes;
using Dapper;
using UtilityExtensions;
using CmsWeb.Models;
// Used for pulling image from service
using System.Net;
// Used for HTML Image Capture
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Web;
using CmsWeb.Code;
using Elmah;
using Encoder = System.Drawing.Imaging.Encoder;

namespace CmsWeb.Areas.Manage.Controllers
{
    [Authorize(Roles = "Admin,Design")]
    [ValidateInput(false)]
    [RouteArea("Manage", AreaPrefix = "Display"), Route("{action}/{id?}")]
    public class DisplayController : CmsStaffController
    {
        [Route("~/Display")]
        [Route("~/Manage/Display")]
        public ActionResult Index()
        {
            return View(new ContentModel());
        }

        public ActionResult ContentView(int id)
        {
            var content = DbUtil.ContentFromID(id);
            return View(content);
        }

        public ActionResult ContentEdit(int? id, bool? snippet)
        {
            if (!id.HasValue)
                throw new HttpException(404, "No ID found.");
            var content = DbUtil.ContentFromID(id.Value);
            if (snippet == true)
            {
                content.Snippet = true;
                DbUtil.Db.SubmitChanges();
            }
            return RedirectEdit(content);
        }

        [HttpPost]
        public ActionResult ContentCreate(int newType, string newName, int? newRole)
        {
            var content = new Content();
            content.Name = newName;
            content.TypeID = newType;
            content.RoleID = newRole ?? 0;
            content.Title = content.Body = "";
            content.DateCreated = DateTime.Now;

            DbUtil.Db.Contents.InsertOnSubmit(content);
            DbUtil.Db.SubmitChanges();

            return RedirectEdit(content);
        }

        [HttpPost]
        public ActionResult ContentUpdate(int id, string name, string title, string body, bool? snippet, int? roleid, string stayaftersave = null)
        {
            var content = DbUtil.ContentFromID(id);
            content.Name = name;
            content.Title = title;
            content.Body = body;
            content.RoleID = roleid ?? 0;
            content.Snippet = snippet;

            if (content.TypeID == ContentTypeCode.TypeEmailTemplate)
            {
                try
                {
                    var captureWebPageBytes = CaptureWebPageBytes(body, 100, 150);
                    var ii = ImageData.Image.UpdateImageFromBits(content.ThumbID, captureWebPageBytes);
                    if (ii == null)
                        content.ThumbID = ImageData.Image.NewImageFromBits(captureWebPageBytes).Id;
                    content.DateCreated = DateTime.Now;
                }
                catch (Exception ex)
                {
					var errorLog = ErrorLog.GetDefault(null);
					errorLog.Log(new Error(ex));
                }
            }
            DbUtil.Db.SubmitChanges();

            if (string.Compare(content.Name, "StandardExtraValues2", StringComparison.OrdinalIgnoreCase) == 0)
            {
                try
                {
                    CmsData.ExtraValue.Views.GetStandardExtraValues(DbUtil.Db, "People");
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.InnerException is System.Xml.XmlException)
                        return Content(Util.EndShowMessage(ex.InnerException.Message, "javascript: history.go(-1)", "Go Back to Repair"));
                }
            }

            if (stayaftersave == "true")
                return RedirectEdit(content);

            return RedirectToAction("Index");
        }

        public ActionResult ContentDelete(int id)
        {
            var content = DbUtil.ContentFromID(id);
            DbUtil.Db.Contents.DeleteOnSubmit(content);
            DbUtil.Db.SubmitChanges();
            return RedirectToAction("Index", "Display");
        }

        public ActionResult ContentDeleteDrafts(string[] draftID)
        {
            string deleteList = String.Join(",", draftID);
            DbUtil.Db.ExecuteCommand("DELETE FROM dbo.Content WHERE Id IN(" + deleteList + ")", "");
            return RedirectToAction("Index", "Display");
        }

        public ActionResult RedirectEdit(Content cContent)
        {
            switch (cContent.TypeID) // 0 = HTML, 1 = Text, 2 = eMail Template
            {
                case ContentTypeCode.TypeHtml:
                    return View("EditHTML", cContent);

                case ContentTypeCode.TypeText:
                    return View("EditText", cContent);

                case ContentTypeCode.TypeSqlScript:
                    return View("EditSqlScript", cContent);

                case ContentTypeCode.TypePythonScript:
                    return View("EditPythonScript", cContent);

                case ContentTypeCode.TypeEmailTemplate:
                case ContentTypeCode.TypeSavedDraft:
                    return View("EditTemplate", cContent);
            }

            return View("Index");
        }

        public ActionResult OrgContent(int id, string what, bool? div)
        {
            var org = DbUtil.Db.LoadOrganizationById(id);
            if (div == true && org.Division == null)
                return Content("no main division");

            switch (what)
            {
                case "message":
                    if (div == true)
                    {
                        ViewData["html"] = org.Division.EmailMessage;
                        ViewData["title"] = org.Division.EmailSubject;
                    }
                    break;
                case "instructions":
                    if (div == true)
                        ViewData["html"] = org.Division.Instructions;
                    ViewData["title"] = "Instructions";
                    break;
                case "terms":
                    if (div == true)
                        ViewData["html"] = org.Division.Terms;
                    ViewData["title"] = "Terms";
                    break;
            }
            ViewData["id"] = id;
            return View();
        }

        [HttpPost]
        public ActionResult RunSqlScript(string body, string parameter)
        {
            var cn = new SqlConnection(Util.ConnectionStringReadOnly);
            cn.Open();
            var script = "DECLARE @p1 VARCHAR(100) = '{0}'\n{1}\n".Fmt(parameter, body);
            var rd = cn.ExecuteReader(script);
            return new GridResult(rd);
        }
        [HttpPost]
        public ActionResult SavePythonScript(string name, string body)
        {
            var content = DbUtil.Db.Content(name, "", ContentTypeCode.TypePythonScript);
            content.Body = body;
            DbUtil.Db.SubmitChanges();
            return new EmptyResult();
        }
        [HttpPost]
        public ActionResult UpdateOrgContent(int id, bool? div, string what, string title, string html)
        {
            var org = DbUtil.Db.LoadOrganizationById(id);

            switch (what)
            {
                case "message":
                    if (div == true)
                    {
                        org.Division.EmailMessage = html;
                        org.Division.EmailSubject = title;
                    }
                    break;
                case "instructions":
                    if (div == true)
                        org.Division.Instructions = html;
                    break;
                case "terms":
                    if (div == true)
                        org.Division.Terms = html;
                    break;
            }
            DbUtil.Db.SubmitChanges();
            return Redirect("/Organization/" + id);
        }

        public static byte[] CaptureWebPageBytes(string body, int width, int height)
        {
            bool bDone = false;
            byte[] data = null;
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;

            //sta thread to allow intiate WebBrowser
            var staThread = new Thread(delegate()
            {
                data = CaptureWebPageBytesP(body, width, height);
                bDone = true;
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();

            while (!bDone)
            {
                endDate = DateTime.Now;
                TimeSpan tsp = endDate.Subtract(startDate);

                System.Windows.Forms.Application.DoEvents();
                if (tsp.Seconds > 50)
                {
                    break;
                }
            }
            staThread.Abort();
            return data;
        }

        static byte[] CaptureWebPageBytesP(string body, int width, int height)
        {
            byte[] data;

            using (WebBrowser web = new WebBrowser())
            {
                web.ScrollBarsEnabled = false; // no scrollbars
                web.ScriptErrorsSuppressed = true; // no errors

                web.DocumentText = body;
                while (web.ReadyState != System.Windows.Forms.WebBrowserReadyState.Complete)
                    System.Windows.Forms.Application.DoEvents();

                web.Width = web.Document.Body.ScrollRectangle.Width;
                web.Height = web.Document.Body.ScrollRectangle.Height;

                // a bitmap that we will draw to
                using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(web.Width, web.Height))
                {
                    // draw the web browser to the bitmap
                    web.DrawToBitmap(bmp, new Rectangle(web.Location.X, web.Location.Y, web.Width, web.Height));
                    // draw the web browser to the bitmap

                    GraphicsUnit units = GraphicsUnit.Pixel;
                    RectangleF destRect = new RectangleF(0F, 0F, width, height);
                    RectangleF srcRect = new RectangleF(0, 0, web.Width, web.Width * 1.5F);

                    Bitmap b = new Bitmap(width, height);
                    Graphics g = Graphics.FromImage((Image)b);
                    g.Clear(Color.White);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    g.DrawImage(bmp, destRect, srcRect, units);
                    g.Dispose();

                    using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                    {
                        EncoderParameter qualityParam = null;
                        EncoderParameters encoderParams = null;
                        try
                        {
                            ImageCodecInfo imageCodec = null;
                            imageCodec = GetEncoderInfo("image/jpeg");

                            qualityParam = new EncoderParameter(Encoder.Quality, 100L);

                            encoderParams = new EncoderParameters(1);
                            encoderParams.Param[0] = qualityParam;
                            b.Save(stream, imageCodec, encoderParams);
                        }
                        catch (Exception)
                        {
                            throw new Exception();
                        }
                        finally
                        {
                            if (encoderParams != null) encoderParams.Dispose();
                            if (qualityParam != null) qualityParam.Dispose();
                        }
                        b.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        stream.Position = 0;
                        data = new byte[stream.Length];
                        stream.Read(data, 0, (int)stream.Length);
                    }
                }
            }
            return data;
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
    public class GridResult : ActionResult
    {
        private readonly IDataReader rd;
        public GridResult(IDataReader rd)
        {
            this.rd = rd;
        }

        public static HtmlTable HtmlTable(IDataReader rd)
        {
            var t = new HtmlTable();
            t.Attributes.Add("class", "table not-wide");
            var h = new HtmlTableRow();
            for (var i = 0; i < rd.FieldCount; i++)
                h.Cells.Add(new HtmlTableCell {InnerText = rd.GetName(i)});
            t.Rows.Add(h);
            while (rd.Read())
            {
                var r = new HtmlTableRow();
                for (var i = 0; i < rd.FieldCount; i++)
                    r.Cells.Add(new HtmlTableCell() { InnerText = rd[i].ToString() });
                t.Rows.Add(r);
            }
            var tc = new HtmlTableCell
            {
                ColSpan = rd.FieldCount, 
                InnerText = "Count = {0} rows".Fmt(t.Rows.Count - 1)
            };
            var tr = new HtmlTableRow();
            tr.Cells.Add(tc);
            t.Rows.Add(tr);
            return t;
        }
        public override void ExecuteResult(ControllerContext context)
        {
            var t = HtmlTable(rd);
            t.RenderControl(new HtmlTextWriter(context.HttpContext.Response.Output));
        }

        public static string Table(IDataReader rd)
        {
            var t = HtmlTable(rd);
            var sb = new StringBuilder();
            t.RenderControl(new HtmlTextWriter(new StringWriter(sb)));
            return sb.ToString();
        }
    }

}