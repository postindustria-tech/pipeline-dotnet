/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2020 51 Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY.
 *
 * This Original Work is licensed under the European Union Public Licence (EUPL) 
 * v.1.2 and is subject to its terms as set out below.
 *
 * If a copy of the EUPL was not distributed with this file, You can obtain
 * one at https://opensource.org/licenses/EUPL-1.2.
 *
 * The 'Compatible Licences' set out in the Appendix to the EUPL (as may be
 * amended by the European Commission) shall be deemed incompatible for
 * the purposes of the Work and the provisions of the compatibility
 * clause in Article 5 of the EUPL shall not apply.
 * 
 * If using the Work as, or as part of, a network application, by 
 * including the attribution notice(s) required under Article 5 of the EUPL
 * in the end user terms of the application under an appropriate heading, 
 * such notice(s) shall fulfill the requirements of that article.
 * ********************************************************************* */

using FiftyOne.Pipeline.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FiftyOne.Pipeline.Web.Framework.Providers
{
    /// <summary>
    /// Extends the default HttpBrowserCapabilities implementation to override
    /// and add values using the result of the 51Degrees Pipeline's processing.
    /// </summary>
    public class PipelineCapabilities : HttpBrowserCapabilities
    {
        public readonly IFlowData FlowData;
        private readonly HttpRequest _request;
        private readonly HttpBrowserCapabilities _baseCaps;
        
        public PipelineCapabilities(
            HttpBrowserCapabilities baseCaps,
            HttpRequest request,
            IFlowData flowData) : base()
        {
            _baseCaps = baseCaps;
            _request = request;
            FlowData = flowData;
        }

        private string IterateDown(string[] keys, IDictionary<string, object> dict)
        {
            string[] subKeys = keys.Skip(1).ToArray();
            var nonStringValue = dict.ContainsKey(subKeys[0]) ? dict[subKeys[0]] : null;
            if (nonStringValue == null || nonStringValue is string)
            {
                return nonStringValue as string;
            }
            if (subKeys.Length > 1 && nonStringValue as IDictionary<string, object> != null)
            {
                return IterateDown(subKeys, nonStringValue as IDictionary<string, object>);
            }
            else if (nonStringValue as IDictionary<string, object> != null ||
                nonStringValue as IList<object> != null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(nonStringValue);
            }
            else
            {
                return nonStringValue.ToString();
            }
        }

        /// <summary>
        /// Gets the value of the browser capability, first using the processed
        /// FlowData, then the base capabilities if the property is not present.
        /// </summary>
        /// <param name="key">
        /// The name of the browser capability to retrieve.
        /// </param>
        /// <returns>
        /// The browser capability, or element property with the specified key
        /// name.
        /// </returns>
        public override string this[string key]
        {
            get
            {
                string value = null;
                // Try getting the value from the FlowData
                try
                {
                    var availableElements = FlowData.Pipeline.ElementAvailableProperties;
                    if (key.Contains("."))
                    {
                        var keys = key.Split('.');
                        if (availableElements.ContainsKey(keys[0]) &&
                            availableElements[keys[0]].ContainsKey(keys[1]) &&
                            availableElements[keys[0]][keys[1]].Available)
                        {
                            var nonStringValue = FlowData.Get(keys[0])[keys[1]];
                            if (nonStringValue as string != null)
                            {
                                value = nonStringValue as string;
                            }
                            else if (nonStringValue as IDictionary<string, object> != null &&
                                keys.Length > 2)
                            {
                                value = IterateDown(keys.Skip(1).ToArray(), nonStringValue as IDictionary<string, object>);
                            }
                            else if (nonStringValue as IDictionary<string, object> != null ||
                                nonStringValue as IList<object> != null ){
                                value = Newtonsoft.Json.JsonConvert.SerializeObject(nonStringValue);
                            }
                            else
                            {
                                value = nonStringValue.ToString();
                            }
                        }
                    }
                    else if (availableElements.Any(i => i.Value.ContainsKey(key)))
                    {
                        var elementKey = availableElements
                            .First(i => i.Value.ContainsKey(key))
                            .Key;
                        var valueObj = FlowData.Get(elementKey)[key];

                        if (availableElements[elementKey][key].Type.Equals(typeof(IReadOnlyList<string>)))
                        {
                            value = string.Join("|", ((IEnumerable<object>)valueObj).Select(i => i.ToString()));
                        }
                        else
                        {
                            value = valueObj.ToString();
                        }
                    }
                }
                catch (Exception e)
                {
                    // There was an exception, ensure the value is null so it
                    // will be retrieved from the base class.
                    value = null;
                }

                // Return the value that was just fetched, or fetch it from the
                // base class if it was not found.
                return value == null ? _baseCaps[key] : value;
            }
        }

        public override bool IsMobileDevice
        {
            get
            {
                if (FlowData.Pipeline.ElementAvailableProperties.ContainsKey("device") &&
                    FlowData.Pipeline.ElementAvailableProperties["device"].ContainsKey("ismobile"))
                {
                    return FlowData.GetAsBool("ismobile");
                }
                else
                {
                    return _baseCaps.IsMobileDevice;
                }
            }
        }

        public override bool CanInitiateVoiceCall
        {
            get
            {
                if (FlowData.Pipeline.ElementAvailableProperties.ContainsKey("device") &&
                    FlowData.Pipeline.ElementAvailableProperties["device"].ContainsKey("supportsphonecalls"))
                {
                    return FlowData.GetAsBool("supportsphonecalls");
                }
                else
                {
                    return _baseCaps.CanInitiateVoiceCall;
                }
            }
        }

        public override string MobileDeviceManufacturer
        {
            get
            {
                if (FlowData.Pipeline.ElementAvailableProperties.ContainsKey("device") &&
                    FlowData.Pipeline.ElementAvailableProperties["device"].ContainsKey("oem"))
                {
                    return FlowData.GetAsString("oem");
                }
                else
                {
                    return _baseCaps.MobileDeviceManufacturer;
                }
            }
        }

        public override string MobileDeviceModel
        {
            get
            {
                if (FlowData.Pipeline.ElementAvailableProperties.ContainsKey("device") &&
                    FlowData.Pipeline.ElementAvailableProperties["device"].ContainsKey("hardwaremodel"))
                {
                    return FlowData.GetAsString("hardwaremodel");
                }
                else
                {
                    return _baseCaps.MobileDeviceModel;
                }
            }
        }

        public override int ScreenPixelsHeight {
            get
            {
                if (FlowData.Pipeline.ElementAvailableProperties.ContainsKey("device") &&
                    FlowData.Pipeline.ElementAvailableProperties["device"].ContainsKey("screenpixelsheight"))
                {
                    return FlowData.GetAsInt("screenpixelsheight");
                }
                else
                {
                    return _baseCaps.ScreenPixelsHeight;
                }
            }
        }

        public override int ScreenPixelsWidth
        {
            get
            {
                if (FlowData.Pipeline.ElementAvailableProperties.ContainsKey("device") &&
                    FlowData.Pipeline.ElementAvailableProperties["device"].ContainsKey("screenpixelswidth"))
                {
                    return FlowData.GetAsInt("screenpixelswidth");
                }
                else
                {
                    return _baseCaps.ScreenPixelsWidth;
                }
            }
        }

        #region Unaltered Properties
            
        public override int DefaultSubmitButtonLimit { get { return _baseCaps.DefaultSubmitButtonLimit; } }
        public override bool CanCombineFormsInDeck { get { return _baseCaps.CanCombineFormsInDeck; } }
        public override bool CanRenderAfterInputOrSelectElement { get { return _baseCaps.CanRenderAfterInputOrSelectElement; } }
        public override bool CanRenderEmptySelects { get { return _baseCaps.CanRenderEmptySelects; } }
        public override bool CanRenderInputAndSelectElementsTogether { get { return _baseCaps.CanRenderInputAndSelectElementsTogether; } }
        public override bool CanRenderMixedSelects { get { return _baseCaps.CanRenderMixedSelects; } }
        public override bool CanRenderOneventAndPrevElementsTogether { get { return _baseCaps.CanRenderOneventAndPrevElementsTogether; } }
        public override bool CanRenderPostBackCards { get { return _baseCaps.CanRenderPostBackCards; } }
        public override bool CanRenderSetvarZeroWithMultiSelectionList { get { return _baseCaps.CanRenderSetvarZeroWithMultiSelectionList; } }
        public override bool CanSendMail { get { return _baseCaps.CanSendMail; } }
        public override int GatewayMajorVersion { get { return _baseCaps.GatewayMajorVersion; } }
        public override double GatewayMinorVersion { get { return _baseCaps.GatewayMinorVersion; } }
        public override string GatewayVersion { get { return _baseCaps.GatewayVersion; } }
        public override bool HasBackButton { get { return _baseCaps.HasBackButton; } }
        public override bool HidesRightAlignedMultiselectScrollbars { get { return _baseCaps.HidesRightAlignedMultiselectScrollbars; } }
        public override string InputType { get { return _baseCaps.InputType; } }
        public override int MaximumHrefLength { get { return _baseCaps.MaximumHrefLength; } }
        public override int MaximumRenderedPageSize { get { return _baseCaps.MaximumRenderedPageSize; } }
        public override int MaximumSoftkeyLabelLength { get { return _baseCaps.MaximumSoftkeyLabelLength; } }
        public override int NumberOfSoftkeys { get { return _baseCaps.NumberOfSoftkeys; } }
        public override string PreferredRenderingMime { get { return _baseCaps.PreferredRenderingMime; } }
        public override string PreferredRenderingType { get { return _baseCaps.PreferredRenderingType; } }
        public override string PreferredRequestEncoding { get { return _baseCaps.PreferredRequestEncoding; } }
        public override string PreferredResponseEncoding { get { return _baseCaps.PreferredResponseEncoding; } }
        public override bool RendersBreakBeforeWmlSelectAndInput { get { return _baseCaps.RendersBreakBeforeWmlSelectAndInput; } }
        public override bool RendersBreaksAfterHtmlLists { get { return _baseCaps.RendersBreaksAfterHtmlLists; } }
        public override bool RendersBreaksAfterWmlAnchor { get { return _baseCaps.RendersBreaksAfterWmlAnchor; } }
        public override bool RendersBreaksAfterWmlInput { get { return _baseCaps.RendersBreaksAfterWmlInput; } }
        public override bool RendersWmlDoAcceptsInline { get { return _baseCaps.RendersWmlDoAcceptsInline; } }
        public override bool RendersWmlSelectsAsMenuCards { get { return _baseCaps.RendersWmlSelectsAsMenuCards; } }
        public override string RequiredMetaTagNameValue { get { return _baseCaps.RequiredMetaTagNameValue; } }
        public override bool RequiresAttributeColonSubstitution { get { return _baseCaps.RequiresAttributeColonSubstitution; } }
        public override bool RequiresContentTypeMetaTag { get { return _baseCaps.RequiresContentTypeMetaTag; } }
        public override bool RequiresDBCSCharacter { get { return _baseCaps.RequiresDBCSCharacter; } }
        public override bool RequiresHtmlAdaptiveErrorReporting { get { return _baseCaps.RequiresHtmlAdaptiveErrorReporting; } }
        public override bool RequiresLeadingPageBreak { get { return _baseCaps.RequiresLeadingPageBreak; } }
        public override bool RequiresNoBreakInFormatting { get { return _baseCaps.RequiresNoBreakInFormatting; } }
        public override bool RequiresOutputOptimization { get { return _baseCaps.RequiresOutputOptimization; } }
        public override bool RequiresPhoneNumbersAsPlainText { get { return _baseCaps.RequiresPhoneNumbersAsPlainText; } }
        public override bool RequiresSpecialViewStateEncoding { get { return _baseCaps.RequiresSpecialViewStateEncoding; } }
        public override bool RequiresUniqueFilePathSuffix { get { return _baseCaps.RequiresUniqueFilePathSuffix; } }
        public override bool RequiresUniqueHtmlCheckboxNames { get { return _baseCaps.RequiresUniqueHtmlCheckboxNames; } }
        public override bool RequiresUniqueHtmlInputNames { get { return _baseCaps.RequiresUniqueHtmlInputNames; } }
        public override bool RequiresUrlEncodedPostfieldValues { get { return _baseCaps.RequiresUrlEncodedPostfieldValues; } }
        public override int ScreenCharactersHeight { get { return _baseCaps.ScreenCharactersHeight; } }
        public override int ScreenCharactersWidth { get { return _baseCaps.ScreenCharactersWidth; } }
        public override bool SupportsAccesskeyAttribute { get { return _baseCaps.SupportsAccesskeyAttribute; } }
        public override bool SupportsBodyColor { get { return _baseCaps.SupportsBodyColor; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsBold { get { return _baseCaps.SupportsBold; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsCacheControlMetaTag { get { return _baseCaps.SupportsCacheControlMetaTag; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsCss { get { return _baseCaps.SupportsCss; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsDivAlign { get { return _baseCaps.SupportsDivAlign; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsDivNoWrap { get { return _baseCaps.SupportsDivNoWrap; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsEmptyStringInCookieValue { get { return _baseCaps.SupportsEmptyStringInCookieValue; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsFontColor { get { return _baseCaps.SupportsFontColor; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsFontName { get { return _baseCaps.SupportsFontName; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsFontSize { get { return _baseCaps.SupportsFontSize; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsImageSubmit { get { return _baseCaps.SupportsImageSubmit; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsIModeSymbols { get { return _baseCaps.SupportsIModeSymbols; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsInputIStyle { get { return _baseCaps.SupportsInputIStyle; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsInputMode { get { return _baseCaps.SupportsInputMode; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsItalic { get { return _baseCaps.SupportsItalic; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsJPhoneMultiMediaAttributes { get { return _baseCaps.SupportsJPhoneMultiMediaAttributes; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsJPhoneSymbols { get { return _baseCaps.SupportsJPhoneSymbols; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsQueryStringInFormAction { get { return _baseCaps.SupportsQueryStringInFormAction; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsRedirectWithCookie { get { return _baseCaps.SupportsRedirectWithCookie; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsSelectMultiple { get { return _baseCaps.SupportsSelectMultiple; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsUncheck { get { return _baseCaps.SupportsUncheck; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsXmlHttp { get { return _baseCaps.SupportsXmlHttp; } }

        #endregion
    }
}
