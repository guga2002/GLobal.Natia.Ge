using System.Web.WebPages.Html;
using AutoMapper;
using Common.Data.Entities;
using Common.Data.Interfaces;
using Common.Domain.Interface;
using Common.Domain.Models;

namespace Common.Domain.Services;

public class SourceService : AbstractService, IsourceServices
{
    public SourceService(IMapper map, IUniteOfWork wor) : base(map, wor)
    {
    }

    public async Task<bool> AddAsync(SourceModel item)
    {
        try
        {
            var channel = await work.ChanellRepository.GetById(item.ChanellId);

            if (channel == null)
            {
                throw new ArgumentException("Arxi ar aris");
                //return false;
            }
            var satellite = await work.satteliterFrequencyRepository.GetByIdIds(item.Reciever_ID ?? 100);
            var source = new Source()
            {
                ChanellFormat = item.ChanellFormat,
                Status = true,
                sourceName = item.sourceName,
                card = item.card,
                EmrNumber = int.Parse(item.EMR),
                port = item.port,
                ChanellId = item.ChanellId,
            };

            if (satellite != null)
            {
                source.SatteliteId = item.Reciever_ID;
            }
            await work.sourceRepository.Add(source);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
    }


    public async Task<bool> Delete(int id)
    {
        try
        {
            await work.sourceRepository.Remove(id);
            return true;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<IEnumerable<SourceModel>> GetAllAsync()
    {
        List<SourceModel> mod = new List<SourceModel>();
        var res = await work.sourceRepository.GetAll();
        foreach (var item in res)
        {
            var mok = new SourceModel
            {
                ChanellFormat = item.ChanellFormat,
                Status = item.Status,
                Reciever_ID = item.SatteliteId,
                ChanellId = item.ChanellId,
                Id = item.Id,
            };
            mod.Add(mok);
        }
        return mod;
    }

    public Task<bool> UpdateAsync(SourceModel item)
    {
        throw new NotImplementedException();
    }

    public async Task<SourceModel> GetDropDOwnListData()
    {
        return null;
    }
}
