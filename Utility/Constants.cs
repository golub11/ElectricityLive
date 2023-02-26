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
        internal static string dayAheadCode= "A44";


        // TODO italy, sweden, denmark and norway have regions, it is not singular value
        // it may happen that some countries do not have entries
        internal static Dictionary<string, string> countryDomains = new Dictionary<string, string>
        {
            {"Estonia", "10Y1001A1001A39I"},
            {"Ireland", "10Y1001A1001A59C"},
            //{"Denmark", "10Y1001A1001A65H"},
            {"Germany", "10Y1001A1001A82H"},
            {"Malta", "10Y1001A1001A93C"},
            {"United Kingdom", "10YGB----------A"},
            {"Moldova", "10Y1001A1001A990"},
            {"Armenia", "10Y1001A1001B004"},
            {"Georgia", "10Y1001A1001B012"},
            {"Azerbaijan", "10Y1001A1001B05V"},
            {"Switzerland", "10YCH-SWISSGRIDZ"},
            {"Ukraine", "10Y1001C--00003F"},
            {"Albania", "10YAL-KESH-----5"},
            {"Belgium", "10YBE----------2"},
            {"Bosnia and Herzegovina", "10YBA-JPCC-----D"},
            {"Bulgaria", "10YCA-BULGARIA-R"},
            {"Montenegro", "10YCS-CG-TSO---S"},
            {"Serbia", "10YCS-SERBIATSOV"},
            {"Czech Republic", "10YCZ-CEPS-----N"},
            {"Cyprus", "10YCY-1001A0003J"},
            {"Finland", "10YFI-1--------U"},
            {"Croatia", "10YHR-HEP------M"},
            {"Greece", "10YGR-HTSO-----Y"},
            {"Hungary", "10YHU-MAVIR----U"},
            {"Lithuania", "10YLT-1001A0008Q"},
            {"North Macedonia", "10YMK-MEPSO----8"},
            //{"Norway", "10YNO-0--------C"},
            {"Netherlands", "10YNL----------L"},
            {"Poland", "10YPL-AREA-----S"},
            {"Portugal", "10YPT-REN------W"},
            //{"Sweden", "10YSE-1--------K"},
            {"Romania", "10YRO-TEL------P"},
            {"Slovakia", "10YSK-SEPS-----K"},
            {"Slovenia", "10YSI-ELES-----O"},
            {"Iceland", "10Y1001A1001A59C"},
            {"Russia", "10Y1001A1001A49F"},
            {"Belarus", "10Y1001A1001A51S"},
            {"France", "10YFR-RTE------C"},
            {"Spain", "10YES-REE------0"},
            {"Latvia", "10YLV-1001A00074"},
            {"Luxembourg", "10YLU-CEGEDEL-NQ"},
            {"Austria", "10YAT-APG------L"},
            {"Turkey", "10YTR-TEIAS----W"},
            
        };

    }
}
