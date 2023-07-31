CREATE TABLE [ConnectionHistory](
  [Id] INTEGER PRIMARY KEY ASC AUTOINCREMENT, 
  [InterfaceGuid] GUID NOT NULL, 
  [InterfaceName] VARCHAR(10) NOT NULL, 
  [SSID] VARCHAR(20) NOT NULL, 
  [ConnectedAt] DATETIME NOT NULL, 
  [ConnectedUntil] DATETIME
);
