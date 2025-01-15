# Game Server

此專案由 Template 產生，建立一個基本的 Web 遊戲伺服器。

專案建立後，請依照需求修改專案中的程式碼。

以下是必須修改的部分：

1. 修改 `appsettings.json` 中的資料庫連線字串
2. 建立 database migration
    ```bash
    dotnet ef migrations add Initial
    ```
3. 更新資料庫
    ```bash
    dotnet ef database update
    ```
4. `AddGameServer` 的方法中 `dbContextOptionsAction` 參數並沒有實作，請依照需求修改。
   - ex. 選擇使用哪種資料庫、設定使用的連線資訊
