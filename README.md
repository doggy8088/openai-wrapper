# openai-wrapper

用來代理 OpenAI 的 API 呼叫，目前僅支援 Azure OpenAI Service 的端點。

## Usage

```bat
openai-wrapper <user-prompt-file.txt>
```

## Requirements

執行前請確保以下環境變數已設定：

1. `AOAI_API_BASE`

    例如: `https://<resource-name>.openai.azure.com`

2. `AOAI_API_KEY`

    Azure OpenAI Service 的 API 金鑰。

## 注意事項

- 目前 Deployment Name 寫死為 `gpt-4o-mini`。