namespace Cartographer.Filters.Compound
{
    internal class MethodHasAtLeastTwoParametersAndReturnsVoid : CompoundMethodFilter
    {
        public MethodHasAtLeastTwoParametersAndReturnsVoid()
        {
            AddFilter(new MethodHasAtLeastNParameters(2));
            AddFilter(new MethodReturnsVoid());
        }
    }
}
