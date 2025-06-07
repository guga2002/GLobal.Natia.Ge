using AutoMapper;
using Common.Data.Entities;
using Common.Data.Interfaces;
using Common.Domain.Interface;
using Common.Domain.Models;

namespace Common.Domain.Services
{
    public class ChanellServices : AbstractService, IChanellServices
    {
        public ChanellServices(IMapper map, IUniteOfWork wor) : base(map, wor)
        {
        }

        public async Task<bool> AddAsync(ChanellModel Item)
        {
            try
            {
                await work.ChanellRepository.Add(new Chanell()
                {
                    Name = Item.Name,
                });
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteByName(string Name)
        {
            try
            {
                await work.ChanellRepository.Remove(Name);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<ChanellModel>> GetAllAsync()
        {
            var res = await work.ChanellRepository.GetAll();
            List<ChanellModel> mod = new List<ChanellModel>();
            foreach (var item in res)
            {
                var chanell = new ChanellModel()
                {
                    Name = item.Name,
                    ID = item.Id,
                };
                foreach (var it in item.Sources)
                {
                    chanell.Sources.Add(new SourceModel()
                    {
                        ChanellFormat = it.ChanellFormat,
                        Status = it.Status,
                    });
                }
                mod.Add(chanell);
            }
            return mod;
        }

        public Task<bool> UpdateAsync(ChanellModel item)
        {
            throw new NotImplementedException();
        }
        public async Task<Chanell> GetCHanellByName(string name)
        {
            return await work.ChanellRepository.GetCHanellByName(name);
        }

        public async Task<bool> addSource(string name, Source sr)
        {
            return await work.ChanellRepository.addSource(name, sr);
        }
    }
}
