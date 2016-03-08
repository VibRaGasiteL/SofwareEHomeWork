using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Configuration;
using Newtonsoft.Json;
using System.Diagnostics;

namespace SofwareE
{
    class Program
    {
        static string[] arrayOfInitialData = new string[3];
        static int CurrentPopulation = Convert.ToInt32(arrayOfInitialData[0]);
        static int WomenCount = Convert.ToInt32(arrayOfInitialData[1]);
        static long TimeOfRepeat = Convert.ToInt64(arrayOfInitialData[2]);
        static void Main(string[] args)
        {
            Console.Title = "Waiting...";
            Console.Write("Введите количество жителей города: ");
            CurrentPopulation = Convert.ToInt32(Console.ReadLine());
            Console.Write("Из них женщин: ");
            WomenCount = Convert.ToInt32(Console.ReadLine());
            Console.Write("Укажите время, за которое выдавать результаты(в милисекундах): ");
            TimeOfRepeat = Convert.ToInt64(Console.ReadLine());
            Console.WriteLine("--------------------------------------------------");
            for (;;)
            {
                PrintOutStatistic( AnalyzeStatisticEvent(GetStatisticEvent()));
            }
        }

        /*public static string [] InitialData()
        {
            string[] arrayOfInitialData = new string[2];
            Console.Write("Введите количество жителей города: ");
            arrayOfInitialData[0] = Console.ReadLine();
            Console.Write("Из них женщин: ");
            arrayOfInitialData[1] = Console.ReadLine();
            return arrayOfInitialData;
        }*/

