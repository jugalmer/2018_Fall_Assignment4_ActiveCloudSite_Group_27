using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using IEXTrading.Models;
using Newtonsoft.Json;

namespace IEXTrading.Infrastructure.IEXTradingHandler
{
    public class IEXHandler
    {
        static string BASE_URL = "https://api.iextrading.com/1.0/"; //This is the base URL, method specific URL is appended to this.
        HttpClient httpClient;

        public IEXHandler()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        /****
         * Calls the IEX reference API to get the list of symbols. 
        ****/
        public List<Company> GetSymbols()
        {
            string IEXTrading_API_PATH = BASE_URL + "ref-data/symbols";
            string companyList = "";

            List<Company> companies = null;

            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                companyList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            if (!companyList.Equals(""))
            {
                companies = JsonConvert.DeserializeObject<List<Company>>(companyList);
                companies = companies.GetRange(0, 9);
            }

            ////
            ///
            //GetTopFiveSymbols();
           
            return companies;
        }

        public List<CompanyInfo> GetTopFiveSymbols()
        {
            ///
            //HttpClient httpClient;
            //httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.Accept.Clear();
            //httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            ////
            ///
            string IEXTrading_STOCK_DATA_API_PATH = BASE_URL + "stock/{0}/chart/1m";
            

            List<String> companiesSymbolList = GetInfocusSymbols();

            //Dictionary<string, List<CompanyData>> companyDict = new Dictionary<string, List<CompanyData>>();
            Dictionary<string, float> changePercentDict = new Dictionary<string, float>();

            foreach (var companySymbol in companiesSymbolList)
            {
                IEXTrading_STOCK_DATA_API_PATH = BASE_URL + "stock/{0}/chart/1m";
                float changePercent = 0;
                HttpClient httpClient;
                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                string companyDataString = "";
                IEXTrading_STOCK_DATA_API_PATH = string.Format(IEXTrading_STOCK_DATA_API_PATH, companySymbol);
                httpClient.BaseAddress = new Uri(IEXTrading_STOCK_DATA_API_PATH);
                HttpResponseMessage response = httpClient.GetAsync(IEXTrading_STOCK_DATA_API_PATH).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    companyDataString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }

                if (!companyDataString.Equals(""))
                {
                    
                    List<CompanyData> companyOneMonthData = new List<CompanyData>();
                    companyOneMonthData = JsonConvert.DeserializeObject<List<CompanyData>>(companyDataString);
                    foreach(var companyOneDay in companyOneMonthData)
                    {
                        changePercent += companyOneDay.changePercent;
                    }
                    changePercent = changePercent / companyOneMonthData.Count;
                }
                changePercentDict.Add(companySymbol, changePercent);
                
            }

            var MaxFive = from entry in changePercentDict orderby entry.Value descending select entry;
            List<CompanyInfo> companiesList = new List<CompanyInfo>();

            foreach (var companyData in MaxFive.Take(5))
            {
                string IEXTrading_COMPANY_PROFILE_API_PATH = BASE_URL + "stock/{0}/company";
                string companySymbol = companyData.Key;
                HttpClient httpClient;
                CompanyInfo companyInfo = new CompanyInfo();
                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                string companyDataString = "";
                IEXTrading_COMPANY_PROFILE_API_PATH = string.Format(IEXTrading_COMPANY_PROFILE_API_PATH, companySymbol);
                httpClient.BaseAddress = new Uri(IEXTrading_COMPANY_PROFILE_API_PATH);
                HttpResponseMessage response = httpClient.GetAsync(IEXTrading_COMPANY_PROFILE_API_PATH).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    companyDataString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }

                if (!companyDataString.Equals(""))
                {

                    companyInfo = JsonConvert.DeserializeObject<CompanyInfo>(companyDataString);
                    
                }
                companiesList.Add(companyInfo);
            }

           
            return companiesList;
        }

        public List<String> GetInfocusSymbols()
        {
            List<String> popularSymbols = new List<String>();
            ////
            HttpClient httpClient;
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            ///
            List<Infocus> InfocusList = new List<Infocus>();
            string IEXTrading_GAINERS_API_PATH = BASE_URL + "stock/market/list/infocus";
            httpClient.BaseAddress = new Uri(IEXTrading_GAINERS_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_GAINERS_API_PATH).GetAwaiter().GetResult();
            string companies = "";
            if (response.IsSuccessStatusCode)
            {
                companies = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            if (companies != null || !companies.Equals(""))
            {
                InfocusList = JsonConvert.DeserializeObject<List<Infocus>>(companies);
            }

            
            foreach (var company in InfocusList)
            {
                popularSymbols.Add(company.symbol);
            }

            return popularSymbols;
        }

        //public void getFinancials()

        //{

        //    string IEXTrading_API_PATH = BASE_URL + "/stock/aapl/financials?period=quarter";

        //    string financialDetails = "";

        //    httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);

        //    HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

        //    if (response.IsSuccessStatusCode)

        //    {

        //        financialDetails = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        //    }

        //    if (!financialDetails.Equals(""))

        //    {

        //        Console.Write(financialDetails);

        //    }

        //}

        /****
         * Calls the IEX stock API to get 1 year's chart for the supplied symbol. 
        ****/
        public List<Equity> GetChart(string symbol)
        {
            //Using the format method.
            //string IEXTrading_API_PATH = BASE_URL + "stock/{0}/batch?types=chart&range=1y";
            //IEXTrading_API_PATH = string.Format(IEXTrading_API_PATH, symbol);

            string IEXTrading_API_PATH = BASE_URL + "stock/" + symbol + "/batch?types=chart&range=1y";

            string charts = "";
            List<Equity> Equities = new List<Equity>();
            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                charts = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            if (!charts.Equals(""))
            {
                ChartRoot root = JsonConvert.DeserializeObject<ChartRoot>(charts, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                Equities = root.chart.ToList();
            }
            //make sure to add the symbol the chart
            foreach (Equity Equity in Equities)
            {
                Equity.symbol = symbol;
            }

            return Equities;
        }
    }
}
