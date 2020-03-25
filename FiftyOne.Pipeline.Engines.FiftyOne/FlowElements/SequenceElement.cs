using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Core.TypedMap;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Engines.FiftyOne.FlowElements
{
    /// <summary>
    /// Sequence element establishes session and sequence evidence in the 
    /// pipeline. If a session id is already in the evidence then the sequence 
    /// is incremented.
    /// </summary>
    public class SequenceElement : FlowElementBase<IElementData, IElementPropertyMetaData>
    {
        private IEvidenceKeyFilter _evidenceKeyFilter = new EvidenceKeyFilterWhitelist(
            new List<string>() {
                Constants.EVIDENCE_SESSIONID,
                Constants.EVIDENCE_SEQUENCE
            });

        private IList<IElementPropertyMetaData> _properties = new List<IElementPropertyMetaData>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger"></param>
        public SequenceElement(ILogger<FlowElementBase<IElementData, IElementPropertyMetaData>> logger) : base(logger)
        {
        }

        /// <summary>
        /// The element data key.
        /// </summary>
        public override string ElementDataKey
        {
            get { return "sequence"; }
        }

        /// <summary>
        /// The evidence key filter.
        /// </summary>
        public override IEvidenceKeyFilter EvidenceKeyFilter
        {
            get { return _evidenceKeyFilter; }
        }

        /// <summary>
        /// The properties populated by this element.
        /// </summary>
        public override IList<IElementPropertyMetaData> Properties => _properties;

        protected override void ManagedResourcesCleanup()
        {
        }

        /// <summary>
        /// Process method checks for presence of a session id and sequence 
        /// number. If they do not exist then they are initialized in evidence.
        /// If they do exist in evidence then the sequence number is incremented
        /// and added back to the evidence.
        /// </summary>
        /// <param name="data"></param>
        protected override void ProcessInternal(IFlowData data)
        {
            var evidence = data.GetEvidence().AsDictionary();

            // If the evidence does not contain a session id then create a new one.
            if (evidence.ContainsKey(FlowElements.Constants.EVIDENCE_SESSIONID) == false)
            {
                data.AddEvidence(FlowElements.Constants.EVIDENCE_SESSIONID, GetNewSessionId());
            }

            // If the evidence does not have a sequence then add one. Otherwise
            // increment it.
            if (evidence.ContainsKey(FlowElements.Constants.EVIDENCE_SEQUENCE) == false)
            {
                data.AddEvidence(FlowElements.Constants.EVIDENCE_SEQUENCE, 1);
            }
            else if (evidence.TryGetValue(FlowElements.Constants.EVIDENCE_SEQUENCE, out object sequence))
            {
                if (sequence is int result || (sequence is string seq && int.TryParse(seq, out result)))
                {
                    data.AddEvidence(FlowElements.Constants.EVIDENCE_SEQUENCE, result + 1);
                }
                else
                {
                    _logger.LogError("Failed to increment usage sequence number.");
                }
            }
            else
            {
                _logger.LogError("Failed to retrieve sequence number.");
            }
        }

        protected override void UnmanagedResourcesCleanup()
        {
        }

        private string GetNewSessionId()
        {
            Guid g = Guid.NewGuid();
            return g.ToString();
        }
    }
}