        public static List<Dictionary<string, string>> GetStatisticEvent()
        {
            string responsetext = "";
            try
            {
                responsetext = new WebClient().DownloadString("http://api.lod-misis.ru/testassignment");
            }
            catch (WebException)
            {
                Console.WriteLine("Не удалось установить связь с сервером!");
                Console.ReadKey();
                Environment.Exit(0);
            }
            try
            {
                List<Citizens> listOfCitizens = JsonConvert.DeserializeObject<List<Citizens>>(responsetext);
                List<Dictionary<string, string>> ListOfdictionaryOfCitizens = new List<Dictionary<string, string>>();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while (stopwatch.ElapsedMilliseconds <= TimeOfRepeat)
                {
                    Console.Title = Convert.ToString((100 * stopwatch.ElapsedMilliseconds) / TimeOfRepeat) + " %";
                    responsetext = new WebClient().DownloadString("http://api.lod-misis.ru/testassignment");
                    foreach (var person in listOfCitizens)
                    {
                        Dictionary<string, string> dictionaryOfCitizens = new Dictionary<string, string>();
                        dictionaryOfCitizens.Add("sex", person.Gender);
                        dictionaryOfCitizens.Add("condition", person.Condition);
                        ListOfdictionaryOfCitizens.Add(dictionaryOfCitizens);
                    }
                }
                stopwatch.Stop();
                return ListOfdictionaryOfCitizens;
            }
            catch (JsonSerializationException)
            {

                List<Dictionary<string, string>> ListOfdictionaryOfCitizens = new List<Dictionary<string, string>>();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while (stopwatch.ElapsedMilliseconds <= TimeOfRepeat)
                {
                    Console.Title = Convert.ToString((100 * stopwatch.ElapsedMilliseconds) / TimeOfRepeat) + " %";
                    responsetext = new WebClient().DownloadString("http://api.lod-misis.ru/testassignment");
                    string[] arrayOfPersons = responsetext.Split(new char[] { ';', '"' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var person in arrayOfPersons)
                    {
                        string[] arrayOfPersonInfo = person.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        Dictionary<string, string> dictionaryOfCitizens = new Dictionary<string, string>();
                        dictionaryOfCitizens.Add("sex", arrayOfPersonInfo[0]);
                        dictionaryOfCitizens.Add("condition", arrayOfPersonInfo[1]);
                        ListOfdictionaryOfCitizens.Add(dictionaryOfCitizens);
                    }
                }
                stopwatch.Stop();
                return ListOfdictionaryOfCitizens;
            }
            catch (NullReferenceException)
            {
                List<Dictionary<string, string>> ListOfdictionaryOfCitizens = new List<Dictionary<string, string>>();
                Dictionary<string, string> dictionaryOfCitizens = new Dictionary<string, string>();
                dictionaryOfCitizens.Add("sex", "0");
                dictionaryOfCitizens.Add("condition", "0");
                ListOfdictionaryOfCitizens.Add(dictionaryOfCitizens);
                return ListOfdictionaryOfCitizens;
            }
        }

        public static Dictionary<string, string> AnalyzeStatisticEvent(List<Dictionary<string, string>> ListOfDict)
        {
            int ManCount = CurrentPopulation - WomenCount;
            int Death = 0;
            int Birth = 0;
            float Fertility;
            float Mortality;
            foreach (var person in ListOfDict)
            {
                switch (person["condition"])
                {
                    case "Died":
                        CurrentPopulation = CurrentPopulation - 1;
                        Death = Death + 1;
                        if (person["sex"] == "Male")
                        {
                            ManCount = ManCount - 1;
                        }
                        else
                        {
                            WomenCount = WomenCount - 1;
                        }
                        break;
                    case "Born":
                        CurrentPopulation = CurrentPopulation + 1;
                        Birth = Birth + 1;
                        if (person["sex"] == "Male")
                        {
                            ManCount = ManCount + 1;
                        }
                        else
                        {
                            WomenCount = WomenCount + 1;
                        }
                        break;
                    case "0":
                        Dictionary<string, string> NullDictionaryOfResults = new Dictionary<string, string>();
                        NullDictionaryOfResults.Add("currentpopulation", "0");
                        return NullDictionaryOfResults;
                }
            }
            Mortality = ((float)Death / (float)CurrentPopulation) * 100; //на выход смертность
            Fertility = ((float)Birth / (float)CurrentPopulation) * 100;  //на выход рождаемость
            var SexCompositionWomen = 0;
            SexCompositionWomen = (WomenCount * 100) / CurrentPopulation;  //на выход процент женщин
            var SexCompositionMan = 100 - SexCompositionWomen; //на выход процент мужчин
            Dictionary<string, string> DictionaryOfResults = new Dictionary<string, string>();
            DictionaryOfResults.Add("currentpopulation", Convert.ToString(CurrentPopulation));
            DictionaryOfResults.Add("mortality", Convert.ToString(Mortality).Remove(3));
            DictionaryOfResults.Add("fertility", Convert.ToString(Fertility).Remove(3));
            DictionaryOfResults.Add("sexcompositionwomen", Convert.ToString(SexCompositionWomen));
            DictionaryOfResults.Add("sexcompositionman", Convert.ToString(SexCompositionMan));
            DictionaryOfResults.Add("deaths", Convert.ToString(Death));
            DictionaryOfResults.Add("births", Convert.ToString(Birth));
            return DictionaryOfResults;
        }

        public static void PrintOutStatistic(Dictionary<string, string> DictionaryOfResults)
        {
            if (DictionaryOfResults["currentpopulation"] == "0")
            {
                return;
            }
            else
            {
                Console.WriteLine("Текущее население города {0};", DictionaryOfResults["currentpopulation"]);
                Console.WriteLine("Смертность за выбранный промежуток времени: {0};", DictionaryOfResults["mortality"]);
                Console.WriteLine("Рождаемость за выбранный промежуток времени: {0};", DictionaryOfResults["fertility"]);
                Console.WriteLine("На данный момент в городе: {0}" + "% женщин и {1}" + "% мужчин;", DictionaryOfResults["sexcompositionwomen"], DictionaryOfResults["sexcompositionman"]);
                Console.WriteLine("Cмертей за это время: {0};", DictionaryOfResults["deaths"]);
                Console.WriteLine("Рождено за это время: {0};", DictionaryOfResults["births"]);
                Console.WriteLine("--------------------------------------------------");
                Console.SetCursorPosition(0, 4);
            }
        }
    }
}
