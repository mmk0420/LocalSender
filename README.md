# LocalSender

## 🛠 Настройка сети

Для работы обмена файлами необходимо открыть порт `8085`.

> [!IMPORTANT]
> Запустите терминал от имени **Администратора**.

```powershell
netsh advfirewall firewall add rule name="LocalFileShare" dir=in action=allow protocol=TCP localport=8085
