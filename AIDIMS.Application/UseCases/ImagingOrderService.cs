using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Enums;
using AIDIMS.Domain.Interfaces;

namespace AIDIMS.Application.UseCases;

public class ImagingOrderService : IImagingOrderService
{
    private readonly IImagingOrderRepository _orderRepository;
    private readonly IRepository<PatientVisit> _visitRepository;
    private readonly IRepository<Patient> _patientRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ImagingOrderService(
        IImagingOrderRepository orderRepository,
        IRepository<PatientVisit> visitRepository,
        IRepository<Patient> patientRepository,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _visitRepository = visitRepository;
        _patientRepository = patientRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ImagingOrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (order == null)
        {
            return Result<ImagingOrderDto>.Failure($"Imaging order with ID {id} not found");
        }

        var orderDto = await MapToDto(order, cancellationToken);
        return Result<ImagingOrderDto>.Success(orderDto);
    }

    public async Task<Result<PagedResult<ImagingOrderDto>>> GetAllAsync(
        PaginationParams paginationParams,
        SearchImagingOrderDto filters,
        CancellationToken cancellationToken = default)
    {
        // Get all orders with Studies included
        var orders = await _orderRepository.GetAllWithStudiesAsync(cancellationToken);
        var query = orders.AsEnumerable();

        // Apply filters
        if (filters.VisitId.HasValue)
        {
            query = query.Where(o => o.VisitId == filters.VisitId.Value);
        }

        if (filters.RequestingDoctorId.HasValue)
        {
            query = query.Where(o => o.RequestingDoctorId == filters.RequestingDoctorId.Value);
        }

        if (filters.PatientId.HasValue)
        {
            // Need to filter by patient - get all visits for this patient first
            var patientVisits = await _visitRepository.FindAsync(
                v => v.PatientId == filters.PatientId.Value,
                cancellationToken);
            var visitIds = patientVisits.Select(v => v.Id).ToHashSet();
            query = query.Where(o => visitIds.Contains(o.VisitId));
        }

        if (!string.IsNullOrWhiteSpace(filters.Modality))
        {
            if (Enum.TryParse<Modality>(filters.Modality, true, out var modalityEnum))
            {
                query = query.Where(o => o.ModalityRequested == modalityEnum);
            }
            else
            {
                query = Enumerable.Empty<ImagingOrder>();
            }
        }

        if (!string.IsNullOrWhiteSpace(filters.BodyPart))
        {
            if (Enum.TryParse<BodyPart>(filters.BodyPart, true, out var bodyPartEnum))
            {
                query = query.Where(o => o.BodyPartRequested == bodyPartEnum);
            }
            else
            {
                query = Enumerable.Empty<ImagingOrder>();
            }
        }

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            if (Enum.TryParse<ImagingOrderStatus>(filters.Status, true, out var statusEnum))
            {
                query = query.Where(o => o.Status == statusEnum);
            }
            else
            {
                query = Enumerable.Empty<ImagingOrder>();
            }
        }

