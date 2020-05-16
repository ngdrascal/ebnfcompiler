namespace EbnfCompiler.Sample
{
   public partial class Parser
   {
      IRootNode BuildRootNode()
      {
         return _astBuilder.RootNode;
      }
   }
}
