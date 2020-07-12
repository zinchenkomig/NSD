using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Text;
using System.Xml;
using Newtonsoft.Json;

namespace hw
{
    class Program
    {
        static string msg;
        private static async Task GoQuery(string query)
        {
            var stringTask = client.GetStringAsync(query);

            msg = await stringTask;
        }
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            string output_file = "Output.csv";
            string inpformat = "<Ticker> <date-from> <date-till>";
            string dateformat = "yyyy-MM-dd";
            if(args.Length < 3){
                Console.WriteLine("Input format: <ticker> <date beginning> <date ending>");

            }
            else{
            DateTime dateFrom, dateTill;
            Console.WriteLine("Command line arguments:");
                foreach(string ar in args){
                    Console.WriteLine(ar);
                }

            if (DateTime.TryParse(args[1], out dateFrom) &&
                DateTime.TryParse(args[2], out dateTill)){
                    Console.WriteLine("Do you want to set minimum trading value? (y/n)");
                    double minTrade;
                    string answer = Console.ReadLine();
                    if(answer == "y"){
                        Console.WriteLine("Enter minimum trading value:");
                        string minstr = Console.ReadLine();
                        minTrade = Convert.ToDouble(minstr);
                    }
                    else{
                        minTrade = 0.0;
                    }
                    string tradeTicker = args[0];
                    string query = $"https://iss.moex.com/iss/securities/{tradeTicker}/aggregates";
                    await GoQuery(query);
                    using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(output_file, false))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(msg);
                        var xRoot = doc.DocumentElement;
                        XmlNodeList columns = doc.GetElementsByTagName("columns");
                        foreach(XmlNode xn in columns[0].ChildNodes){
                            XmlNode attr = xn.Attributes.GetNamedItem("name");
                            file.Write(attr.Value + ',');
                        }
                        file.WriteLine("");
                    }
                Console.WriteLine("Dates parsed successfully.");
                Console.WriteLine($"Trading results from {dateFrom} till {dateTill}");
                for(DateTime currentDate = dateFrom; currentDate <= dateTill; currentDate = currentDate.AddDays(1)){
                    string dateStr = currentDate.ToString(dateformat);
                    query = $"https://iss.moex.com/iss/securities/{tradeTicker}/aggregates?date={dateStr}";
                    Console.WriteLine(query);
                    await GoQuery(query);
                    using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(output_file, true))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(msg);
                        var xRoot = doc.DocumentElement;
                        XmlNodeList columns = doc.GetElementsByTagName("rows");
                        if(columns[0].HasChildNodes){
                        foreach(XmlNode xn in columns[0].ChildNodes){
                            string valstr = xn.Attributes.GetNamedItem("value").Value;
                            double value;
                            if(valstr == "") value = 0.0;
                            try{
                                value = Convert.ToDouble(valstr);
                            }
                            catch(FormatException){
                                value = 0.0;
                            }
                            if (value >= minTrade){
                            foreach(XmlNode attr in xn.Attributes){
                                file.Write(attr.Value + ',');
                            }
                            file.WriteLine("");
                            }
                        }
                        }
                    }
                }
            }
            else{
                Console.WriteLine("Error parsing date.");
                Console.WriteLine("Input format is: {0}", inpformat);
                Console.WriteLine("Please use the next format to enter date: {0}",
                 dateformat);
            }
            }

        }

    }
}
