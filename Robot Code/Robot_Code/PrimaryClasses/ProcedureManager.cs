namespace PrimaryClasses
{
    public class ProcedureManager
    {
        private static ProcedureManager m_Instance;

        public ProcedureManager GetInstance()
        {
            return m_Instance ??= new ProcedureManager();
        }

        private ProcedureManager()
        {

        }

    }
}