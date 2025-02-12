# 發行筆記

```sh
dotnet publish -c Release --nologo --self-contained -r win-x64 -p:PublishSingleFile=true -p:DebugType=none
```