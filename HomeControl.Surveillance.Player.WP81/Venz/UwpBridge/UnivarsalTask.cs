namespace System.Threading.Tasks
{
    public static class UniversalTask
    {
        #if WP81
        private static Task Instance = Task.FromResult<Object>(null);
        #else
        private static Task Instance = Task.CompletedTask;
        #endif

        public static Task CompletedTask => Instance;
    }
}