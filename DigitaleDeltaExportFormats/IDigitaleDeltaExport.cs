namespace DigitaleDeltaExportFormats;

using DatabaseModel.Models;
using DigitaleDelta;
using Observation = DatabaseModel.Models.Observation;

public interface IDigitaleDeltaExport<T> where T: IBaseResponseObject
{
	Task<IQueryable<T>> GenerateExportDataAsync(List<Observation> data, Dictionary<Guid, DatabaseModel.Models.Reference> references, ILookup<Guid, DatabaseModel.Models.RelatedObservation>? relatedObservations, 
		bool includeRelated, ILookup<Guid, GuidRole> relatedReferences);
}