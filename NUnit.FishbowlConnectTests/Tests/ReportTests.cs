using FishbowlConnect;
using FishbowlConnect.Exceptions;
using FishbowlConnect.Json.APIObjects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests.Tests
{

    [TestFixture]
    public class ReportTests
    {
        const string GoodServerAddress = "192.168.150.4";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";

        const string DatabaseAddress = "192.168.150.2";
        const int DatabasePort = 2361;
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        const string DatabaseName = "BRITEIDEASUPDATE";


        public async Task<int> PrintReportToPrinter(string ReportName, string PrinterName, int NumberOfCopies, List<ReportParam> ReportParams = null)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword, 20000);

            using (FishbowlSession session = new FishbowlSession(config))
            {
                int reportId = await session.GetReportIdFromName(ReportName);
                List<Printer> printers = await session.GetServerPrinterList();
                if (printers == null || printers.Count == 0)
                {
                    throw new FishbowlException("Printers not found");
                }

                int? printerId = printers.Where(p => p.Name == PrinterName).FirstOrDefault()?.Id;

                if (!printerId.HasValue)
                {
                    throw new FishbowlException(string.Format("Printer {0} not found", PrinterName));
                }

                return await session.PrintReportToPrinter(reportId, printerId.Value, ReportParams, NumberOfCopies);

            }
        }

        public async Task<int> PrintReportToPrinter(int reportId, int PrinterId, int NumberOfCopies, List<ReportParam> ReportParams = null)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword, 20000);

            using (FishbowlSession session = new FishbowlSession(config))
            {
                List<Printer> printers = await session.GetServerPrinterList();
                if (printers == null || printers.Count == 0)
                {
                    throw new FishbowlException("Printers not found");
                }

                int? printerId = printers.Where(p => p.Id == PrinterId).FirstOrDefault()?.Id;

                if (!printerId.HasValue)
                {
                    throw new FishbowlException(string.Format("Printer {0} not found", PrinterId));
                }

                return await session.PrintReportToPrinter(reportId, printerId.Value, ReportParams, NumberOfCopies);

            }
        }


        [TestCase("New Receipt 3in", "CutePDF Writer", 1)]
        public async Task PrintStandardReportToPrinterTest(string ReportName, string PrinterName, int NumberOfCopies)
        {
            List<ReportParam> reportParams = new List<ReportParam>();
            reportParams.Add(new ReportParam { Name = "soNum", Value = "71229" });

            int jobId = await this.PrintReportToPrinter(ReportName, PrinterName, NumberOfCopies, reportParams);
            Assert.That(jobId > 0);
        }


        [TestCase("Product Label Zebra 1.25 x 2.25", "CutePDF Writer", 1)]
        public async Task PrintLabelReportToPrinterTest(string ReportName, string PrinterName, int NumberOfCopies)
        {
            List<ReportParam> reportParams = new List<ReportParam>();
            reportParams.Add(new ReportParam { Name = "productNum", Value = "100GCL" });
            int jobId = await this.PrintReportToPrinter(ReportName, PrinterName, NumberOfCopies, reportParams);
            Assert.That(jobId > 0);
        }

        //tests
        //Print report where id doesnt exist throws correct error response
        //Print to printer where id is invalid throws correct response
        [TestCase(999)]
        public async Task PrintToInvalidPrinterIDThrowsCorrectError(int invalidId)
        {
            try
            {
                int jobId = await PrintReportToPrinter(3, invalidId, 1);
            }
            catch (FishbowlException e)
            {
                Assert.IsInstanceOf(typeof(FishbowlException), e);
                Assert.That(e.Message.Contains("Printer " + invalidId + " not found"));
            }
        }

        [TestCase(9999)]
        public async Task RequestInvalidReportIdThrowsCorrectError(int reportId)
        {
            try
            {
                int jobId = await PrintReportToPrinter(reportId, 69309791, 1);
            }
            catch (FishbowlException e)
            {
                Assert.IsInstanceOf(typeof(FishbowlException), e);
                Assert.That(e.Message.Contains("Report id " + reportId + " not found"));
            }

            
        }
          




        [Test]
        public async Task GetListOfServerPrinters()
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword,20000);

            using (FishbowlSession session = new FishbowlSession(config))
            {

                List<Printer> printers = await session.GetServerPrinterList();

                Assert.NotNull(printers);
                Assert.That(printers.Count > 0);

            }
        }
    }
}
