using FishbowlConnect.Exceptions;
using FishbowlConnect.Json;
using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.CsvClassMaps;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FishbowlConnect
{
    public partial class FishbowlSession
    {
        /// <summary>
        /// Prints any sepcified Fishbowl report to the specified printer. The printer must be found on the fishbowl server machine
        /// </summary>
        /// <param name="ReportId">Id of the Fishbowl report</param>
        /// <param name="PrinterId">Id of the printNode printer to print to</param>

        /// <param name="ParameterList">List of parameters and values. Names must match the actual parameter name in iReport, not the displayed name in fishbowl</param>
        /// <param name="NumOfCopies">Number of copies to print</param>
        /// <returns></returns>
        public async Task<int> PrintReportToPrinter(int ReportId, int PrinterId,  List<ReportParam> ParameterList, int NumOfCopies = 1)
        {
            if (ReportId <= 0)
            {
                throw new ArgumentException("Report Id is invalid");
            }

            if (PrinterId <= 0)
            {
                throw new ArgumentException("Printer Id is Invalid");
            }

            PrintReportToPrinterRq reportToPrinterRq = new PrintReportToPrinterRq();

            reportToPrinterRq.PrinterId = PrinterId;
            reportToPrinterRq.NumberOfCopies = NumOfCopies;
            reportToPrinterRq.ReportId = ReportId;

            if (ParameterList != null && ParameterList?.Count > 0)
            {
                reportToPrinterRq.ParameterList = new ParameterList();
                reportToPrinterRq.ParameterList.ReportParam = ParameterList;
            }

            PrintReportToPrinterRs reportToPrinterRs = await this.IssueJsonRequestAsync<PrintReportToPrinterRs>(reportToPrinterRq);

            return reportToPrinterRs.JobId;
        }

        /// <summary>
        /// Returns a list pf installed printer names from the Fishbowl server to be used in report printing
        /// </summary>
        /// <returns>List of string of printer names</returns>
        public async Task<List<Printer>> GetServerPrinterList()
        {
            GetServerPrinterListRq printerListRq = new GetServerPrinterListRq();

            GetServerPrinterListRs printerListRs = await IssueJsonRequestAsync<GetServerPrinterListRs>(printerListRq);

            return printerListRs.Printers.Printer;
        }

        /// <summary>
        /// Returns list of active reports
        /// </summary>
        /// <returns>List of Report Objects</returns>
        public async Task<List<Report>> GetReports()
        {
            string query = @"Select id as ReportId, name as ReportName
                                            From report
                                            where report.activeFlag = 1";


            return await ExecuteQueryAsync<Report, ReportClassMap>(query);
        }

        /// <summary>
        /// Get the report Id from the name provided. Must be an exact match
        /// </summary>
        /// <param name="reportName"></param>
        /// <returns></returns>
        public async Task<int> GetReportIdFromName(string reportName)
        {
            string query = string.Format(@"Select id as ReportId
                                            From report
                                            where report.name = '{0}'",reportName);


            string reportID = await ExecuteQueryAsync(query);
            if (string.IsNullOrEmpty(reportID))
            {
                throw new FishbowlException(string.Format("Report {0} not found", reportName));
            }
            return int.Parse(reportID);
        }
    }
}
