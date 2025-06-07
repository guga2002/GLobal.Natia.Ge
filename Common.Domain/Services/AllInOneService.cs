using AutoMapper;
using Common.Data.Interfaces;
using Common.Domain.Interface;
using Common.Domain.Models;

namespace Common.Domain.Services;

public class AllInOneService : AbstractService, IAllInOneService
{
    public AllInOneService(IMapper map, IUniteOfWork wor) : base(map, wor)
    {
    }

    public async Task<List<AllInOneModel>> SeedData()
    {
        List<AllInOneModel> model = new List<AllInOneModel>();
        var source = await work.sourceRepository.GetAll();
        foreach (var item in source)
        {
            var mod = new AllInOneModel();
            mod.SourceIsActive = item.Status;
            mod.ChanellFormat = item.ChanellFormat;


            if (item is not null)
            {
                var rek = await work.satteliterFrequencyRepository.GetByIdIds(item.SatteliteId ?? 400);
                if (rek is not null)
                {
                    mod.Frequency = $"{rek.Frequency} {rek.Polarisation} {rek.SymbolRate}  {rek.Degree}°";
                }
            }
            else
            {
                mod.Frequency = "Undefined";
            }
            if (item.Transcoder is not null)
            {
                mod.TranscoderInfo = $"EMR:{item.Transcoder.EmrNumber} ->Card:{item.Transcoder.Card} ->Port:{item.Transcoder.Port}";
            }
            else
            {
                mod.TranscoderInfo = "Undefined";
            }
            if (item.chanell is not null)
            {
                mod.ChanellName = item.chanell.Name + ' ' + item.sourceName;
            }
            else
            {
                mod.ChanellName = "Undefined";
            }
            if (item.Desclambler is not null)
            {
                mod.DesclamlerInfo = $"EMR:{item.Desclambler.EmrNumber} -> Card:{item.Desclambler.Card} -> Port:{item.Desclambler.Port}";
            }
            else
            {
                mod.DesclamlerInfo = "Undefined";
            }
            model.Add(mod);
        }

        return model;
    }
}
