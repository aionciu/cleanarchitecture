using Application.Commands.CreateTransaction;
using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

/// <summary>
/// AutoMapper profile for transaction-related mappings.
/// </summary>
public class TransactionMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionMappingProfile"/> class.
    /// </summary>
    public TransactionMappingProfile()
    {
        // Map CreateTransactionCommand to Transaction entity
        // Note: The Transaction constructor will handle validation and set defaults
        // (Status = Pending, Id = Guid.NewGuid(), CreatedAt, UpdatedAt)
        // Ignore all members to ensure constructor is used exclusively
        CreateMap<CreateTransactionCommand, Transaction>()
            .ConstructUsing(cmd => new Transaction(
                cmd.Amount,
                cmd.Currency,
                cmd.Iban,
                cmd.MerchantId,
                cmd.CustomerId,
                cmd.TransactionDate))
            .ForAllMembers(opt => opt.Ignore());

        // Map Transaction entity to TransactionDto
        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
            .ForMember(dest => dest.Iban, opt => opt.MapFrom(src => src.Iban))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.MerchantId, opt => opt.MapFrom(src => src.MerchantId))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.TransactionDate, opt => opt.MapFrom(src => src.TransactionDate))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}

