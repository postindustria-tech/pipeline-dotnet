using FiftyOne.Pipeline.Core.FlowElements;

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
