using Common.Domain.Models.PageVIewModel;
using Common.Persistance.Entities;
using Common.Persistance.Interface;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Common.Persistance.Services;

public class EmrServices : IService
{
    private readonly HttpClient _httpClient;
    public EmrServices(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

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


    public async Task<List<ChannelInfo>> GetIpChanellsChannelInfoAsync()
    {
        try
        {
            var urls = new[]
                        {
                          "http://192.168.20.210/goform/formEMR30?type=5&cmd=1&rowIndex=0&language=0&slotNo=6&portNo=0&ran=0.27117265330512463",
                          "http://192.168.20.210/goform/formEMR30?type=5&cmd=1&rowIndex=32&language=0&slotNo=6&portNo=0&ran=0.6016903270097027",
                          "http://192.168.20.210/goform/formEMR30?type=5&cmd=1&rowIndex=64&language=0&slotNo=6&portNo=0&ran=0.9630006746823162",
                          "http://192.168.20.210/goform/formEMR30?type=5&cmd=1&rowIndex=96&language=0&slotNo=6&portNo=0&ran=0.5519907333657804",
                          "http://192.168.20.210/goform/formEMR30?type=5&cmd=1&rowIndex=128&language=0&slotNo=6&portNo=0&ran=0.5395086937741503"
                        };

            var allChannels = new List<ChannelInfo>();

            foreach (var url in urls)
            {
                var channels = await GetChannelInfoFromUrlAsync(url);
                allChannels.AddRange(channels);
            }
            return allChannels;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching channel info: {ex.Message}");
            return new List<ChannelInfo>();
        }
    }

    private async Task<List<ChannelInfo>> GetChannelInfoFromUrlAsync(string url)
    {
        var response = await _httpClient.GetStringAsync(url);
        var channelInfos = new List<ChannelInfo>();
        var blocks = response.Split("<*1*>", StringSplitOptions.RemoveEmptyEntries);

        foreach (var block in blocks)
        {
            var parts = block.Split("<*2*>", StringSplitOptions.None);
            if (parts.Length >= 5)
            {
                var rawName = parts[1].Trim();
                var bitrate1Str = parts[3].Trim();
                var bitrate2Str = parts[4].Trim();

                var nameOnly = Regex.Replace(rawName, @"(?i)^Port\d*|\d+|[^\p{L}\s]", "").Trim();

                if (!string.IsNullOrWhiteSpace(nameOnly) && Regex.IsMatch(nameOnly, @"[a-zA-Z\u10A0-\u10FF]"))
                {
                    double bitrate1 = ParseBitrate(bitrate1Str);
                    double bitrate2 = ParseBitrate(bitrate2Str);

                    channelInfos.Add(new ChannelInfo
                    {
                        Name = nameOnly,
                        Bitrate1Mbps = bitrate1,
                        Bitrate2Mbps = bitrate2
                    });
                }
            }
        }

        return channelInfos;
    }

    private double ParseBitrate(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return 0.0;

        raw = raw.Trim().ToLowerInvariant();

        if (raw.EndsWith("mbps"))
        {
            if (double.TryParse(raw.Replace("mbps", "").Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var mbps))
                return mbps;
        }
        else if (raw.EndsWith("kbps"))
        {
            if (double.TryParse(raw.Replace("kbps", "").Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var kbps))

                if (kbps < 650)
                    return 0.01;
        }
        else if (raw.EndsWith("bps"))
        {
            if (double.TryParse(raw.Replace("bps", "").Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var bps))
                return bps / 1000000.0;
        }

        return 0.0;
    }

}
