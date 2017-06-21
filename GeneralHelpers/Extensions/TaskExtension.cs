using System;
using System.Threading.Tasks;

namespace Extensions
{
    public static class TaskExtension
    {
        //https://stackoverflow.com/questions/22864367/fire-and-forget-approach
        public static async Task Forget(this Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                throw;
            }
        }
    }
}
