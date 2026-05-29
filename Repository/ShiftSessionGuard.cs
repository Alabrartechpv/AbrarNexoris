using ModelClass;

namespace Repository
{
    public static class ShiftSessionGuard
    {
        public static bool CanDoTransaction(out string errorMessage)
        {
            if (!SessionContext.CanDoTransaction(out errorMessage))
                return false;

            using (var sessionRepo = new ShiftSessionRepo())
            {
                return sessionRepo.IsCurrentSessionOpen(out errorMessage);
            }
        }
    }
}
