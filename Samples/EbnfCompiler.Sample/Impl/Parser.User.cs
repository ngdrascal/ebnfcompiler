namespace EbnfCompiler.Sample.Impl
{
   public partial class Parser
   {
      IRootNode BuildRootNode()
      {
         return _astBuilder.RootNode;
      }
   }
}
