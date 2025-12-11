using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Interfaces;
using AutoMapper;

namespace AIDIMS.Application.UseCases;

public class ImageAnnotationService : IImageAnnotationService
{
    private readonly IImageAnnotationRepository _annotationRepository;
    private readonly IDicomInstanceRepository _instanceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ImageAnnotationService(
        IImageAnnotationRepository annotationRepository,
        IDicomInstanceRepository instanceRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _annotationRepository = annotationRepository;
        _instanceRepository = instanceRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ImageAnnotationDto>> CreateAsync(CreateImageAnnotationDto dto, CancellationToken cancellationToken = default)
    {
        // Find instance by OrthancInstanceId or SopInstanceUid
        var instances = await _instanceRepository.GetAllAsync(cancellationToken);
        var instance = instances.FirstOrDefault(i =>
            i.OrthancInstanceId == dto.InstanceId ||
            i.SopInstanceUid == dto.InstanceId);

        if (instance == null)
        {
            return Result<ImageAnnotationDto>.Failure($"DICOM Instance with ID {dto.InstanceId} not found");
        }

        var annotation = new ImageAnnotation
        {
            InstanceId = instance.Id,
            AnnotationType = dto.AnnotationType,
            AnnotationData = dto.AnnotationData
        };

        var createdAnnotation = await _annotationRepository.AddAsync(annotation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var annotationDto = _mapper.Map<ImageAnnotationDto>(createdAnnotation);
        return Result<ImageAnnotationDto>.Success(annotationDto);
    }

    public async Task<Result<ImageAnnotationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var annotation = await _annotationRepository.GetByIdAsync(id, cancellationToken);

        if (annotation == null)
        {
            return Result<ImageAnnotationDto>.Failure($"Image Annotation with ID {id} not found");
        }

        var annotationDto = _mapper.Map<ImageAnnotationDto>(annotation);
        return Result<ImageAnnotationDto>.Success(annotationDto);
    }

    public async Task<Result<PagedResult<ImageAnnotationDto>>> GetAllAsync(
        PaginationParams paginationParams,
        SearchImageAnnotationDto filters,
        CancellationToken cancellationToken = default)
    {
        var annotations = await _annotationRepository.GetAllAsync(cancellationToken);
        var query = annotations.AsEnumerable();

        if (filters.InstanceId.HasValue)
        {
            query = query.Where(a => a.InstanceId == filters.InstanceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.AnnotationType))
        {
            var annotationType = filters.AnnotationType.Trim();
            query = query.Where(a => a.AnnotationType == annotationType);
        }

        var totalCount = query.Count();

        var pagedAnnotations = query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();

        var pagedAnnotationDtos = _mapper.Map<List<ImageAnnotationDto>>(pagedAnnotations);

        var pagedResult = new PagedResult<ImageAnnotationDto>
        {
            Items = pagedAnnotationDtos,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<ImageAnnotationDto>>.Success(pagedResult);
    }

    public async Task<Result<ImageAnnotationDto>> UpdateAsync(Guid id, UpdateImageAnnotationDto dto, CancellationToken cancellationToken = default)
    {
        var existingAnnotation = await _annotationRepository.GetByIdAsync(id, cancellationToken);

        if (existingAnnotation == null)
        {
            return Result<ImageAnnotationDto>.Failure($"Image Annotation with ID {id} not found");
        }

        _mapper.Map(dto, existingAnnotation);
        await _annotationRepository.UpdateAsync(existingAnnotation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var annotationDto = _mapper.Map<ImageAnnotationDto>(existingAnnotation);
        return Result<ImageAnnotationDto>.Success(annotationDto);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingAnnotation = await _annotationRepository.GetByIdAsync(id, cancellationToken);

        if (existingAnnotation == null)
        {
            return Result.Failure($"Image Annotation with ID {id} not found");
        }

        await _annotationRepository.DeleteAsync(existingAnnotation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<ImageAnnotationDto>>> GetByInstanceIdAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        // Find instance by OrthancInstanceId or SopInstanceUid
        var instances = await _instanceRepository.GetAllAsync(cancellationToken);
        var instance = instances.FirstOrDefault(i =>
            i.OrthancInstanceId == instanceId ||
            i.SopInstanceUid == instanceId);

        if (instance == null)
        {
            return Result<IEnumerable<ImageAnnotationDto>>.Success(new List<ImageAnnotationDto>());
        }

        var annotations = await _annotationRepository.GetByInstanceIdAsync(instance.Id, cancellationToken);
        var annotationDtos = _mapper.Map<List<ImageAnnotationDto>>(annotations);
        return Result<IEnumerable<ImageAnnotationDto>>.Success(annotationDtos);
    }
}

