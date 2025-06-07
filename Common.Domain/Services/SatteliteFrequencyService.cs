using AutoMapper;
using Common.Data.Entities;
using Common.Data.Interfaces;
using Common.Domain.Interface;
using Common.Domain.Models;
using Common.Domain.Models.ViewModels;

namespace Common.Domain.Services;

public class SatteliteFrequencyService : AbstractService, ISatteliteFrequencyService
{
    public SatteliteFrequencyService(IMapper map, IUniteOfWork wor) : base(map, wor)
    {
    }

    public async Task<List<int>> GetAllarmsFromRegion()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
        using (var client = new HttpClient(handler))
        {
            var result = await client.GetAsync("https://192.168.1.102:3999/api/RegionChecker/GetRegionWhereOpticIsOff");

            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();

                var ports = System.Text.Json.JsonSerializer.Deserialize<List<int>>(content);
                return ports ?? new List<int>();
            }
            return new List<int>();
        }
    }

    public async Task<bool> AddAsync(SatteliteFrequencyModel Item)
    {
        try
        {
            var sattelit = new SatteliteFrequency()
            {
                Degree = Item.Degree,
                Frequency = Item.Frequency,
                Polarisation = Item.Polarisation,
                SymbolRate = Item.SymbolRate,
                PortIn250 = Item.PortIn250,
                EmrNumber = Item.EmrNumber,
                CardNumber = Item.CardNumber,
                portNumber = Item.portNumber,
            };
            await work.satteliterFrequencyRepository.Add(sattelit);
            return true;

        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            await work.satteliterFrequencyRepository.Remove(id);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<IEnumerable<SatteliteFrequencyModel>> GetAllAsync()
    {
        try
        {
            var res = await work.satteliterFrequencyRepository.GetAll();

            List<SatteliteFrequencyModel> mod = new List<SatteliteFrequencyModel>();

            foreach (var item in res)
            {
                var re = new SatteliteFrequencyModel()
                {
                    Degree = item.Degree,
                    Frequency = item.Frequency,
                    Polarisation = item.Polarisation,
                    SymbolRate = item.SymbolRate,
                    PortIn250 = item.PortIn250,
                };
                if (item.EmrNumber > 0 && item.CardNumber >= 0)
                {
                    Random ran = new Random();
                    using (var htpserver = new HttpClient())
                    {
                        var response = await htpserver.GetAsync($"http://192.168.20.{item.EmrNumber}/goform/formEMR30?type=2&cmd=1&language=0&slotNo={item.CardNumber - 1}&portNo={item.portNumber - 1}&ran=0.99{ran.Next()}");
                        var cont = await response.Content.ReadAsStringAsync();
                        var splited = cont.Split(new string[] { "<*1*>", "<html>", "</html>" }, StringSplitOptions.None);
                        if (splited[0].ToLower() == "unlock")
                        {
                            re.HaveError = true;

                        }
                        else
                        {
                            re.mer = splited[4];
                        }
                    }
                }
                mod.Add(re);
            }
            return mod;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> UpdateAsync(SatteliteFrequencyModel item)
    {
        try
        {
            var res = new SatteliteFrequency()
            {
                Frequency = item.Frequency,
                Polarisation = item.Polarisation,
                SymbolRate = item.SymbolRate,
                PortIn250 = item.PortIn250,
            };
            await work.satteliterFrequencyRepository.Update(res);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<IEnumerable<SatteliteViewMonitoring>> GetMonitoringFrequencies()
    {
        try
        {
            var res = await work.satteliterFrequencyRepository.GetAll();

            var groupedByDegree = res.GroupBy(item => item.Degree);

            List<SatteliteViewMonitoring> monitor = new List<SatteliteViewMonitoring>();

            foreach (var group in groupedByDegree)
            {
                SatteliteViewMonitoring mon = new SatteliteViewMonitoring();
                mon.Degree = group.Key;
                mon.details = new List<SatteliteFrequencyModel>();

                foreach (var item in group)
                {
                    //mon.details.Add
                    var mod = new SatteliteFrequencyModel()
                    {
                        HaveError = false,
                        PortIn250 = item.PortIn250,
                        Frequency = item.Frequency,
                        Polarisation = item.Polarisation,
                        SymbolRate = item.SymbolRate,
                    };

                    if (item.EmrNumber > 0 && item.CardNumber >= 0)
                    {
                        Random ran = new Random();
                        using (var htpserver = new HttpClient())
                        {
                            var response = await htpserver.GetAsync($"http://192.168.20.{item.EmrNumber}/goform/formEMR30?type=2&cmd=1&language=0&slotNo={item.CardNumber - 1}&portNo={item.portNumber - 1}&ran=0.99{ran.Next()}");
                            var cont = await response.Content.ReadAsStringAsync();
                            var splited = cont.Split(new string[] { "<*1*>", "<html>", "</html>" }, StringSplitOptions.None);
                            if (splited[0].ToLower().Equals("unlock"))
                            {
                                mod.HaveError = true;
                            }
                            else
                            {
                                if (item.EmrNumber == 30 && item.CardNumber == 1)
                                {
                                    mod.mer = splited[5];
                                }
                                if (item.EmrNumber == 40)
                                {
                                    mod.mer = splited[6];
                                }
                                else
                                {
                                    mod.mer = splited[4];
                                }
                            }
                        }
                    }
                    mon.details.Add(mod);
                }

                monitor.Add(mon);
            }

            return monitor;
        }
        catch (Exception)
        {
            throw;
        }

    }

    //SatteliteFrequencyModel

    public async Task<SatteliteFrequencyModel> GetDetailsByEmrport(int PortId)
    {
        SatteliteFrequencyModel sat = new SatteliteFrequencyModel();
        var de = await work.satteliterFrequencyRepository.GetById(PortId);
        if (de is not null)
        {


            sat.Polarisation = de.Polarisation;
            sat.Frequency = de.Frequency;
            sat.SymbolRate = de.SymbolRate;
            sat.Degree = de.Degree;
            sat.PortIn250 = PortId;
            foreach (var item in de.Sources)
            {
                //if (item.chanell is not null)
                ///{
                var chan = new ChanellModel()
                {
                    Name = item.chanell.Name,

                };
                foreach (var it in item.chanell.Sources)
                {
                    chan.Sources.Add(new SourceModel()
                    {
                        ChanellFormat = it.ChanellFormat,
                        Status = it.Status
                    });
                }
                sat.chanellid.Add(chan);
                // }
            }
        }
        return sat;
    }
}
