namespace Common.Domain.Interface;

public interface IBackupService
{
    Task<bool> Start();
    bool IsTimeForBackup();
}
