namespace Brigadier.Arguments
{
    public abstract class FinalizingArgument<T>
    {
        public abstract T Finalize(CommandSender sender);
    }
}