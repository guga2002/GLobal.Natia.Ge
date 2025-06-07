using Common.Domain.Models.PageVIewModel;
using Common.Persistance.Interface;
using System.Text.RegularExpressions;

namespace Common.Persistance.Services;

public class EmrServices : IService
{
    public async Task<Dictionary<int, string>> GetChanellNames()
    {
        using (var httpClient = new HttpClient())
        {
            HashSet<string> list = new HashSet<string>();
            List<string> CHanells = new List<string>();
            Dictionary<int, string> map = new Dictionary<int, string>();
            HttpResponseMessage response = await httpClient.GetAsync("http://192.168.20.160/mux/mux_config_en.asp");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var qer = content.Split(new string[] { "zInNode7" }, StringSplitOptions.None);
                var res = qer[1].Split(new char[] { '\r', '\n' });
                for (int i = 0; i < res.Length; i++)
                {
                    list.Add(res[i]);
                }

                var newlist = list.Where(io => io.Contains("movelevel:\"3\"")).ToList();

                foreach (var item in newlist)
                {
                    var regular = ExtractChannelName(item);
                    string klr = regular.Split('(')[0];
                    CHanells.Add(klr);

                }

                var logs = CHanells.Skip(153).ToList();
                int ik = 1;
                foreach (var item in logs)
                {
                    map.Add(ik, item);
                    ik++;
                }
                return map;
            }
            else
            {
                Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");
                return null;
            }
        }
    }
    static string ExtractChannelName(string jsonString)
    {
        string pattern = @"name\s*:\s*""([^""]+)""";
        Regex regex = new Regex(pattern);
        Match match = regex.Match(jsonString);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        else
        {
            return "";
        }
    }

    public async Task<List<int>> GetPortsWhereAlarmsIsOn()
    {
        List<int> lst = new List<int>();
        try
        {
            string link = $"http://192.168.20.250/goform/formEMR30?type=8&cmd=1&language=0&slotNo=255&alarmSlotNo=NaN&ran=0.362191435456645";

            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetAsync(link);

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


                                        if (!lst.Any(io => io.ToString() == portNumber))
                                        {

                                            lst.Add(int.Parse(portNumber));
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
                return lst.OrderBy(io => io).ToList();
            }

        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message);
            return null;
        }
    }

    public async Task<IEnumerable<SourceFromEMRModel>> GetEmrChanells(string emrcode)
    {

        var chanells = await GetChanellNames();

        List<SourceFromEMRModel> lst = new List<SourceFromEMRModel>();

        using (var res = new HttpClient())
        {

            var rek = await res.GetAsync($"http://192.168.20.{emrcode}/mux/mux_config_en.asp");


            var str = await rek.Content.ReadAsStringAsync();

            var reki = str.Split(new string[] { "zOutNode7", "var language" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> list = new List<string>();
            list.AddRange(reki[1].Split('\n'));

            foreach (var item in chanells)
            {
                SourceFromEMRModel sor = new SourceFromEMRModel();

                var rel = list.Where(io => io.ToLower().Contains(item.Value.ToLower())).ToList();

                foreach (var it in rel)
                {
                    if (rel.Count() > 0)
                    {
                        var lef = ExtractName(rel[0]);
                        sor.name = item.Value;
                        var splited = lef.Split('(', ')');
                        if (splited.Length > 1)
                        {
                            sor.sourceName = splited[1];
                        }
                        else
                        {
                            sor.sourceName = "_";
                        }
                        sor.card = ExtractCard(rel[0]);
                        sor.port = ExtractPort(rel[0]);
                    }
                }
                lst.Add(sor);
            }
        }
        return lst;
    }
    static string ExtractName(string input)
    {
        Match match = Regex.Match(input, @"name:""([^""]+)""");
        if (match.Success)
        {
            return match.Groups[1].Value.Split("(SID")[0];
        }
        return "";
    }


    static string ExtractCard(string input)
    {
        Match match = Regex.Match(input, @"Card(\d+)");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return "";
    }

    static string ExtractPort(string input)
    {
        Match match = Regex.Match(input, @"Port(\d+)");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return "";
    }
}
