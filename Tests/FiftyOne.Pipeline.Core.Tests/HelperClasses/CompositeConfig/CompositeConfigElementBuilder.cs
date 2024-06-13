using FiftyOne.Pipeline.Core.FlowElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiftyOne.Pipeline.Core.Tests.HelperClasses.CompositeConfig
{
    public class CompositeConfigElementBuilder: ICompositeConfigFragmentContainer
    {
        private CompositeConfigFragment _configFragment = new CompositeConfigFragment();

        public IFlowElement Build()
        {
            return new CompositeConfigElement(_configFragment.Number, _configFragment.Text);
        }

        public CompositeConfigFragment getOrMakeCompositeConfigFragment() => _configFragment;
    }
}
