using FiftyOne.Pipeline.Core.Data;

namespace FiftyOne.Pipeline.CloudRequestEngine.FailHandling.Failable
{
    internal class FailableEvidenceKeyFilter : IEvidenceKeyFilter, IFailableLazyResult
    {
        private readonly IEvidenceKeyFilter _evidenceKeyFilter;
        private readonly bool _mayBeSaved;

        public FailableEvidenceKeyFilter(IEvidenceKeyFilter evidenceKeyFilter, bool mayBeSaved)
        {
            _evidenceKeyFilter = evidenceKeyFilter;
            _mayBeSaved = mayBeSaved;
        }

        public bool MayBeSaved => _mayBeSaved;

        public bool Include(string key) => _evidenceKeyFilter.Include(key);
        public int? Order(string key) => _evidenceKeyFilter.Order(key);
    }
}