        if (filters.FromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= filters.FromDate.Value);
        }

        if (filters.ToDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= filters.ToDate.Value);
        }

        var totalCount = query.Count();

        var pagedOrders = query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();

        var orderDtos = new List<ImagingOrderDto>();
        foreach (var order in pagedOrders)
        {
            var dto = await MapToDto(order, cancellationToken);
            orderDtos.Add(dto);
        }

        var pagedResult = new PagedResult<ImagingOrderDto>
        {
            Items = orderDtos,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<ImagingOrderDto>>.Success(pagedResult);
    }

    public async Task<Result<ImagingOrderDto>> CreateAsync(CreateImagingOrderDto dto, CancellationToken cancellationToken = default)
    {
        // Validate visit exists
        var visit = await _visitRepository.GetByIdAsync(dto.VisitId, cancellationToken);
        if (visit == null)
        {
            return Result<ImagingOrderDto>.Failure($"Patient visit with ID {dto.VisitId} not found");
        }

        // Validate doctor exists
        var doctor = await _userRepository.GetByIdAsync(dto.RequestingDoctorId, cancellationToken);
        if (doctor == null)
        {
            return Result<ImagingOrderDto>.Failure($"Doctor with ID {dto.RequestingDoctorId} not found");
        }

        // Parse modality
        if (!Enum.TryParse<Modality>(dto.ModalityRequested, true, out var modality))
        {
            return Result<ImagingOrderDto>.Failure($"Invalid modality: {dto.ModalityRequested}");
        }

        // Parse body part
        if (!Enum.TryParse<BodyPart>(dto.BodyPartRequested, true, out var bodyPart))
        {
            return Result<ImagingOrderDto>.Failure($"Invalid body part: {dto.BodyPartRequested}");
        }

        var order = new ImagingOrder
        {
            VisitId = dto.VisitId,
            RequestingDoctorId = dto.RequestingDoctorId,
            ModalityRequested = modality,
            BodyPartRequested = bodyPart,
            ReasonForStudy = dto.ReasonForStudy,
            Status = ImagingOrderStatus.Pending
        };

        var createdOrder = await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (visit.Status != PatientVisitStatus.Inprogress)
        {
            visit.Status = PatientVisitStatus.Inprogress;
            await _visitRepository.UpdateAsync(visit, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var orderDto = await MapToDto(createdOrder, cancellationToken);
        return Result<ImagingOrderDto>.Success(orderDto);
    }

    public async Task<Result<ImagingOrderDto>> UpdateAsync(Guid id, UpdateImagingOrderDto dto, CancellationToken cancellationToken = default)
    {
        var existingOrder = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (existingOrder == null)
        {
            return Result<ImagingOrderDto>.Failure($"Imaging order with ID {id} not found");
        }

        // Update modality if provided
        if (!string.IsNullOrWhiteSpace(dto.ModalityRequested))
        {
            if (Enum.TryParse<Modality>(dto.ModalityRequested, true, out var modality))
            {
                existingOrder.ModalityRequested = modality;
            }
            else
            {
                return Result<ImagingOrderDto>.Failure($"Invalid modality: {dto.ModalityRequested}");
            }
        }

        // Update body part if provided
        if (!string.IsNullOrWhiteSpace(dto.BodyPartRequested))
        {
            if (Enum.TryParse<BodyPart>(dto.BodyPartRequested, true, out var bodyPart))
            {
                existingOrder.BodyPartRequested = bodyPart;
            }
            else
            {
                return Result<ImagingOrderDto>.Failure($"Invalid body part: {dto.BodyPartRequested}");
            }
        }

        // Update reason if provided
        if (dto.ReasonForStudy != null)
        {
            existingOrder.ReasonForStudy = dto.ReasonForStudy;
        }

        // Update status if provided
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (Enum.TryParse<ImagingOrderStatus>(dto.Status, true, out var status))
            {
                existingOrder.Status = status;
            }
            else
            {
                return Result<ImagingOrderDto>.Failure($"Invalid status: {dto.Status}");
            }
        }

        await _orderRepository.UpdateAsync(existingOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var orderDto = await MapToDto(existingOrder, cancellationToken);
        return Result<ImagingOrderDto>.Success(orderDto);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingOrder = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (existingOrder == null)
        {
            return Result.Failure($"Imaging order with ID {id} not found");
        }

        await _orderRepository.DeleteAsync(existingOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<ImagingOrderDto> MapToDto(ImagingOrder order, CancellationToken cancellationToken)
    {
        var dto = new ImagingOrderDto
        {
            Id = order.Id,
            VisitId = order.VisitId,
            RequestingDoctorId = order.RequestingDoctorId,
            ModalityRequested = order.ModalityRequested.ToString(),
            BodyPartRequested = order.BodyPartRequested.ToString(),
            ReasonForStudy = order.ReasonForStudy,
            Status = order.Status.ToString(),
            StudyId = order.Studies?.FirstOrDefault()?.Id,
            CreatedAt = order.CreatedAt
        };

        // Get visit and patient info
        var visit = await _visitRepository.GetByIdAsync(order.VisitId, cancellationToken);
        if (visit != null)
        {
            dto.PatientId = visit.PatientId;
            var patient = await _patientRepository.GetByIdAsync(visit.PatientId, cancellationToken);
            dto.PatientName = patient?.FullName ?? "Unknown";
        }

        // Get doctor info
        var doctor = await _userRepository.GetByIdAsync(order.RequestingDoctorId, cancellationToken);
        dto.RequestingDoctorName = GetDoctorFullName(doctor);

        return dto;
    }

    private static string GetDoctorFullName(User? user)
    {
        if (user == null) return "Unknown";

        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.Username : fullName;
    }
}
