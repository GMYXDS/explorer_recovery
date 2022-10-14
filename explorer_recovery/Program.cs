namespace explorer_recovery {
    internal static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        public static EventWaitHandle ProgramStarted;

        [STAThread]
        static void Main() {
            // ���Դ���һ�������¼�
            bool createNew;
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "explorer_recovery", out createNew);
            // ����������¼��Ѿ�����(������ǰһ������ʵ��)�����¼�֪ͨ���˳�
            if (!createNew) { ProgramStarted.Set(); return; }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}