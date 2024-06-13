namespace FiftyOne.Pipeline.Core.Tests.HelperClasses.CompositeConfig
{
    public interface ICompositeConfigFragmentContainer
    {
        CompositeConfigFragment getOrMakeCompositeConfigFragment();
    }

    public static class CompositeConfigFragmentExtensions
    {
        public static T SetNumber<T>(this T builder, int number) where T : ICompositeConfigFragmentContainer
        {
            builder.getOrMakeCompositeConfigFragment().Number = number;
            return builder;
        }

        public static T SetText<T>(this T builder, string text) where T : ICompositeConfigFragmentContainer
        {
            builder.getOrMakeCompositeConfigFragment().Text = text;
            return builder;
        }
    }
}
