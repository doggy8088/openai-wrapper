using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 1 && (args[0] == "/h" || args[0] == "-h" || args[0] == "--help"))
        {
            Console.WriteLine("Usage: openai-wrapper.exe <inputFilePath> <outputFileName> <type>\n\n" +
                              "<inputFilePath>: 必須提供的輸入檔案路徑。\n" +
                              "<outputFileName>: 必須提供的輸出檔案名稱。\n" +
                              "<type>: 必須提供的操作類型，其值可以是 translate 或 optimize。\n" +
                              "\n範例:\n" +
                              "openai-wrapper.exe input.txt output.txt translate\n" +
                              "openai-wrapper.exe input.txt output.txt optimize");
            return;
        }

        if (args.Length < 2)
        {
            Console.WriteLine("請提供至少兩個參數：<inputFilePath> 和 <outputFileName>。");
            return;
        }

        if (args.Length < 3)
        {
            Console.WriteLine("請提供第三個參數 <type>，其值可以是 translate 或 optimize。");
            return;
        }

        string filePath = args[0];
        if (!File.Exists(filePath))
        {
            Console.WriteLine("檔案不存在：" + filePath);
            return;
        }

        string outputPath = args[1];

        if (Path.GetDirectoryName(outputPath) != string.Empty)
        {
            Console.WriteLine("輸出路徑只能包含檔名，不能包含路徑。");
            return;
        }

        // 將 outputPath 轉換為 %TEMP% 資料夾下的絕對路徑
        outputPath = Path.Combine(Environment.GetEnvironmentVariable("TEMP")!, outputPath);

        string type = args[2].ToLower();
        if (type != "translate" && type != "optimize")
        {
            Console.WriteLine("<type> 參數必須是 translate 或 optimize。");
            return;
        }

        string userPrompt = await File.ReadAllTextAsync(filePath);

        if (type == "optimize")
        {
            try
            {
                string optimizedContent = await OptimizeContentAsync(userPrompt);
                // Console.WriteLine("優化後的內容：\n" + optimizedContent);
                await File.WriteAllTextAsync(outputPath, optimizedContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"發生錯誤: {ex.Message}");
            }
            return;
        }

        try
        {
            // Console.WriteLine($"傳入的文字: {userPrompt}");
            string detectedLanguage = await DetectLanguageAsync(userPrompt);
            // Console.WriteLine($"檢測到的語言: {detectedLanguage}");

            string translatedText = await TranslateAsync(userPrompt, detectedLanguage);
            // Console.WriteLine($"翻譯結果: {translatedText}");
            Console.WriteLine($"{translatedText}");

            await File.WriteAllTextAsync(outputPath, translatedText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"發生錯誤: {ex.Message}");
        }
    }

    private static async Task<string> DetectLanguageAsync(string text)
    {
        // 如果文字全部都在 ASCII 範圍內，直接回傳 "en"
        if (text.All(c => c <= 127))
        {
            return "en";
        }

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("""
                Determine if the provided user input is composed of Chinese characters or English text, and return a language code accordingly.

                - If the input contains Chinese characters, return "zh".
                - If the input contains only English characters and no Chinese characters, return "en".
                - Assume inputs are singularly composed of either Chinese or English, not mixed.

                # Output Format

                Return a single string: either "zh" for Chinese or "en" for English.

                # Examples

                **Example 1:**

                - **Input:** 你好
                - **Output:** zh

                **Example 2:**

                - **Input:** Hello
                - **Output:** en

                **Example 3:**

                - **Input:** 早上好
                - **Output:** zh

                **Example 4:**

                - **Input:** Good morning
                - **Output:** en
                """),
            new UserChatMessage($"""
                Here is the text to be analyzed:

                {text}
                """)
        };

        return await GetChatCompletionAsync(messages);
    }

    private static async Task<string> TranslateAsync(string text, string lang)
    {
        string srcLang = lang == "zh" ? "#zh-tw" : "English";
        string targetLang = lang == "zh" ? "English" : "#zh-tw";
        string additionalPrompt = "";

        if (targetLang == "English")
        {
            additionalPrompt = """
                Some of the proper nouns should not be translated:
                - Will 保哥 = Will Huang


                """;
        }

        if (targetLang == "#zh-tw")
        {
            additionalPrompt = """
                Never say create as 創建, use 建立 instead.
                Never say quality as 質量, use 品質 instead.

                Some of the proper nouns should not be translated:
                - Phi-3
                - Gemma
                - Gemini
                - Cookbook

                Use the following terms mapping rules:
                - information = 資訊
                - message = 訊息
                - store = 儲存
                - search = 搜尋
                - view = 檢視, 檢視表 (No 視圖 as always)
                - data = 資料
                - object = 物件
                - queue = 佇列
                - stack = 堆疊
                - invocation = 呼叫
                - code = 程式碼
                - running = 執行
                - building = 建構
                - package = 套件
                - audio = 音訊
                - video = 影片
                - class = 類別
                - library = 函式庫
                - component = 元件
                - Transaction = 交易
                - Scalability = 延展性
                - Metadata =  Metadata
                - Clone = 複製
                - Memory = 記憶體
                - Built-in = 內建
                - Global = 全域
                - Compatibility = 相容性
                - Function = 函式
                - example = 範例
                - realtime = 即時
                - file = 檔案
                - document = 文件
                - integration = 整合
                - plugin = 外掛
                - asynchronous = 非同步
                - coding = programming = 程式設計
                - OS = operating system = 作業系統
                - thread = 執行緒
                - variable = 變數
                - byte = 字節 = 位元組
                - cluster = 叢集


                """;
        }

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage($"Translate the following {srcLang} text into {targetLang} while preserving the original meaning. Only return the translation result without adding any additional content."),
            new UserChatMessage($"""
                {additionalPrompt}Here is text to be translated:

                {text}
                """)
        };

        return await GetChatCompletionAsync(messages);
    }

    private static async Task<string> OptimizeContentAsync(string content)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("Please optimize the following content to ensure clarity of meaning and a reasonable structure:"),
            new UserChatMessage(content)
        };

        return await GetChatCompletionAsync(messages);
    }

    private static async Task<string> GetChatCompletionAsync(List<ChatMessage> messages)
    {
        string? apiKey = Environment.GetEnvironmentVariable("AOAI_API_KEY");
        string? endpoint = Environment.GetEnvironmentVariable("AOAI_API_BASE");

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException("請確保所有環境變數都已設定。");
        }

        AzureKeyCredential credential = new AzureKeyCredential(apiKey);
        AzureOpenAIClient azureClient = new(new Uri(endpoint), credential);
        ChatClient chatClient = azureClient.GetChatClient("gpt-4o-mini");

        var options = new ChatCompletionOptions
        {
            Temperature = (float)0.3,
            MaxOutputTokenCount = 800,
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);
        stopwatch.Stop();

        if (completion.Content != null && completion.Content.Count > 0)
        {
            return completion.Content[0].Text;
        }

        return string.Empty;
    }
}
