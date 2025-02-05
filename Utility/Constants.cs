namespace nigo.Utility
{
    public abstract class Constants
    {
        public static string apiUrl = "https://web-api.tp.entsoe.eu/api?";
        internal static string emptyValue = "";
        internal static string documentTypeParam = "documentType";
        internal static string periodEndParam = "periodEnd";
        internal static string periodStartParam = "periodStart";
        internal static string outDomainParam = "out_Domain";
        internal static string securityTokenParam = "securityToken";
        internal static string inDomainParam = "in_Domain";


        internal static string csvFilePath = "dataTest.csv";

        // it may happen that some countries do not have entries
        internal static Dictionary<string, dynamic> countryDomains = new Dictionary<string, dynamic>
        {
            {"Serbia", "10YCS-SERBIATSOV"},
            {"Italy", new string[] {"10Y1001A1001A73I","10Y1001A1001A70O","10Y1001A1001A71M","10Y1001A1001A788","10Y1001C--00096J","10Y1001A1001A74G","10Y1001A1001A885","10Y1001A1001A893" }},
            {"Denmark", new string[] {"10YDK-1--------W","10YDK-2--------M"}},
            {"Estonia", "10Y1001A1001A39I"},
            {"Germany", "10Y1001A1001A82H"},
            {"Ireland", "10Y1001A1001A59C"},
            {"Switzerland", "10YCH-SWISSGRIDZ"},
            {"Belgium", "10YBE----------2"},
            {"Bulgaria", "10YCA-BULGARIA-R"},
            {"Montenegro", "10YCS-CG-TSO---S"},
            {"Czech Republic", "10YCZ-CEPS-----N"},
            {"Finland", "10YFI-1--------U"},
            {"Croatia", "10YHR-HEP------M"},
            {"Greece", "10YGR-HTSO-----Y"},
            {"Hungary", "10YHU-MAVIR----U"},
            {"Lithuania", "10YLT-1001A0008Q"},
            {"North Macedonia", "10YMK-MEPSO----8"},
            {"Norway", new string[] {"10Y1001A1001A64J","10Y1001C--001219","10YNO-3--------J","10YNO-4--------9","10Y1001A1001A48H"}},
            {"Netherlands", "10YNL----------L"},
            {"Poland", "10YPL-AREA-----S"},
            {"Portugal", "10YPT-REN------W"},
            {"Sweden", new string[] {"10Y1001A1001A44P","10Y1001A1001A45N","10Y1001A1001A46L","10Y1001A1001A47J"}},
            {"Romania", "10YRO-TEL------P"},
            {"Slovakia", "10YSK-SEPS-----K"},
            {"Slovenia", "10YSI-ELES-----O"},
            {"Iceland", "10Y1001A1001A59C"},
            {"France", "10YFR-RTE------C"},
            {"Spain", "10YES-REE------0"},
            {"Latvia", "10YLV-1001A00074"},
            {"Austria", "10YAT-APG------L"}
            

            //{"Malta", "10Y1001A1001A93C"},
            //{"United Kingdom", "10YGB----------A"},
            //{"Moldova", "10Y1001A1001A990"},
            //{"Armenia", "10Y1001A1001B004"},
            //{"Georgia", "10Y1001A1001B012"},
            //{"Azerbaijan", "10Y1001A1001B05V"},
            
            //{"Ukraine", "10Y1001C--00003F"},
            //{"Albania", "10YAL-KESH-----5"},
            //{"Bosnia and Herzegovina", "10YBA-JPCC-----D"},
            //{"Cyprus", "10YCY-1001A0003J"},
            // {"Norway_2", "10Y1001C--001219"},
            // {"Norway_3", "10YNO-3--------J"},
            // {"Norway_4", "10YNO-4--------9"},
            // {"Norway_5", "10Y1001A1001A48H"},
            // {"Sweden_2", "10Y1001A1001A45N"},
            // {"Sweden_3", "10Y1001A1001A46L"},
            // {"Sweden_4", "10Y1001A1001A47J"},
            //{"Russia", "10Y1001A1001A49F"},
            //{"Belarus", "10Y1001A1001A51S"},
            //{"Turkey", "10YTR-TEIAS----W"},
            //{"Luxembourg", "10YLU-CEGEDEL-NQ
            // {"Italy_CNorth","10Y1001A1001A70O" },
            // {"Italy_CSouth","10Y1001A1001A71M" },
            // {"Italy_South","10Y1001A1001A788" },
            // {"Italy_Calabria","10Y1001C--00096J" },
            // {"Italy_Sicily","10Y1001A1001A75E" },
            // {"Italy_Sardinia","10Y1001A1001A74G" },
            // {"Italy_SACODC","10Y1001A1001A885" },            
            // {"Italy_SACOAC","10Y1001A1001A893" },           
        };

    }
}

abstract class DocumentType
{
    internal static string priceDocument = "A44";

}
