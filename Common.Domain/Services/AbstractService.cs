using AutoMapper;
using Common.Data.Interfaces;

namespace Common.Domain.Services;

public abstract class AbstractService
{
    protected readonly IMapper maper;
    protected readonly IUniteOfWork work;

    protected AbstractService(IMapper map, IUniteOfWork wor)
    {
        maper = map;
        work = wor;
    }
}
