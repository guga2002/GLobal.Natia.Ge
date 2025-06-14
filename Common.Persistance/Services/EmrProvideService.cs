using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Persistance.Entities;
using Common.Persistance.Interface;
using Microsoft.Extensions.Configuration;

namespace Common.Persistance.Services;

public class EmrProvideService: IEmrServices
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;
    public EmrProvideService(HttpClient client, IConfiguration config)
    {
        _client = client;
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    #region GetInfoAboutAlrams
    public async Task<List<string>> GetPortsWhereAlarmsIsOn()
    {
        int count = 0;
        List<string> lst = new List<string>() { };
        var time = DateTime.Now;

        if (time.Hour == 15 && time.Minute >= 00 && time.Minute <= 02 && time.ToString("tt").ToLower() == "pm")//shuadgemshvidobisa
        {
            if (count < 1)
            {
                count++;
                lst.Add("1500");
            }
            count++;
        }
        else
        if (time.Hour == 18 && time.Minute >= 00 && time.Minute <= 02 && time.ToString("tt").ToLower() == "pm")//sagamomshvidobisa
        {
            if (count < 1)
            {
                count++;
                lst.Add("2000");
            }
            count++;
        }
        else
        if (time.Hour == 22 && time.Minute >= 00 && time.Minute <= 02 && time.ToString("tt").ToLower() == "pm")//gamemshvidobisa
        {
            if (count < 1)
            {
                count++;
                lst.Add("2500");
            }
            count++;
        }
        else
        if (time.Hour == 10 && time.Minute >= 01 && time.Minute <= 03 && time.ToString("tt").ToLower() == "am")//dilamshvidobisa
        {
            if (count < 1)
            {
                count++;
                lst.Add("3000");
            }
            count++;
        }
        else
        {
            count = 0;
        }


        if (time.Month == 2 && time.Day == 20 && (time.Hour == 0 && time.Minute >= 00 && time.Minute <= 02 && time.ToString("tt") == "AM" || time.Hour == 11 && time.Minute >= 10 && time.Minute <= 12 && time.ToString("tt") == "AM"))
        {
            lst.Add("2002");//gugas dabadebis dge
        }

        if (time.Month == 5 && time.Day == 16 && (time.Hour == 0 && time.Minute >= 00 && time.Minute <= 02 && time.ToString("tt") == "AM" || time.Hour == 11 && time.Minute >= 10 && time.Minute <= 12 && time.ToString("tt") == "AM"))
        {
            lst.Add("1605");//gio Gujabidzes dabadebis dge
        }

        if (time.Month == 12 && time.Day == 10 && (time.Hour == 0 && time.Minute >= 00 && time.Minute <= 03 && time.ToString("tt") == "AM" || time.Hour == 11 && time.Minute >= 10 && time.Minute <= 13 && time.ToString("tt") == "AM"))
        {
            lst.Add("1210");//Gurulos dabadebis dge
        }

        if (time.Month == 7 && time.Day == 5 && (time.Hour == 0 && time.Minute >= 00 && time.Minute <= 03 && time.ToString("tt") == "AM" || time.Hour == 15 && time.Minute >= 00 && time.Minute <= 03 && time.ToString("tt") == "PM" || time.Hour == 11 && time.Minute >= 10 && time.Minute <= 13 && time.ToString("tt") == "AM"))
        {
            lst.Add("7055");//Mishas dabadebis dge
        }

        if (time.Month == 6 && time.Day == 14 && (time.Hour == 0 && time.Minute >= 00 && time.Minute <= 03 && time.ToString("tt") == "AM" || time.Hour == 11 && time.Minute >= 10 && time.Minute <= 13 && time.ToString("tt") == "AM"))
        {
            lst.Add("6144");//Iraklis dabadebis dge
        }

        if (time.Month == 5 && time.Day == 5 && (time.Hour == 0 && time.Minute >= 00 && time.Minute <= 03 && time.ToString("tt") == "AM" || time.Hour == 11 && time.Minute >= 10 && time.Minute <= 13 && time.ToString("tt") == "AM"))
        {
            lst.Add("0504");//vagos dabadebis dge
        }

        if (time.Month == 8 && time.Day == 17 && (time.Hour == 0 && time.Minute >= 00 && time.Minute <= 03 && time.ToString("tt") == "AM" || time.Hour == 11 && time.Minute >= 10 && time.Minute <= 13 && time.ToString("tt") == "AM"))
        {
            lst.Add("8170");//darchos dabadebis dge
        }

        if (time.Month == 10 && time.Day == 31 && (time.Hour == 0 && time.Minute >= 00 && time.Minute <= 03 && time.ToString("tt") == "AM" || time.Hour == 11 && time.Minute >= 10 && time.Minute <= 13 && time.ToString("tt") == "AM"))
        {
            lst.Add("1031");//Dimas dabadebis dge
        }

        if (time.Month == 10 && time.Day == 13 && (time.Hour == 0 && time.Minute >= 00 && time.Minute <= 03 && time.ToString("tt") == "AM" || time.Hour == 11 && time.Minute >= 10 && time.Minute <= 13 && time.ToString("tt") == "AM"))
        {
            lst.Add("1013");//Vakhos dabadebis dge
        }
        if (time.Month == 03 && time.Day == 12 && (time.Hour == 0 && time.Minute >= 00 && time.Minute <= 03 && time.ToString("tt") == "AM" || time.Hour == 11 && time.Minute >= 10 && time.Minute <= 13 && time.ToString("tt") == "AM"))
        {
            lst.Add("3120");//dekanoidzes dabadebis dge
        }

        var rand = new Random();

        string link = $"http://192.168.20.250/goform/formEMR30?type=8&cmd=1&language=0&slotNo=255&alarmSlotNo=NaN&ran=0.3621{rand.Next()}";
        var res = await _client.GetAsync(link);

        if (res.IsSuccessStatusCode)
        {
            var re = await res.Content.ReadAsStringAsync();

            var splitResult = re.Split(new string[] { "<*1*>", "<html>", "<html/>", "</html>" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in splitResult.OrderByDescending(io => io.Contains("Main GbE Card (C451E)")))
            {
                if (item.Contains("Card 7 GbE"))
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        var axali = item.Split(new string[] { "Card (C451E): Card 7" }, StringSplitOptions.None);
                        if (axali.Length >= 2)
                        {
                            string pattern = @"Port (\d+)";
                            Match match = Regex.Match(axali[1], pattern);
                            if (match.Success)
                            {
                                string portNumber = match.Groups[1].Value;
                                if (!lst.Any(io => io == portNumber))
                                {

                                    lst.Add(portNumber);
                                }
                            }
                            else
                            {
                                Console.WriteLine("portis nomeri ver vnaxet");
                            }
                        }
                    }
                }
            }
        }
        return lst;
    }
    #endregion

    #region ActivateCardNow
    public async Task<List<ActiveCard>> GetCardsWhichNeedToBeActivate(string Emr)
    {
        var lst = new List<ActiveCard>();

        string link = $@"http://192.168.20.{Emr}/goform/formEMR30?type=8&cmd=1&language=0&slotNo=255&alarmSlotNo=NaN&ran=0.36219193628209";
        var res = await _client.GetAsync(link);

        if (res.IsSuccessStatusCode)
        {
            var re = await res.Content.ReadAsStringAsync();

            var splitResult = re.Split(new string[] { "<*1*>", "<html>", "<html/>", "</html>" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in splitResult.OrderByDescending(io => io.Contains("Demod Desc Card")))
            {
                if (item.Contains("Descramble Abnormally"))
                {
                    string pattern = @"Card\s+(\d+).*?Card\s+([A-Za-z])";

                    Match match = Regex.Match(item, pattern);
                    if (match.Success)
                    {
                        string firstCard = match.Groups[1].Value;
                        string secondCard = match.Groups[2].Value;

                        lst.Add(new ActiveCard
                        {
                            Card = firstCard,
                            Port = secondCard,
                            Emr = Emr,
                        });
                    }
                }
            }
        }
        return lst;
    }


    #endregion

    public async Task<List<string>> GetTotalProblemsWhatWeHaveRightNowOverSystem()
    {
        var rand = new Random();
        var res = _config.GetSection("EmrIps").Get<List<string>>() ?? throw new ArgumentNullException("EmrIps config section missing");
        List<string> total = new List<string>();
        foreach (var it in res)
        {

            string link = $"http://{it}/goform/formEMR30?type=8&cmd=1&language=0&slotNo=255&alarmSlotNo=NaN&ran=0.3621{rand.Next()}";
            var result = await _client.GetAsync(link);

            if (result.IsSuccessStatusCode)
            {
                var re = await result.Content.ReadAsStringAsync();

                var splitResult = re.Split(new string[] { "<*1*>", "<html>", "<html/>", "</html>" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in splitResult.OrderByDescending(io => io.Contains("Main GbE Card (C451E)")))
                {
                    if (item.Contains("Card 7 GbE"))
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            var axali = item.Split(new string[] { "Card (C451E): Card 7" }, StringSplitOptions.None);
                            if (axali.Length >= 2)
                            {
                                string pattern = @"Port (\d+)";
                                Match match = Regex.Match(axali[1], pattern);
                                if (match.Success)
                                {
                                    string portNumber = match.Groups[1].Value;
                                    if (!total.Any(io => io == portNumber))
                                    {

                                        total.Add(portNumber);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("portis nomeri ver vnaxet");
                                }
                            }
                        }
                    }
                }
            }
        }
        return total.ToList();
    }
}
