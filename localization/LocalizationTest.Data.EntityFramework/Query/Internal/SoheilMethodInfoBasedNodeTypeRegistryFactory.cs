namespace LocalizationTest.Data.EntityFramework.Query.Internal
{
    using LocalizationTest.Data.EntityFramework.Query.ResultOperators.Internal;

    using Microsoft.EntityFrameworkCore.Query.Internal;

    using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

    public class SoheilMethodInfoBasedNodeTypeRegistryFactory : MethodInfoBasedNodeTypeRegistryFactory
    {
        public SoheilMethodInfoBasedNodeTypeRegistryFactory()
            : base(MethodInfoBasedNodeTypeRegistry.CreateFromRelinqAssembly())
        {
            RegisterMethods(IncludeLocalizationExpressionNode.SupportedMethods, typeof(IncludeLocalizationExpressionNode));
        }
    }
}
