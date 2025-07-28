using AutoMapper;
using Common.Data.Entities;
using Common.Data.Interfaces;
using Common.Domain.Interface;
using Common.Domain.Models.ViewModels;

namespace Common.Domain.Services
{
    public class TranscoderServices : AbstractService, ITranscoderService
    {
        public TranscoderServices(IMapper map, IUniteOfWork wor) : base(map, wor)
        {
        }

        public async Task<bool> AddAsync(TranscoderViewModel Item)
        {

            var source = await work.sourceRepository.GetById(int.Parse(Item.Id));
            if (source != null)
            {
                source.ChanellFormat = Item.TranscodingFormat;
                Transcoder trans = new Transcoder()
                {
                    Port = int.Parse(Item.Port),
                    Card = int.Parse(Item.Card),
                    EmrNumber = int.Parse(Item.EmrNumber),
                    Source_ID = source.Id,
                    Source = source,
                };
                await work.transcoderRepository.Add(trans);
                return true;
            }
            throw new ArgumentException("msgavsi arxi ar arsebobs");
        }


        public async Task<TranscoderViewModel> GetDropDownList()
        {
            TranscoderViewModel vw = new TranscoderViewModel();
            return vw;
        }
        public async Task<bool> Delete(int id)
        {
            try
            {
                await work.transcoderRepository.Remove(id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<TranscoderViewModel>> GetAllAsync()
        {
            try
            {
                List<TranscoderViewModel> view = new List<TranscoderViewModel>();
                var trans = await work.transcoderRepository.GetAll();
                foreach (var item in trans)
                {
                    var nk = new TranscoderViewModel()
                    {
                        Card = item.Card.ToString(),
                        Port = item.Port.ToString(),
                        EmrNumber = item.EmrNumber.ToString(),
                        TranscodingFormat = item.Source.ChanellFormat,
                        Id = item.Source.chanell.Name,
                    };
                    view.Add(nk);
                }
                return view;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task Remove(int emrNumber, int card, int port)
        {
            try
            {
                await work.transcoderRepository.Remove(emrNumber, card, port);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<bool> UpdateAsync(TranscoderViewModel item)
        {
            throw new NotImplementedException();
        }
    }
}
