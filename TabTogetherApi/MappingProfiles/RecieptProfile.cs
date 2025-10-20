using AutoMapper;

namespace TabTogetherApi.MappingProfiles
{
    public class RecieptProfile : Profile
    {
        public RecieptProfile()
        {
            CreateMap<Entities.Receipt, Models.ReceiptDto>();
            CreateMap<Entities.ReceiptItem, Models.ReceiptItemDto>();

        }

    }
}
